using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MechStats")]
public class MechStats : ScriptableObject
{
    [SerializeField] private Sprite mechIcon;
    public Sprite MechIcon => mechIcon;

    [Header("Parameters")]
    //Forward/back driving
    [SerializeField] private float maxSpeedWhileStraight; public float MaxSpeedWhileStraight => maxSpeedWhileStraight;
    [SerializeField] private float maxSpeedWhileMaxTurning; public float MaxSpeedWhileMaxTurning => maxSpeedWhileMaxTurning;
    [SerializeField] private float maxSpeedReverse; public float MaxSpeedReverse => maxSpeedReverse;
    [SerializeField] private float accelSpeed; public float AccelSpeed => accelSpeed; //acceleration in units/second 
    [Space(5)]
    [SerializeField] private float frictionSpeed; public float FrictionSpeed => frictionSpeed; //friction in units/second
    [SerializeField] private float brakeSpeed; public float BrakeSpeed => brakeSpeed; //deceleration in units/second
    [Space(5)]
    [SerializeField] private float turnFriction; public float TurnFriction => turnFriction;
    //[SerializeField] private float turnFrictionMultiplier; //uses current turn speed to apply friction

    //Turning
    [Space(25)]
    [SerializeField] private float maxTurnSpeed; public float MaxTurnSpeed => maxTurnSpeed; //angles/sec to turn when holding max left/right
    [SerializeField] private float turnSpeedAccel; public float TurnSpeedAccel => turnSpeedAccel; //units/sec to move currTurnSpeed towards target turn speed
    [SerializeField] private float turnSpeedFriction; public float TurnSpeedFriction => turnSpeedFriction; //units/sec to move currTurnSpeed back towards 0 when not holding left/right

    //Drifting
    [Space(25)]
    [SerializeField] private float maxDriftSpeed; public float MaxDriftSpeed => maxDriftSpeed; //angles/sec to turn when holding drift left/right
    [SerializeField] private float driftSpeedAccel; public float DriftSpeedAccel => driftSpeedAccel; //amount to move currDriftSpeed towards held direction (angles/sec)
    [SerializeField] private float driftSpeedFriction; public float DriftSpeedFriction => driftSpeedFriction;//amount to move currDriftSpeed towards 0 when nothing held (angles/sec)
}
