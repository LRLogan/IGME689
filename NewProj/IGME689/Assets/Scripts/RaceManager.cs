using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to finish line
/// </summary>
public class RaceManager : MonoBehaviour
{
    [SerializeField] private CarController player;

    // Start is called before the first frame update
    void Start()
    {
        // Teleport player (already esists in scene) to starting point
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            
        }
    }
}
