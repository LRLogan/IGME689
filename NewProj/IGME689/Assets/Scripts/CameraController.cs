using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Elevation;
using Esri.GameEngine.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcGISCameraComponent))]
[RequireComponent(typeof(ArcGISLocationComponent))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 500f;
    public float rotationSpeed = 200f;
    public float zoomSpeed = 200f;

    [SerializeField] private ArcGISCameraComponent arcGISCamera;
    [SerializeField] private ArcGISLocationComponent locationComponent;

    private float yaw = 0f;
    private float pitch = 0f;

    private IEnumerator Start()
    {
        arcGISCamera = GetComponent<ArcGISCameraComponent>();
        locationComponent = GetComponent<ArcGISLocationComponent>();

        // Wait until the ArcGISLocationComponent has a valid position
        while (locationComponent.Position == null)
            yield return null;

        Debug.Log("ArcGIS Camera Controller ready!");
    }

    private void Update()
    {
        if (locationComponent == null || locationComponent.Position == null)
            return;
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        double moveX = 0, moveY = 0, moveZ = 0;

        // WASD movement on horizontal plane
        if (Input.GetKey(KeyCode.W)) moveZ += 1;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1;
        if (Input.GetKey(KeyCode.A)) moveX -= 1;
        if (Input.GetKey(KeyCode.D)) moveX += 1;

        // QE for altitude up/down
        if (Input.GetKey(KeyCode.E)) moveY += 1;
        if (Input.GetKey(KeyCode.Q)) moveY -= 1;

        // Combine into a direction vector in local space
        Vector3 moveDir = new Vector3((float)moveX, (float)moveY, (float)moveZ).normalized;
        Vector3 worldMove = transform.TransformDirection(moveDir) * moveSpeed * Time.deltaTime;

        // Update location using ArcGIS component (meters)
        var currentPos = locationComponent.Position;
        locationComponent.Position = new ArcGISPoint(
            currentPos.X + worldMove.x,
            currentPos.Y + worldMove.z,
            currentPos.Z + worldMove.y,
            currentPos.SpatialReference
        );
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right-click to rotate
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        // Mouse scroll for zoom (adjusts height)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            var pos = locationComponent.Position;
            pos = new ArcGISPoint(pos.X, pos.Y, pos.Z + scroll * -zoomSpeed);
            locationComponent.Position = pos;
        }
    }
}
