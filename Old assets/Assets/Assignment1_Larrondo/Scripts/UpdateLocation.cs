using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Unity.VisualScripting;

public class UpdateLocation : MonoBehaviour
{
    private ArcGISCameraComponent arcGISCam;
    private ArcGISMapComponent arcGISMap;
    private ArcGISLocationComponent locationComponent;

    private void Awake()
    {
        arcGISMap = GetComponent<ArcGISMapComponent>();
        arcGISCam = GetComponent<ArcGISCameraComponent>();
        locationComponent = GetComponent<ArcGISLocationComponent>();

        MoveCam();
        DebugPos();
    }
    // Start is called before the first frame update
    void Start()
    {
        DebugPos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MoveCam()
    {
        // Changes the pos
        locationComponent.Position = new ArcGISPoint(locationComponent.Position.X + 1,
            locationComponent.Position.Y + 1, locationComponent.Position.Z, ArcGISSpatialReference.WGS84());
    }

    void DebugPos()
    {
        Debug.Log(locationComponent.Position.X + ", " + locationComponent.Position.Y);
    }
}
