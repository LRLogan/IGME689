using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// Reads and aggregates a traffic count CSV, then applies averaged data to Unity road objects.
/// </summary>
public class RoadCountCSVParser : MonoBehaviour
{
    [Header("CSV Input")]
    [Tooltip("File name (with extension) located in StreamingAssets folder.")]
    public string csvFileName = "TrafficCounts.csv";

    [Header("Dependencies")]
    [Tooltip("Reference to the RoadMapLineBuilder that created the road GameObjects.")]
    public RoadMapLineBuilder roadBuilder;

    [Header("Options")]
    [Tooltip("Normalize CSV road names before matching.")]
    public bool normalizeCSVNames = true;

    // Holds per-day raw data for each CSV road before averaging
    private Dictionary<string, List<float[]>> csvDailyData = new Dictionary<string, List<float[]>>();

    // Holds final averaged hourly values for each road
    private Dictionary<string, float[]> averagedRoadData = new Dictionary<string, float[]>();

    // Holds the successfully matched Unity roads
    public List<RoadData> matchedRoads = new List<RoadData>();

    private void Start()
    {
        if (roadBuilder == null)
        {
            Debug.LogError("RoadCountCSVParser: RoadMapLineBuilder reference not assigned!");
            return;
        }
    }

    /// <summary>
    /// Waits until roadBuilder has generated roads, then parses and applies data.
    /// </summary>
    public IEnumerator WaitAndParse(Action onComplete)
    {
        yield return new WaitUntil(() => roadBuilder.lineArray != null && roadBuilder.lineArray.Count > 0);

        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"CSV file not found at: {path}");
            yield break;
        }

        string csvText = File.ReadAllText(path);

        ParseCSV(csvText);
        ComputeAverages();
        ApplyTrafficData();

        onComplete?.Invoke();
    }

    /// <summary>
    /// Reads CSV lines and groups daily records by road name.
    /// </summary>
    private void ParseCSV(string text)
    {
        csvDailyData.Clear();
        averagedRoadData.Clear();

        List<string> lines = text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (lines.Count <= 1)
        {
            Debug.LogWarning("CSV file appears empty or missing data rows.");
            return;
        }

        for (int i = 1; i < lines.Count; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length < 31) continue; // not enough hourly columns

            string name = row[2].Trim();
            if (normalizeCSVNames)
                name = Normalize(name.ToUpperInvariant());

            float[] hourly = new float[24];
            for (int h = 0; h < 24; h++)
            {
                string raw = row[7 + h].Trim();

                // Remove wrapping quotes and whitespace
                if (raw.StartsWith("\"") && raw.EndsWith("\""))
                    raw = raw.Substring(1, raw.Length - 2);

                // Now try parsing
                if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out float val))
                    hourly[h] = val;
                else
                    hourly[h] = 0f;

            }


            if (!csvDailyData.ContainsKey(name))
                csvDailyData[name] = new List<float[]>();

            csvDailyData[name].Add(hourly);
        }

        Debug.Log($"Parsed {csvDailyData.Count} unique roads from CSV (before averaging).");
    }

    /// <summary>
    /// Averages multiple days of data for each road.
    /// </summary>
    private void ComputeAverages()
    {
        averagedRoadData.Clear();

        foreach (var entry in csvDailyData)
        {
            string roadName = entry.Key;
            List<float[]> dailyRecords = entry.Value;

            float[] averaged = new float[24];

            for (int hour = 0; hour < 24; hour++)
            {
                float sum = 0f;
                int count = 0;

                foreach (var record in dailyRecords)
                {
                    sum += record[hour];
                    count++;
                }

                averaged[hour] = count > 0 ? sum / count : 0f;
            }

            averagedRoadData[roadName] = averaged;
        }

        Debug.Log($"Computed averaged hourly data for {averagedRoadData.Count} roads.");
    }

    /// <summary>
    /// Matches Unity roads to averaged CSV data and assigns hourly counts.
    /// </summary>
    private void ApplyTrafficData()
    {
        if (roadBuilder == null || roadBuilder.lineArray == null || roadBuilder.lineArray.Count == 0)
        {
            Debug.LogError("RoadCountCSVParser: No roads available to match.");
            return;
        }

        matchedRoads.Clear();
        int matched = 0;
        int unmatched = 0;

        // Build a lookup dictionary for Unity roads by name (case-insensitive)
        Dictionary<string, RoadData> unityRoadLookup = new Dictionary<string, RoadData>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var roadObj in roadBuilder.lineArray)
        {
            RoadData data = roadObj.GetComponent<RoadData>();
            if (data == null || string.IsNullOrWhiteSpace(data.roadName))
                continue;

            if (!unityRoadLookup.ContainsKey(data.roadName))
                unityRoadLookup[data.roadName] = data;
        }

        // Match each CSV road to a Unity road
        foreach (var kvp in averagedRoadData)
        {
            string csvRoadName = kvp.Key;
            float[] hourlyCounts = kvp.Value;

            // Attempt direct match against Unity road names
            string match = unityRoadLookup.Keys.FirstOrDefault(uName => csvRoadName.Equals(uName, StringComparison.InvariantCultureIgnoreCase));

            // If not found, try simple partial matching
            if (match == null)
                match = unityRoadLookup.Keys.FirstOrDefault(uName => uName.ToUpperInvariant().Contains(csvRoadName.ToUpperInvariant()) ||
                                                                     csvRoadName.ToUpperInvariant().Contains(uName.ToUpperInvariant()));

            if (match != null)
            {
                Debug.Log($"Match was not null : {match}");
                RoadData data = unityRoadLookup[match];
                if (data.averageCount == null)
                    data.averageCount = new Dictionary<int, float>();
                else
                    data.averageCount.Clear();

                for (int h = 1; h <= 24; h++)
                    data.averageCount[h] = hourlyCounts[h - 1];

                data.congestionValue = hourlyCounts.Average();

                matchedRoads.Add(data);
                matched++;
                Debug.Log($"Applied averageCount to {data.roadName} — Example hour 8: {data.averageCount[8]}");
            }
            else
            {
                unmatched++;
            }

        }

        Debug.Log($"Traffic data applied. Matched: {matched}, Unmatched: {unmatched}");
    }

    /// <summary>
    /// Cleans and normalizes CSV road names for matching.
    /// </summary>
    private string Normalize(string input)
    {
        string name = input.ToLower().Trim();
        name = name.Replace(".", "").Replace(",", "").Replace("  ", " ");

        name = name.Replace(" ave", " avenue")
                   .Replace(" blvd", " boulevard")
                   .Replace(" st", " street")
                   .Replace(" rd", " road")
                   .Replace(" dr", " drive")
                   .Replace(" pl", " place")
                   .Replace(" ln", " lane")
                   .Replace(" ct", " court")
                   .Replace(" sq", " square")
                   .Replace(" ter", " terrace")
                   .Replace(" pkwy", " parkway");

        return name;
    }
}
