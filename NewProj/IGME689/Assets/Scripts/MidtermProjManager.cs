using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Central manager controlling simulation setup and user-driven updates,
/// including time-of-day and weather effects on congestion visualization.
/// </summary>
public class MidtermProjManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoadCountCSVParser csvParser;
    [SerializeField] private RoadMapLineBuilder lineBuilder;
    [SerializeField] private Toggle toggle;                     // AM/PM toggle (true = PM, false = AM)
    [SerializeField] private TMP_Dropdown timeDD;               // Dropdown for hour selection (1–12)
    [SerializeField] private TMP_Dropdown weatherDD;            // Dropdown for weather (Clear, Light Rain, Heavy Rain, Snow)
    [SerializeField] private GameObject loadingPannel;

    private List<GameObject> lineArray;
    private bool roadSetUpFin = false;

    // Weather multipliers
    private readonly Dictionary<string, float> weatherMultipliers = new Dictionary<string, float>
    {
        { "Weather clear", 1.0f },
        { "Weather light rain", 1.25f },
        { "Weather heavy rain", 1.4f },
        { "Weather snow", 1.8f }
    };

    private void Start()
    {
        StartSimulation();

        // Register event listeners
        if (timeDD != null)
            timeDD.onValueChanged.AddListener(OnTimeChanged);

        if (weatherDD != null)
            weatherDD.onValueChanged.AddListener(OnWeatherChanged);

        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void StartSimulation()
    {
        loadingPannel.GetComponentInChildren<TextMeshProUGUI>().text =
            "Traffic visualizer simulation now loading:\nFetching road data. This will take a moment.";

        StartCoroutine(lineBuilder.QueryFeatureService(() =>
        {
            lineArray = lineBuilder.lineArray;
            AssignStartingData();
        }, loadingPannel.GetComponentInChildren<TextMeshProUGUI>()));
    }

    private void AssignStartingData()
    {
        loadingPannel.GetComponentInChildren<TextMeshProUGUI>().text =
            "Traffic visualizer simulation now loading:\nAnalyzing data and preparing simulation.\n" +
            "This will take a few minutes if this is your first time compiling project.";

        StartCoroutine(csvParser.WaitAndParse(() =>
        {
            roadSetUpFin = true;
            Debug.Log($"Road setup complete: {roadSetUpFin}");

            loadingPannel.SetActive(false);
            UpdateCongestionForSelectedHour();
        }));
    }

    private void OnWeatherChanged(int _)
    {
        if (roadSetUpFin)
            UpdateCongestionForSelectedHour();
    }

    private void OnTimeChanged(int _)
    {
        if (roadSetUpFin)
            UpdateCongestionForSelectedHour();
    }

    private void OnToggleChanged(bool _)
    {
        if (roadSetUpFin)
            UpdateCongestionForSelectedHour();
    }

    /// <summary>
    /// Updates congestion values for all roads based on the selected hour, AM/PM state, and weather.
    /// </summary>
    private void UpdateCongestionForSelectedHour()
    {
        if (lineArray == null || lineArray.Count == 0) return;

        int hourIndex = GetSelectedHourIndex(); // 0–23
        float weatherMultiplier = GetWeatherMultiplier();

        foreach (GameObject roadObj in lineArray)
        {
            RoadData data = roadObj.GetComponent<RoadData>();
            if (data == null || data.averageCount == null || data.averageCount.Count == 0)
                continue;

            if (data.averageCount.TryGetValue(hourIndex + 1, out float value))
            {
                float adjustedValue = value * weatherMultiplier;
                data.UpdateCValAndGrad(adjustedValue);
            }
        }

        Debug.Log($"Updated congestion values for hour index: {hourIndex} ({GetReadableTimeLabel(hourIndex)}), weather: {weatherDD.options[weatherDD.value].text}");
    }

    private float GetWeatherMultiplier()
    {
        if (weatherDD == null || weatherDD.options.Count == 0)
            return 1.0f;

        string selectedWeather = weatherDD.options[weatherDD.value].text;
        return weatherMultipliers.TryGetValue(selectedWeather, out float multiplier) ? multiplier : 1.0f;
    }

    private int GetSelectedHourIndex()
    {
        int selectedHour = timeDD != null ? timeDD.value : 0; // Dropdown values start at 0
        bool isPM = toggle != null && toggle.isOn;

        int hourIndex = selectedHour % 12;
        if (isPM) hourIndex += 12;

        return hourIndex; // 0–23
    }

    private string GetReadableTimeLabel(int hourIndex)
    {
        string suffix = hourIndex >= 12 ? "PM" : "AM";
        int displayHour = hourIndex % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour}:00 {suffix}";
    }

    public void UpdateCValue(float cValue)
    {
        foreach (GameObject obj in lineArray)
        {
            obj.GetComponent<RoadData>().UpdateCValAndGrad(cValue);
        }
    }
}
