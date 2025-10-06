using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<CarController>().curCheckpoint++; 
            other.GetComponent<CarController>().curCheckpointObj = this.gameObject; 
        }
    }
}
