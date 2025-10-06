using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))] // Ensure AudioSource exists
public class CarController : MonoBehaviour
{
    [Header("Root Reference")]
    public Transform rootTransform; // Used for visuals

    [Header("Race State")]
    public bool raceEnded = false;

    [Header("Kart Settings")]
    public float maxSpeed = 60f;
    public float accelerationRate = 25f;
    public float decelerationRate = 15f;
    public float turnSpeed = 40f;
    public float inputLerpSpeed = 5f;

    private Rigidbody rb;
    private Vector2 inputDir;
    private float currentSpeed;
    private Vector2 smoothedInput;

    public int curCheckpoint = 0;
    public GameObject curCheckpointObj;

    [Header("Audio")]
    [SerializeField] private AudioClip moveSfx; // Sound to play while moving
    private AudioSource audioSource;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // Loop engine sound while moving
        audioSource.playOnAwake = false;
    }

    private void FixedUpdate()
    {
        if (raceEnded)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 2f);
            return;
        }

        // Smooth the input
        smoothedInput = Vector2.Lerp(Vector2.zero, inputDir, inputLerpSpeed * Time.fixedDeltaTime);

        // Forward/backward movement
        if (smoothedInput.y > 0f) currentSpeed += accelerationRate * Time.fixedDeltaTime;
        else if (smoothedInput.y < 0f) currentSpeed -= accelerationRate * Time.fixedDeltaTime;
        else currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationRate * Time.fixedDeltaTime);

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Apply velocity
        Vector3 move = transform.forward * currentSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // Turning only if moving
        if (Mathf.Abs(smoothedInput.x) > 0.05f && Mathf.Abs(currentSpeed) > 0.05f)
        {
            float turn = smoothedInput.x * turnSpeed * Time.fixedDeltaTime * Mathf.Sign(currentSpeed);
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // Handle movement sound playback based on  velocity
        bool carIsMoving = rb.velocity.magnitude > 0.5f; 

        if (carIsMoving && !audioSource.isPlaying && moveSfx != null)
        {
            audioSource.clip = moveSfx;
            audioSource.Play();
        }
        else if (!carIsMoving && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputDir = context.ReadValue<Vector2>();
        Debug.Log("Move input: " + inputDir);
    }
}
