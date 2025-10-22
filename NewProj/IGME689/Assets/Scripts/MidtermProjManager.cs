using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MidtermProjManager : MonoBehaviour
{
    [SerializeField] private RoadCountCSVParser csvParser;
    [SerializeField] private RoadMapLineBuilder lineBuilder;

    private bool roadSetUpFin = false;

    // Start is used as a wrapper function
    void Start()
    {
        StartSimulation();
    }

    private void StartSimulation()
    {
        StartCoroutine(lineBuilder.QueryFeatureService(() =>
        {
            AssignStartingData();
        }));
    }

    private void AssignStartingData()
    {
        StartCoroutine(csvParser.WaitAndParse(() =>
        {
            roadSetUpFin = true;
            Debug.Log($"Road set up complete: {roadSetUpFin}");
        }));
    }
}
