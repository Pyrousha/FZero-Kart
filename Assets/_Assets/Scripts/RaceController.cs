using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that holds all information about the current race.
/// Mainly tracks the race position of all racers, handles race setup, and race end code. 
/// </summary>
public class RaceController : Singleton<RaceController>
{
    public Checkpoint endCheckpoint { get; private set; }
    private List<MechRacer> finishedRacers;
    private List<MechRacer> currentRacers;

    [SerializeField] private List<Checkpoint> checkpoints;

    public int totalLaps { get; private set; }

    public void Start()
    {
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



        //Set starting next checkpoint for all races
        foreach (MechRacer racer in currentRacers)
        {
            racer.SetNextCheckpoint(firstCheckpoint);
        }
    }

    public void Update()
    {
        currentRacers.Sort(CompareRacerPositions);

        //Update positions for all current racers
        for (int i = 0; i < currentRacers.Count; i++)
        {
            currentRacers[i].racePosition = i + 1 + finishedRacers.Count;
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

        //Set position based on how many finished before
        mechRacer.racePosition = finishedRacers.Count;

        if (currentRacers.Count == 0)
        {
            //all racers have finished!
        }
    }

    /// <summary>
    /// Comparison function used for sorting racers based on how far along the track they are
    /// </summary>
    /// <param name="racer1"></param>
    /// <param name="racer2"></param>
    /// <returns> 1 if racer1 is ahead of racer2, -1 if racer2 is ahead of racer1, 0 if tied. </returns>
    private int CompareRacerPositions(MechRacer racer1, MechRacer racer2)
    {
        if (racer1.TrackPos > racer2.TrackPos)
            return 1;
        if (racer1.TrackPos < racer2.TrackPos)
            return -1;
        return 0;
    }
}
