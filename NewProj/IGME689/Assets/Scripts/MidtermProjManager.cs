using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MidtermProjManager : MonoBehaviour
{
    [SerializeField] private RoadCountCSVParser csvParser;
    [SerializeField] private RoadMapLineBuilder lineBuilder;

    private bool roadSetUpFin = false;
    private List<GameObject> lineArray;

    // Start is used as a wrapper function
    void Start()
    {
        StartSimulation();
    }

    private void StartSimulation()
    {
        StartCoroutine(lineBuilder.QueryFeatureService(() =>
        {
            lineArray = lineBuilder.lineArray;
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

    public void UpdateCValue(float cValue)
    {
        foreach(GameObject obj in lineArray)
        {
            obj.GetComponent<RoadData>().UpdateCValAndGrad(cValue);
        }
    }
}
