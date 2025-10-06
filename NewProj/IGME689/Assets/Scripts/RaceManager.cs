using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to finish line
/// </summary>
public class RaceManager : MonoBehaviour
{
    [SerializeField] private CarController player;
    [SerializeField] private Text timerTxt, checkpointNumTxt;
    [SerializeField] private Button restartBtn;

    private Vector3 startingPoint;

    // Start is called before the first frame update
    void Start()
    {
        startingPoint = player.transform.position;

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
