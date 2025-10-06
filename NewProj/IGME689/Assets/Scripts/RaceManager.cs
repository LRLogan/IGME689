using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to finish line
/// </summary>
public class RaceManager : MonoBehaviour
{
    [SerializeField] private CarController player;
    [SerializeField] private TextMeshProUGUI timerTxt, checkpointNumTxt, raceEndTxt;
    [SerializeField] private Button restartBtn, respawnBtn;

    private Vector3 startingPoint;
    private Vector3 lastCheckpointLoc;

    // Start is called before the first frame update
    void Start()
    {
        startingPoint = player.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        lastCheckpointLoc = player.curCheckpointObj.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && player.curCheckpoint == 347)
        {
            player.raceEnded = true;
            raceEndTxt.enabled = true;
        }
    }

    public void Respawn()
    {

    }

    public void Restart()
    {

    }
}
