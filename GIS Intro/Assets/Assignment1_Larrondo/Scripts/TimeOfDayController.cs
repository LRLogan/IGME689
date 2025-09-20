using System;
using UnityEngine;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Components;

public class SunPositionController : MonoBehaviour
{
    [SerializeField] private ArcGISMapComponent mapComponent;
    [SerializeField] private Light sunLight; // Your Directional Light

    [Header("Time Control")]
    [SerializeField] private bool useRealTime = true;    // Juat a way to test different times
    [SerializeField] private DateTime simulatedTime = new DateTime(2025, 9, 10, 12, 0, 0); 
    [SerializeField] private float timeScale = 60f;      

    private DateTime currentSimTime;
    private DateTime timeToUse;

    void Start()
    {
        currentSimTime = simulatedTime;
    }

    void Update()
    {
        if (useRealTime)
        {
            timeToUse = DateTime.Now;
        }
        else
        {
            // Advance simulated time based on timeScale
            currentSimTime = currentSimTime.AddSeconds(Time.deltaTime * timeScale);
            timeToUse = currentSimTime;
        }

        UpdateSunPosition(timeToUse);
    }

    void UpdateSunPosition(DateTime time)
    {
        // Get current geographic origin (lat/long)
        ArcGISPoint origin = mapComponent.OriginPosition;
        double latitude = origin.Y;
        double longitude = origin.X;

        // Convert UTC to fractional hours
        double totalMinutes = time.Hour * 60 + time.Minute + time.Second / 60.0;
        double timeUTC = totalMinutes / 60.0;

        int dayOfYear = time.DayOfYear;

        // Declination of the sun (approx)
        double decl = 23.45 * Mathf.Deg2Rad *
                      Mathf.Sin((float)(2 * Math.PI * (284 + dayOfYear) / 365.0));

        // Time correction for longitude
        double lstm = 15 * Math.Round(longitude / 15.0); // local standard meridian
        double tc = 4 * (longitude - lstm);
        double solarTime = timeUTC * 60 + tc; 
        double hourAngle = (solarTime / 4.0 - 180.0) * Mathf.Deg2Rad;

        // Solar alt
        double latRad = latitude * Mathf.Deg2Rad;
        double altitude = Math.Asin(Math.Sin(latRad) * Math.Sin(decl) +
                                    Math.Cos(latRad) * Math.Cos(decl) * Math.Cos(hourAngle));

        // Solar azimuth
        double azimuth = Math.Acos((Math.Sin(decl) - Math.Sin(altitude) * Math.Sin(latRad)) /
                                   (Math.Cos(altitude) * Math.Cos(latRad)));

        if (hourAngle > 0) { azimuth = 2 * Math.PI - azimuth; }

        float altitudeDeg = (float)(altitude * Mathf.Rad2Deg);
        float azimuthDeg = (float)(azimuth * Mathf.Rad2Deg);

        Quaternion sunRot = Quaternion.Euler(altitudeDeg, azimuthDeg, 0);
        sunLight.transform.rotation = sunRot;

        Debug.Log($"[Sun] {time} -> Alt: {altitudeDeg:F2}, Azi: {azimuthDeg:F2}");
    }
}
