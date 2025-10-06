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
    [SerializeField] private TextMeshProUGUI timerTxt, checkpointNumTxt, raceEndTxt, timeToBeatTxt;
    [SerializeField] private Button restartBtn, respawnBtn, playAgainBtn;

    private Vector3 startingPoint;
    private Vector3 lastCheckpointLoc;
    private float timeElapsed = 0;
    private float timeToBeat = float.MaxValue;
    private bool race1 = true;

    // Start is called before the first frame update
    void Start()
    {
        startingPoint = player.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if(player.curCheckpointObj == null)
        {
            lastCheckpointLoc = startingPoint;
        }
        else
        {
            lastCheckpointLoc = player.curCheckpointObj.transform.position;
        }

        if (!player.raceEnded)
        {
            timerTxt.text = $"Race Time: {(timeElapsed += Time.deltaTime):F2}";
            checkpointNumTxt.text = $"Checkpoint #: {player.curCheckpoint} / 347";

            if (!race1) timeToBeatTxt.text = $"Time To Beat: {timeToBeat}";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && player.curCheckpoint >= 347)
        {
            player.raceEnded = true;
            raceEndTxt.gameObject.SetActive(true);
            playAgainBtn.gameObject.SetActive(true);
            race1 = false;
            if(timeElapsed < timeToBeat) timeToBeat = timeElapsed;
            restartBtn.gameObject.SetActive(false);
            respawnBtn.gameObject.SetActive(false);
        }
    }

    public void Respawn()
    {
        player.gameObject.transform.position = player.curCheckpointObj.transform.position;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void Restart()
    {
        timeElapsed = 0;
        player.gameObject.transform.position = startingPoint;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void PlayAgain()
    {
        timeElapsed = 0;
        player.gameObject.transform.position = startingPoint;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.raceEnded = false;
        raceEndTxt.gameObject.SetActive(false);
        playAgainBtn.gameObject.SetActive(false);
        restartBtn.gameObject.SetActive(true);
        respawnBtn.gameObject.SetActive(true);
    }
}
