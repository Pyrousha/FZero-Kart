using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class NPCController : NetworkBehaviour
{
    [Header("NPC-Specific Parameters")]
    [SerializeField] private float turnThreshold; //how much of an angle difference to target to start turning
    [SerializeField] private float driftThreshold; //how much of an angle difference to target to start drifting
    [SerializeField] private float noAccelerateThreshold; //how much of an angle difference to target to stop accelerating
    [SerializeField] private float brakeThreshold; //how much of an angle difference to target to stop accelerating


    #region NPC Buttons
    public struct NPCButtonState
    {
        private bool firstFrame;
        public bool held { get; private set; }
        public bool down
        {
            get
            {
                return held && firstFrame;
            }
        }
        public bool released
        {
            get
            {
                return !held && firstFrame;
            }
        }

        public void Set(bool toSet)
        {
            held = toSet;
            firstFrame = true;
        }
        public void Reset()
        {
            firstFrame = false;
        }
    }

    //Movement Buttons
    private NPCButtonState accelerateBoost;

    private NPCButtonState brake;

    private float steering;
    private float driftAxis;

    //Combat Buttons
    private NPCButtonState attack;
    #endregion

    public void SetAIParams(AIEvolutionStats.ParamStruct param)
    {
        driftThreshold = param.driftThresh;
        noAccelerateThreshold = param.noAccelThresh;

        //Slightly randomize "AI"
        driftThreshold += driftThreshold * Random.Range(-0.15f, 0.15f);
        noAccelerateThreshold += noAccelerateThreshold * Random.Range(-0.15f, 0.15f);
    }
    public AIEvolutionStats.ParamStruct GetAIParams(float raceTime)
    {
        return new AIEvolutionStats.ParamStruct(driftThreshold, noAccelerateThreshold, raceTime);
    }

    [Header("Parameters")]
    [SerializeField] private MechRacer mechRacer;

    // Update is called once per frame
    [Server]
    void Update()
    {
        if (mechRacer.InLobby)
            return;

        //determine angle between forward and next checkpoint
        Vector3 toNextCheckpoint = mechRacer.NextCheckpoint.transform.position - transform.position;
        Vector3 downProj = Vector3.Project(toNextCheckpoint, -transform.up);
        Vector3 toNextHorizontal = toNextCheckpoint - downProj;
        float angleToTurn = -Vector3.SignedAngle(toNextHorizontal, transform.forward, transform.up);

        float absAngle = Mathf.Abs(angleToTurn);
        //calulate steering
        if (absAngle >= turnThreshold)
            steering = Mathf.Clamp(Mathf.Sign(angleToTurn), -1, 1);
        else
            steering = 0;

        //calculate drifting
        if (absAngle >= driftThreshold)
            driftAxis = Mathf.Clamp(Mathf.Sign(angleToTurn), -1, 1);
        else
            driftAxis = 0;

        //calculate acceleration
        if (absAngle >= noAccelerateThreshold)
            accelerateBoost.Set(false);
        else
            accelerateBoost.Set(true);

        //calculate braking
        if (absAngle >= brakeThreshold)
            brake.Set(true);
        else
            brake.Set(false);

        //Finally, pass all input to mechRacer to apply
        mechRacer.SetInputFromAIServer(steering, driftAxis, accelerateBoost.down, accelerateBoost.held, brake.held);
    }

    private void LateUpdate()
    {
        //Rest direction buttons
        accelerateBoost.Reset();
        brake.Reset();

        //reset attack inputs
        attack.Reset();
    }
}
