using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadData : MonoBehaviour
{
    public float congestionValue;
    public string roadName;
    public Dictionary<int, float> averageCount; // Holds the average traffic count for each hour

    // Start is called before the first frame update
    void Start()
    {
        averageCount = new Dictionary<int, float>();

        // Assigning hours to the dict
        for(int i = 1; i < 25; i++)
        {
            averageCount[i] = 0;
        }
    }

}
