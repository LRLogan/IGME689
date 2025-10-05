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

public class ArcGISFeatureLayerComponent : MonoBehaviour
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
    private JToken[] jFeatures;
    private float spawnHeight = 0;

    public List<GameObject> FeatureItems = new List<GameObject>();
    public QueryLink WebLink;
    [SerializeField] private SplineContainer splineContainer;
    private ArcGISMapComponent mapComponent;

    private void Start()
    {
        StartCoroutine(nameof(GetFeatures));
        mapComponent = FindFirstObjectByType<ArcGISMapComponent>();
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
        foreach (var feature in jFeatures)
        {
            // Get coordinates in the Feature Service
            var coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray();

            foreach (var coordinate in coordinates)
            {

                var currentFeature = new FeatureQueryData();
                coordinates.ToArray();
                currentFeature.Geometry.Latitude = Convert.ToDouble(coordinate[1]);
                currentFeature.Geometry.Longitude = Convert.ToDouble(coordinate[0]);
                // Create new ArcGIS Point and pass the Feature Lat and Long to it
                var position = new ArcGISPoint(currentFeature.Geometry.Longitude, currentFeature.Geometry.Latitude, spawnHeight, new ArcGISSpatialReference(4326));

                // Create new Bezier Knot that stores transform data
                BezierKnot bezierKnot = new BezierKnot();

                // Convert ArcGISPoint to Engine Coordinates
                bezierKnot.Position = mapComponent.GeographicToEngine(position);

                // Add converted position to the splines container
                splineContainer.Splines[0].Add(bezierKnot);
            }
        }
    }
}
