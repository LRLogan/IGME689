using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// Reads a traffic count CSV file and assigns data to each RoadData component.
/// </summary>
public class RoadCountCSVParser : MonoBehaviour
{
    [Header("CSV Input")]
    [Tooltip("File name (with extension) located in StreamingAssets folder.")]
    public string csvFileName = "TrafficCounts.csv";

    [Header("Dependencies")]
    [Tooltip("Reference to the RoadMapLineBuilder that created the road GameObjects.")]
    public RoadMapLineBuilder roadBuilder;

    [Header("Matching Settings")]
    [Tooltip("Normalize names to match common suffix differences (Ave -> Avenue, etc.)")]
    public bool normalizeNames = true;

    private Dictionary<string, float[]> csvRoadData;

    private void Start()
    {
        csvRoadData = new Dictionary<string, float[]>();

        if (roadBuilder == null)
        {
            Debug.LogError("RoadCountCSVParser: RoadMapLineBuilder reference not assigned!");
            return;
        }
    }

    public IEnumerator WaitAndParse(Action onComplete)
    {
        // Wait until roads have been generated
        yield return new WaitUntil(() => roadBuilder.lineArray != null && roadBuilder.lineArray.Count > 0);

        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"CSV file not found at: {path}");
            yield break;
        }

        string csvText = File.ReadAllText(path);
        ParseCSV(csvText);
        ApplyToRoads();
        onComplete.Invoke();
    }

    private void ParseCSV(string text)
    {
        List<string> lines = text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (lines.Count <= 1)
        {
            Debug.LogWarning("CSV file appears empty or missing data rows.");
            return;
        }

        for (int i = 1; i < lines.Count; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length < 31) continue; // ID + 6 columns + 24 hourly columns

            string name = row[2].Trim(); // "Roadway" column
            if (normalizeNames) name = Normalize(name);

            float[] hourly = new float[24];
            for (int h = 0; h < 24; h++)
            {
                if (float.TryParse(row[7 + h], NumberStyles.Any, CultureInfo.InvariantCulture, out float val))
                    hourly[h] = val;
            }

            if (!csvRoadData.ContainsKey(name))
                csvRoadData[name] = hourly;
        }

        Debug.Log($"Parsed {csvRoadData.Count} roads from CSV.");
    }

    private void ApplyToRoads()
    {
        double matched = 0;
        double unmatched = 0;

        foreach (var road in roadBuilder.lineArray)
        {
            RoadData data = road.GetComponent<RoadData>();
            if (data == null || string.IsNullOrEmpty(data.roadName))
                continue;

            string matchName = normalizeNames ? Normalize(data.roadName) : data.roadName;

            // Try direct match
            if (csvRoadData.TryGetValue(matchName, out float[] trafficCounts))
            {
                for (int h = 1; h <= 24; h++)
                    data.averageCount[h] = trafficCounts[h - 1];

                data.congestionValue = trafficCounts.Average();
                matched++;
            }
            else
            {
                // Try partial or fuzzy match (basic)
                var possible = csvRoadData.Keys.FirstOrDefault(k => k.Contains(matchName) || matchName.Contains(k));
                if (possible != null)
                {
                    float[] counts = csvRoadData[possible];
                    for (int h = 1; h <= 24; h++)
                        data.averageCount[h] = counts[h - 1];
                    data.congestionValue = counts.Average();
                    matched++;
                }
                else
                {
                    unmatched++;
                }
            }
        }

        Debug.Log($"Traffic data applied. {matched} roads matched, {unmatched} unmatched.");
    }

    private string Normalize(string input)
    {
        string newName = input.ToLower().Trim();
        newName = newName.Replace(".", "").Replace(",", "").Replace("  ", " ");

        // Replace abbreviations
        newName = newName.Replace(" ave", " avenue")
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

        return newName;
    }
}
