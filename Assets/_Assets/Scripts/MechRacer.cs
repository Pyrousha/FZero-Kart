using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a racer for the current race, handles non-input related things related to racing (checkpoints, race position, etc)
/// </summary>
public class MechRacer : MonoBehaviour
{
    private bool isLocalPlayer;
    public bool IsLocalPlayer => isLocalPlayer;

    private bool raceFinished = false;

    private int lapsFinished = 0;
    private int checkpointsHit = 0;

    private Checkpoint nextCheckpoint;
    public Checkpoint NextCheckpoint => nextCheckpoint;

    public void SetNextCheckpoint(Checkpoint check)
    {
        nextCheckpoint = check;
    }

    private float trackPos;
    public float TrackPos => trackPos;

    public PlayerController playerController {get; private set;}
    public NPCController npcController {get; private set;}

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        npcController = GetComponent<NPCController>();

        isLocalPlayer = playerController != null;

        RaceController.Instance.AddRacer(this);
    }

    void Update()
    {
        if (raceFinished)
            return;

        //Ratio of how far the player has traveled from the previous checkpoint to the next one
        float ratioToNextCheck = 1 - (Vector3.Distance(transform.position, nextCheckpoint.transform.position) / nextCheckpoint.distToPrevCheck);
        trackPos = checkpointsHit + ratioToNextCheck;
    }

    public void OnTriggerEnter(Collider collider)
    {
        //Checkpoint hit
        Checkpoint checkpoint = collider.GetComponent<Checkpoint>();

        if (checkpoint != nextCheckpoint) //make sure right checkpoint was hit
        {
            //Debug.Log("Wrong checkpoint hit!");
            return;
        }

        checkpointsHit++;
        //Debug.Log("Racer " + name + " has hit checkpoint " + checkpointsHit);

        if (isLocalPlayer)
        {
            //change color, for testing
            checkpoint.gameObject.GetComponent<MeshRenderer>().material = RaceController.Instance.inactiveCheckpointMaterial;
            checkpoint.NextCheckpoint.gameObject.GetComponent<MeshRenderer>().material = RaceController.Instance.activeCheckpointMaterial;
        }

        if ((checkpoint == RaceController.Instance.endCheckpoint) && (raceFinished == false))
        {
            //lap finished
            lapsFinished++;
            Debug.Log("Racer " + name + " has completed lap " + lapsFinished);
            if (lapsFinished >= RaceController.Instance.TotalLaps)
            {
                //Race Finished
                raceFinished = true;
                RaceController.Instance.FinishedRace(this);

                if(isLocalPlayer)
                {
                    //swap player input for npc input
                    PlayerController playerController = GetComponent<PlayerController>();
                    playerController.enabled = false;
                    NPCController npcController = GetComponent<NPCController>();
                    npcController.enabled = true;

                    npcController.LoadStateFromPlayer(playerController);
                }
            }
            if (isLocalPlayer)
                RaceController.Instance.UpdateLapUI(lapsFinished);
        }

        nextCheckpoint = checkpoint.NextCheckpoint;
    }
}
