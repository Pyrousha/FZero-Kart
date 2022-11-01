using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that holds all information about the current race.
/// Mainly tracks the race position of all racers, handles race setup, and race end code. 
/// </summary>
public class RaceController : Singleton<RaceController>
{
    [Header("References")]
    public Material activeCheckpointMaterial;
    public Material inactiveCheckpointMaterial;
    [SerializeField] private TextMeshProUGUI posNumberText;
    [SerializeField] private TextMeshProUGUI posSuffixText;

    public Checkpoint endCheckpoint { get; private set; }
    private List<MechRacer> finishedRacers = new List<MechRacer>();
    private List<MechRacer> currentRacers = new List<MechRacer>();

    [Header("Track Specific References/Settings")]
    [SerializeField] private int totalLaps;
    [SerializeField] private Transform checkpointParent;
    private List<Checkpoint> checkpoints;
    public int TotalLaps => totalLaps;

    public void Start()
    {
        checkpoints = new List<Checkpoint>(checkpointParent.GetComponentsInChildren<Checkpoint>());

        //Link checkpoints to each other
        Checkpoint firstCheckpoint = checkpoints[0];
        Checkpoint lastCheckpoint = checkpoints[checkpoints.Count - 1];

        //Set next checkpoints
        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            checkpoints[i].SetNextCheckpoint(checkpoints[i + 1]);
        }
        lastCheckpoint.SetNextCheckpoint(firstCheckpoint);

        //Set previous checkpoints
        for (int i = 1; i < checkpoints.Count; i++)
        {
            checkpoints[i].SetPrevCheckpoint(checkpoints[i - 1]);
        }
        firstCheckpoint.SetPrevCheckpoint(lastCheckpoint);

        //Set end checkpoint var
        endCheckpoint = lastCheckpoint;
    }

    public void AddRacer(MechRacer racer)
    {
        currentRacers.Add(racer);
        racer.SetNextCheckpoint(checkpoints[0]);
    }

    public void Update()
    {
        currentRacers.Sort(CompareRacerPositions);

        //Update position UI for the local racers
        for (int i = 0; i < currentRacers.Count; i++)
        {
            MechRacer mechRacer = currentRacers[i];
            if (mechRacer.IsLocalPlayer)
            {
                UpdateRacePosUI(i + 1 + finishedRacers.Count);
                return;
            }
        }
    }

    /// <summary>
    /// called when a racer finishes the race (passes last checkpoint on final lap)
    /// </summary>
    /// <param name="mechRacer"> racer that just finished </param>
    public void FinishedRace(MechRacer mechRacer)
    {
        currentRacers.Remove(mechRacer);
        finishedRacers.Add(mechRacer);

        //Set position UI based on how many finished before
        if (mechRacer.IsLocalPlayer)
            UpdateRacePosUI(finishedRacers.Count);

        if (currentRacers.Count == 0)
        {
            //all racers have finished!
            Debug.Log("FINISH!!");
        }
    }

    /// <summary>
    /// Sets the UI displaying the local player's position in the race (stuff like 1st, 2nd, etc.)
    /// </summary>
    /// <param name="currPos"> local player's position/rank in the race. </param>
    private void UpdateRacePosUI(int currPos)
    {
        string posSuffix = RacePosSuffix(currPos);

        posNumberText.text = currPos.ToString();
        posSuffixText.text = "\n"+posSuffix;
    }

    /// <summary>
    /// Comparison function used for sorting racers based on how far along the track they are
    /// </summary>
    /// <param name="racer1"></param>
    /// <param name="racer2"></param>
    /// <returns> 1 if racer1 is ahead of racer2, -1 if racer2 is ahead of racer1, 0 if tied.t  </returns>
    private int CompareRacerPositions(MechRacer racer2, MechRacer racer1)
    {
        if (racer1.TrackPos > racer2.TrackPos)
            return 1;
        if (racer1.TrackPos < racer2.TrackPos)
            return -1;
        return 0;
    }

    /// <summary>
    /// Converts a race position int to string suffix, for example 1 becomes "st", 2 becomes "nd", etc.
    /// </summary>
    /// <param name="racePos"> race position as an int. </param>
    /// <returns> race position suffix as a string. </returns>
    public static string RacePosSuffix(int racePos)
    {

        if ((racePos == 1) || ((racePos % 10 == 1) && (racePos > 20)))
            return "st";

        if ((racePos == 2) || ((racePos % 10 == 2) && (racePos > 20)))
            return "nd";

        if ((racePos == 3) || ((racePos % 10 == 3) && (racePos > 20)))
            return "rd";

        return "th";
    }
}
