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

    [Header("Raycast Settings")]
    public LayerMask groundLayer;
    // Ground snapping variables
    public bool isGrounded;
    public float groundCheckDistance = 1.05f;
    public float rotationAlignSpeed = 0.05f;
    public float horizontalOffset = 0.2f; // Horizontal offset for ground check raycast

    [Header("Sphere Collider stuff")]
    public float colliderOffset = 1.69f; //Offset for the sphere collider to position kart correctly
    public Transform spherePosTransform; //Reference to the sphere collider transform
    public Rigidbody sphere;

    [Header("Kart Settings")]
    //acceleration, decceleration
    public float accelerationRate, deccelerationRate, airDeccelerationRate;
    public float minSpeed = 5f;
    public float maxSpeed = 60f;
    public float airTurnSpeed = 30f; //Turning speed in the air, to prevent kart from turning too fast in the air
    public float turnSpeed = 40;
    public float maxSteerAngleTires = 20f; //Multiplier for wheel turning speed    
    public float maxSteeringAngle = 10f; //Maximum steering angle for the steering wheel
    public Transform kartNormal;
    public float gravity = 20;
    public float inputLerpSpeed = 5f; //Lerp speed for input smoothing

    [Header("Do not Change")]
    public Vector3 acceleration; //How fast karts velocity changes        
    public Vector3 movementDirection;
    public Quaternion turning;
    Vector3 inputFixed;

    [Header("Input System Settings")]
    public PlayerInput playerInput;
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
        HandleGroundCheck();

        if (!raceEnded)
        {
            // Movement
            movementDirection.x = Mathf.Lerp(movementDirection.x, inputFixed.x, inputLerpSpeed * Time.deltaTime);
            movementDirection.z = inputFixed.z;

            if (movementDirection.z != 0f && isGrounded)
            {
                //Setting acceleration 
                if ((sphere.velocity.magnitude > maxSpeed) || (isDrifting && sphere.velocity.magnitude > driftMaxSpeed))
                {
                    acceleration = Vector3.zero; //If we are going too fast, stop accelerating
                }
                else
                {
                    acceleration = kartModel.forward * movementDirection.z * accelerationRate * Time.deltaTime;
                }

            }
            else if (isGrounded)
            {
                //Decceleration
                acceleration *= 1f - (deccelerationRate * Time.fixedDeltaTime);

                //Stop the vehicle once we reach a certain minimum speed
                if (sphere.velocity.magnitude < minSpeed)
                {
                    sphere.velocity = Vector3.zero;
                    acceleration = Vector3.zero;
                }
            }
            else
            {
                //In the air, decelerating bc of drag
                acceleration *= 1f - (airDeccelerationRate * Time.fixedDeltaTime);
            }

            // Turning
            if (!(sphere.velocity == Vector3.zero))
            {
                float turnAngle = maxTurnAngle;
                //If we are moving backwards, reverse the turning direction
                if (movementDirection.z < 0)
                {
                    turnAngle *= -1;
                }
                //If we are drifting, increase the turn angle
                if (isDrifting)
                {
                    turnAngle *= driftTurnBoost;
                }
                //Reduce turn angle based on current speed
                turnAngle *= Mathf.Clamp01(sphere.velocity.magnitude / maxSpeed);
                //Apply rotation to the kart model
                kartModel.Rotate(0, movementDirection.x * turnAngle * Time.fixedDeltaTime, 0);
            }
        }
    }

    void HandleGroundCheck()
    {
        RaycastHit hitNear;

        if (doGroundCheck)
        {
            if (Physics.Raycast(transform.position + (transform.up * .2f), -kartNormal.up, out hitNear, groundCheckDistance, groundLayer))
            {
                isGrounded = true;

                //Normal Rotation
                kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * rotationAlignSpeed);
                kartNormal.Rotate(0, transform.eulerAngles.y, 0);

            }
            else
            {
                isGrounded = false;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // Reverse Inputs
        if (isConfused)
        {
            input *= -1;
        }

        inputFixed = new Vector3(input.x, 0, input.y);
        //movementDirection = fixedInput;


        //movementDirection.z = movementDirection.y;
        //float inputZ = movementDirection.z;

        //movementDirection.x = Mathf.Lerp(movementDirection.x, fixedInput.x, inputLerpSpeed * Time.deltaTime);
        //movementDirection.z = fixedInput.z;
        //movementDirection.y = 0; //We are not gonna jump duh

        // determines when driving starts and when driving ends
        if (context.started)
        {
            isDriving = true;
        }
        else if (context.canceled)
        {
            isDriving = false;

            // // stops engine sound
            // if (soundPlayer.isPlaying)
            // {
            //     soundPlayer.Stop();
            // }
        }
    }
}
