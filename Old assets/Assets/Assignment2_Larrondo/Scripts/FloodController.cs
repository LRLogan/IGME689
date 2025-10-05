using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloodController : MonoBehaviour
{
    // World set up
    [SerializeField] private ArcGISLocationComponent ocean;
    [SerializeField] private ArcGISMapComponent mapComponent;
    [SerializeField] private ArcGISLocationComponent locationComponent;
    public Location location;

    // Buttons
    [Header("UI Buttons")]
    [SerializeField] private Button stepForwardBtn;
    [SerializeField] private Button stepBackBtn;
    [SerializeField] private Button nextScenarioBtn;

    // UI Text fields
    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI scenarioText;
    [SerializeField] private TextMeshProUGUI yearText;
    [SerializeField] private TextMeshProUGUI elevationText;

    // Simulation controls
    private List<SeaLevelStationData> scenarios; 
    private int curScenario = 0;
    private int curYearIndex = 0;
    private List<int> yearKeys;

    [System.Serializable]
    public struct Location
    {
        public double longitude;
        public double latitude;
        public double altitude;
    }

    [System.Serializable]
    public struct SeaLevelStationData
    {
        public int objectID;
        public SortedDictionary<int, float> yearsToLevels;
    }

    void Start()
    {
        // Getting the point in lat / long
        ArcGISPoint geoPoint = new ArcGISPoint(
            location.longitude,
            location.latitude,
            location.altitude,
            ArcGISSpatialReference.WGS84()
        );
        mapComponent.OriginPosition = geoPoint;
        locationComponent.Position = geoPoint;

        // Load CSV
        string path = Path.Combine(Application.streamingAssetsPath, "stationdata.csv");
        scenarios = LoadCSV(path);

        // Init first scenario
        SetScenario(0);
        UpdateOceanHeight();

        // Hook up buttons
        stepForwardBtn.onClick.AddListener(StepForward);
        stepBackBtn.onClick.AddListener(StepBack);
        nextScenarioBtn.onClick.AddListener(NextScenario);
    }

    private void SetScenario(int index)
    {
        curScenario = index;
        yearKeys = new List<int>(scenarios[curScenario].yearsToLevels.Keys);
        curYearIndex = 0;
        UpdateOceanHeight();
    }

    private void StepForward()
    {
        if (curYearIndex < yearKeys.Count - 1)
        {
            curYearIndex++;
            UpdateOceanHeight();
        }
    }

    private void StepBack()
    {
        if (curYearIndex > 0)
        {
            curYearIndex--;
            UpdateOceanHeight();
        }
    }

    private void NextScenario()
    {
        int nextIndex = (curScenario + 1) % scenarios.Count;
        SetScenario(nextIndex);
        if (scenarioText != null) scenarioText.text = $"Scenario: {curScenario + 1}";
    }

    private void UpdateOceanHeight()
    {

        int curYear = yearKeys[curYearIndex];
        float curSeaLevelCm = scenarios[curScenario].yearsToLevels[curYear];
        float curSeaLevelMeters = curSeaLevelCm / 100f;

        // Build ArcGIS point at new elevation
        ArcGISPoint geoPoint = new ArcGISPoint(
            location.longitude,
            location.latitude,
            curSeaLevelMeters,
            ArcGISSpatialReference.WGS84()
        );
        ocean.Position = geoPoint;

        // Update UI
        if (yearText != null) yearText.text = $"Year: {curYear}";
        if (elevationText != null) elevationText.text = $"Sea Level: {curSeaLevelCm} cm";

        Debug.Log($"Scenario {curScenario}, Year {curYear}, " +
                  $"Sea Level = {curSeaLevelCm}cm, " +
                  $"Ocean altitude = {ocean.Position.Z}m");
    }

    public static List<SeaLevelStationData> LoadCSV(string filePath)
    {
        var results = new List<SeaLevelStationData>();
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
            throw new Exception("CSV does not contain data.");

        string[] headers = lines[0].Split(',');

        // Extract years (skip OBJECTID)
        List<int> years = new List<int>();
        for (int i = 1; i < headers.Length; i++)
        {
            string yearStr = new string(Array.FindAll(headers[i].ToCharArray(), c => char.IsDigit(c)));
            if (int.TryParse(yearStr, out int year))
                years.Add(year);
        }

        // Parse rows
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            SeaLevelStationData station = new SeaLevelStationData
            {
                objectID = int.Parse(cols[0]),
                yearsToLevels = new SortedDictionary<int, float>()
            };

            for (int j = 1; j < cols.Length - 1; j++)
            {
                if (float.TryParse(cols[j], out float value))
                {
                    station.yearsToLevels[years[j - 1]] = value;
                }
            }

            results.Add(station);
        }
        return results;
    }
}
