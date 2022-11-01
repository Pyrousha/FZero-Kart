using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A checkpoint placed on the track for a race. Has next/previous checkpoints set by the RaceController.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    private Checkpoint nextCheckpoint;
    public Checkpoint NextCheckpoint => nextCheckpoint;

    private Checkpoint prevCheckpoint;
    public Checkpoint PrevCheckpoint => prevCheckpoint;

    public float distToNextCheck { get; private set; }
    public float distToPrevCheck { get; private set; }

    public void SetNextCheckpoint(Checkpoint newChek)
    {
        nextCheckpoint = newChek;
        distToNextCheck = Vector3.Distance(transform.position, nextCheckpoint.transform.position);
    }

    public void SetPrevCheckpoint(Checkpoint newChek)
    {
        prevCheckpoint = newChek;
        distToPrevCheck = Vector3.Distance(transform.position, prevCheckpoint.transform.position);
    }
}
