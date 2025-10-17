// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Creates a spline using data from a feature layer
/// This version if modified by Logan Larrondo to account for a custom prefab as the road and talord / optimized to the Midterm assignment
/// </summary>
public class ArcGISFeatureLayerQueryMidtermVersion : MonoBehaviour
{
    [System.Serializable]
    public struct QueryLink
    {
        public string Link;
        public string[] RequestHeaders;
    }

    [System.Serializable]
    public class GeometryData
    {
        public double Latitude;
        public double Longitude;
    }

    [System.Serializable]
    public class PropertyData
    {
        public List<string> PropertyNames = new List<string>();
        public List<string> Data = new List<string>();
    }

    [System.Serializable]
    public class FeatureQueryData
    {
        public GeometryData Geometry = new GeometryData();
        public PropertyData Properties = new PropertyData();
    }

    private List<FeatureQueryData> Features = new List<FeatureQueryData>();
    private FeatureData featureInfo;
    [SerializeField] private GameObject featurePrefab;
    [SerializeField] private GameObject checkpointPrefab;
    private JToken[] jFeatures;
    private float spawnHeight = 0;

    public List<GameObject> FeatureItems = new List<GameObject>();
    public QueryLink WebLink;
    [SerializeField] private SplineContainer splineContainer;
    private ArcGISMapComponent mapComponent;

    private void Start()
    {
        mapComponent = FindFirstObjectByType<ArcGISMapComponent>();
        StartCoroutine(nameof(GetFeatures));
    }

    public void CreateLink(string link)
    {
        if (link != null)
        {
            foreach (var header in WebLink.RequestHeaders)
            {
                if (!link.ToLower().Contains(header))
                {
                    link += header;
                }
            }

            WebLink.Link = link;
        }
    }

    public IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        UnityWebRequest Request = UnityWebRequest.Get(WebLink.Link);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            Debug.Log(Request.downloadHandler.text);
            CreateGameObjectsFromResponse(Request.downloadHandler.text);
        }
    }

    private void CreateGameObjectsFromResponse(string response)
    {
        // Deserialize the JSON response from the query.
        var jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();
        CreateFeatures();
    }

    private void CreateFeatures()
    {
        // Group features by the road name property to reduce the number of splines.
        var groupedFeatures = jFeatures
            .Where(f => f["properties"]?["NAME"] != null)
            .GroupBy(f => f["properties"]["NAME"].ToString());

        Debug.Log($"Creating splines for {groupedFeatures.Count()} grouped roads.");

        // Create a spline for each group of coords that share the parameters set above
        foreach (var group in groupedFeatures)
        {
            string streetName = group.Key;
            Spline newSpline = new Spline(0);

            // For each feature (road segment) in the group
            foreach (JToken feature in group)
            {
                // Reset previous position so new segment doesn’t connect to the last one
                Vector3? previousPos = null;

                // Get coordinates in the Feature Service
                var coordinates = feature.SelectToken("geometry.coordinates")?.ToArray();
                if (coordinates == null || coordinates.Length == 0)
                    continue;

                // Handle each coordinate
                foreach (JToken coordinate in coordinates)
                {
                    var currentFeature = new FeatureQueryData();
                    currentFeature.Geometry.Latitude = (double)coordinate[1];
                    currentFeature.Geometry.Longitude = (double)coordinate[0];

                    ArcGISPoint position = new ArcGISPoint(
                        currentFeature.Geometry.Longitude,
                        currentFeature.Geometry.Latitude,
                        spawnHeight,
                        new ArcGISSpatialReference(4326)
                    );

                    // Create new Bezier Knot that stores transform data
                    BezierKnot knot = new BezierKnot
                    {
                        Position = mapComponent.GeographicToEngine(position)
                    };

                    // Spawn a checkpoint prefab at this position 
                    if (checkpointPrefab != null)
                    {
                        if (previousPos.HasValue)
                        {
                            Vector3 roadDir = ((Vector3)knot.Position - previousPos.Value).normalized;
                            Quaternion rot = Quaternion.LookRotation(roadDir, Vector3.up);
                            Instantiate(checkpointPrefab, knot.Position, rot, transform);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Checkpoint Prefab not assigned in inspector!");
                    }

                    // Add converted position to the spline
                    newSpline.Add(knot);
                    previousPos = knot.Position;
                }
            }

            // Add completed spline to container
            splineContainer.AddSpline(newSpline);
        }
    }








}
