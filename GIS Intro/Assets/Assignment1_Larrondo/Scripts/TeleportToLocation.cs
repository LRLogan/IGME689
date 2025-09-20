using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TeleportToLocation : MonoBehaviour
{
    [SerializeField] private ArcGISCameraComponent arcGISCam;
    [SerializeField] private ArcGISLocationComponent locationComponent;
    [SerializeField] private ArcGISMapComponent mapComponent;
    [SerializeField] private GameObject player;
    private float playerHeightOffset = 20f;

    [System.Serializable]
    public struct Location
    {
        public double longitude;
        public double latitude;
        public double altitude;
    }

    public Location[] locations;
    private int currentIndex = 1;

    public void SwapLocation()
    { 
        currentIndex = (currentIndex + 1) % locations.Length;
        var loc = locations[currentIndex];

        // Getting the point in lat / long
        ArcGISPoint geoPoint = new ArcGISPoint(
            loc.longitude,
            loc.latitude,
            loc.altitude,
            ArcGISSpatialReference.WGS84()
        );

        // Centering map at origin due to finding out that without this converting
        // the lat / long into unity coordnites would result in too large of a number
        mapComponent.OriginPosition = geoPoint;

        Debug.Log($"Teleporting camera to {geoPoint.X}, {geoPoint.Y}, {geoPoint.Z} (Lat / Long)");
        locationComponent.Position = geoPoint;

        Debug.Log($"Teleporting player to 0, {(float)-loc.altitude + playerHeightOffset}, 0 \n(Center of map at coorisponding alditude)");
        player.transform.position = new Vector3(0, (float)-loc.altitude + playerHeightOffset, 0);
    }
}
