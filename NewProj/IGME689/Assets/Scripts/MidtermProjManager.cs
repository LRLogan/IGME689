using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Central manager controlling simulation setup and user-driven updates
/// such as time-of-day changes affecting congestion visualization.
/// </summary>
public class MidtermProjManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoadCountCSVParser csvParser;
    [SerializeField] private RoadMapLineBuilder lineBuilder;
    [SerializeField] private Toggle toggle;          // AM/PM toggle (true = PM, false = AM)
    [SerializeField] private TMP_Dropdown dropdown;  // Dropdown for hour selection (1–12)
    [SerializeField] private GameObject loadingPannel;

    private List<GameObject> lineArray;
    private bool roadSetUpFin = false;

    private void Start()
    {
        StartSimulation();

        // Register event listeners for the dropdown and toggle
        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnTimeChanged);

        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void StartSimulation()
    {
        loadingPannel.GetComponentInChildren<TextMeshProUGUI>().text = 
            "Traffic visualizer simulation now loading: \nFetching road data. This will take a moment.";

        StartCoroutine(lineBuilder.QueryFeatureService(() =>
        {
            lineArray = lineBuilder.lineArray;
            AssignStartingData();
        }, loadingPannel.GetComponentInChildren<TextMeshProUGUI>()));
    }

    private void AssignStartingData()
    {
        loadingPannel.GetComponentInChildren<TextMeshProUGUI>().text = 
            "Traffic visualizer simulation now loading: \nAnalyzing data and preparing simulation. " +
            "\nThis will take a few minitues if this is your first time compiling project";
        StartCoroutine(csvParser.WaitAndParse(() =>
        {
            roadSetUpFin = true;
            Debug.Log($"Road setup complete: {roadSetUpFin}");

            loadingPannel.SetActive(false);

            // Once setup is finished, initialize congestion view for the first hour.
            UpdateCongestionForSelectedHour();
        }));
    }

    /// <summary>
    /// Called by the dropdown when a new hour is selected.
    /// </summary>
    private void OnTimeChanged(int _)
    {
        if (roadSetUpFin)
            UpdateCongestionForSelectedHour();
    }

    /// <summary>
    /// Called when the AM/PM toggle changes.
    /// </summary>
    private void OnToggleChanged(bool _)
    {
        if (roadSetUpFin)
            UpdateCongestionForSelectedHour();
    }

    /// <summary>
    /// Updates congestion values for all roads based on the selected hour and AM/PM state.
    /// </summary>
    private void UpdateCongestionForSelectedHour()
    {
        if (lineArray == null || lineArray.Count == 0) return;

        int hourIndex = GetSelectedHourIndex(); // 0–23

        foreach (GameObject roadObj in lineArray)
        {
            RoadData data = roadObj.GetComponent<RoadData>();
            if (data == null || data.averageCount == null || data.averageCount.Count == 0)
                continue;

            // averageCount keys are 1–24 (1 = 12AM–1AM, 24 = 11PM–12AM) so we add 1 to index
            if (data.averageCount.TryGetValue(hourIndex + 1, out float value))
            {
                data.congestionValue = value;
                data.UpdateCValAndGrad(value);
            }
        }

        Debug.Log($"Updated congestion values for hour index: {hourIndex} ({GetReadableTimeLabel(hourIndex)})");
    }

    /// <summary>
    /// Computes the 0–23 hour index based on dropdown and AM/PM toggle state.
    /// </summary>
    private int GetSelectedHourIndex()
    {
        int selectedHour = dropdown != null ? dropdown.value : 1; // Dropdown values 
        bool isPM = toggle != null && toggle.isOn;

        // Convert 12-hour format to 24-hour index
        int hourIndex = selectedHour % 12;
        if (isPM) hourIndex += 12;

        return hourIndex; // 0–23
    }

    /// <summary>
    /// Converts the hour index (0–23) into a readable time label for debugging.
    /// </summary>
    private string GetReadableTimeLabel(int hourIndex)
    {
        string suffix = hourIndex >= 12 ? "PM" : "AM";
        int displayHour = hourIndex % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour}:00 {suffix}";
    }

    /// <summary>
    /// Optional manual update method for testing.
    /// </summary>
    public void UpdateCValue(float cValue)
    {
        foreach (GameObject obj in lineArray)
        {
            obj.GetComponent<RoadData>().UpdateCValAndGrad(cValue);
        }
    }
}
