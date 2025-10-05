using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script that creates a race track of splines based off of points gotten from feature layer API
/// </summary>
public class FeatureTrackCreator : ArcGISFeatureLayerComponent
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(GetFeatures));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
