using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // root reference of the prefab
    public Transform rootTransform;

    public bool raceEnded = false;

    [Header("Kart Settings")]
    public float maxSpeed = 60f;
    public float turnSpeed = 40;
    public float inputLerpSpeed = 5f; //Lerp speed for input smoothing
    public Vector3 acceleration; //How fast karts velocity changes        
    public Vector3 movementDirection;
    public Quaternion turning;
    public Vector3 inputFixed;
    public bool isDriving;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Move as long as the race has not ended
        if(!raceEnded)
        {
            // Movement
            movementDirection.x = Mathf.Lerp(movementDirection.x, inputFixed.x, inputLerpSpeed * Time.deltaTime);
            movementDirection.z = inputFixed.z;

            // Turning
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        inputFixed = new Vector3(input.x, 0, input.y);

        // Determines when driving starts and when driving ends
        if (context.started)
        {
            isDriving = true;
        }
        else if (context.canceled)
        {
            isDriving = false;
        }
    }
    
}
