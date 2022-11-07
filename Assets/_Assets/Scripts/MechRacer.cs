using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a racer for the current race, handles non-input related things related to racing (checkpoints, race position, etc)
/// </summary>
public class MechRacer : MonoBehaviour
{
    [SerializeField] private Color playerNameColor;
    public Color PlayerNameColor => playerNameColor;
    private bool isLocalPlayer;
    public bool IsLocalPlayer => isLocalPlayer;
    private bool isHuman;
    public bool IsHuman => isHuman;

    //States
    private bool inLobby = true;
    public bool InLobby => inLobby;
    private bool raceFinished = false;
    private int lapsFinished = 0;
    private int checkpointsHit = 0;
    private bool isDead;
    private bool canMove = true;

    private Checkpoint nextCheckpoint;
    public Checkpoint NextCheckpoint => nextCheckpoint;

    public void SetNextCheckpoint(Checkpoint check)
    {
        nextCheckpoint = check;
    }

    private float trackPos;
    public float TrackPos => trackPos;

    public PlayerController playerController { get; private set; }
    public NPCController npcController { get; private set; }

    #region physics and controls
    private float currSpeed;

    private Vector3 gravityDirection;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float groundedGravityMultiplier;
    [SerializeField] private float lerpSpeed;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerModelParent;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Animator driftAnim;
    [Space(5)] //ground checking stuff
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundRaycastParent;
    private List<Transform> raycastPoints;
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private float raycastLength;
    private bool isGrounded;
    [Space(5)] //UI + Effects stuff
    [SerializeField] private ParticleSystemModifier leftTurnFX;
    [SerializeField] private ParticleSystemModifier rightTurnFX;
    [Space(5)]
    [SerializeField] private ParticleSystemModifier speedLines;
    [SerializeField] private TextMeshProUGUI speedNumberText;

    [Header("Parameters")]
    [SerializeField] private MechStats mechStats;
    public MechStats MechStats => mechStats;
    [SerializeField] private float timeToHitCheckpoint; //how much time the AI has to hit the next checkpoint or else they go boom
    private float checkpointTimer;


    #region local driving parameters
    //Forward/back driving
    private float currMaxSpeed;
    private float maxSpeedWhileStraight;
    private float maxSpeedWhileMaxTurning;
    private float maxSpeedReverse;
    private float accelSpeed; //acceleration in units/second
    private float frictionSpeed; //friction in units/second
    private float brakeSpeed; //deceleration in units/second
    private float turnFriction;

    //Turning
    private float currTurnSpeed; //current turning speed in degrees/second
    private float maxTurnSpeed; //angles/sec to turn when holding max left/right
    private float turnSpeedAccel; //units/sec to move currTurnSpeed towards target turn speed
    private float turnSpeedFriction; //units/sec to move currTurnSpeed back towards 0 when not holding left/right

    //Drifting
    private float currDriftSpeed;
    private float maxDriftSpeed; //angles/sec to turn when holding drift max left/right
    private float driftSpeedAccel; //amount to move currDriftSpeed towards held direction (angles/sec)
    private float driftSpeedFriction; //amount to move currDriftSpeed towards 0 when nothing held (angles/sec)
    #endregion

    #endregion

    private int score;
    public int Score => score;
    private int lastScore;
    public int LastScore => lastScore;
    public void AddPoints(int pointsToAdd)
    {
        lastScore = score;
        score += pointsToAdd;
    }

    void Start()
    {
        LoadStatsFromFile(mechStats);

        gravityDirection = Vector3.down;
        raycastPoints = Utils.GetChildrenOfTransform(groundRaycastParent);

        playerController = GetComponent<PlayerController>();
        npcController = GetComponent<NPCController>();

        isHuman = playerController != null;
        isLocalPlayer = isHuman; //TEMP: This is assuming only 1 player is in the scene

        //RaceController.Instance.AddRacer(this);
    }

    /// <summary>
    /// called when the race has ended for all players, and the next race scene is about to be loaded. Disable input, reset speed, etc.
    /// <summary>
    public void OnNewRaceLoading()
    {
        canMove = false;
        isDead = false;
        inLobby = true;

        raceFinished = false;
        checkpointsHit = 0;
        lapsFinished = 0;

        currTurnSpeed = 0;
        currDriftAxis = 0;
        Vector3 newModelRotation = playerModel.localEulerAngles;
        newModelRotation.y = currTurnSpeed * 0.5f + currDriftSpeed * 0.7f;
        playerModel.localEulerAngles = newModelRotation;

        currSpeed = 0;
        if (isHuman)
        {
            playerController.enabled = true;
            npcController.enabled = false;

            StartCoroutine(playerController.CameraRotate.UndoRotate());
        }
    }

    /// <summary>
    /// called when the new race scene has finished loading
    /// <summary>
    public void OnRaceFinishedLoading()
    {
        inLobby = false;
        canMove = false;
    }

    /// <summary>
    /// called when the lobby scene is loaded, should only be called on player racers, AIs will be destroyed isntead.
    /// resets varaibles to allow movement, and resets score to 0.
    /// <summary>
    public void OnEnterLobby()
    {
        inLobby = true;
        canMove = true;

        score = 0;
        lastScore = 0;
    }

    private void LoadStatsFromFile(MechStats stats)
    {
        maxSpeedWhileStraight = stats.MaxSpeedWhileStraight;
        maxSpeedWhileMaxTurning = stats.MaxSpeedWhileMaxTurning;
        maxSpeedReverse = stats.MaxSpeedReverse;
        accelSpeed = stats.AccelSpeed;

        frictionSpeed = stats.FrictionSpeed;
        brakeSpeed = stats.BrakeSpeed;
        turnFriction = stats.TurnFriction;

        maxTurnSpeed = stats.MaxTurnSpeed;
        turnSpeedAccel = stats.TurnSpeedAccel;
        turnSpeedFriction = stats.TurnFriction;

        maxDriftSpeed = stats.MaxDriftSpeed;
        driftSpeedAccel = stats.DriftSpeedAccel;
        driftSpeedFriction = stats.DriftSpeedFriction;
    }

    public void OnRaceStarted()
    {
        canMove = true;
        if (!isHuman)
            checkpointTimer = timeToHitCheckpoint;
    }

    private float currSteerAxis;
    private float currDriftAxis;
    private bool acceleratePressed;
    private bool accelerateHeld;
    private bool brakeHeld;

    public void SetInput(float _turning, float _drifting, bool _acceleratePressed, bool _accelerateHeld, bool _brakeHeld)
    {
        if (isDead == false)
        {
            currSteerAxis = _turning;
            currDriftAxis = _drifting;
            acceleratePressed = _acceleratePressed;
            accelerateHeld = _accelerateHeld;
            brakeHeld = _brakeHeld;
        }
        else
        {
            currSteerAxis = 0;
            currDriftAxis = 0;
            acceleratePressed = false;
            accelerateHeld = false;
            brakeHeld = false;
        }
    }

    void Update()
    {
        UpdateAndApplyTurning();

        float turnPercent = Mathf.Max(0, (Mathf.Abs(currTurnSpeed + currDriftSpeed) - maxTurnSpeed) / (maxDriftSpeed));
        //float turnPercent = Mathf.Abs(currDriftSpeed)/(maxDriftSpeed) - ;
        currMaxSpeed = Utils.RemapPercent(1 - turnPercent, maxSpeedWhileMaxTurning, maxSpeedWhileStraight);

        CalcAcceleration();
        CalcFriction();

        if (isLocalPlayer)
        {
            //Set speedlines based on speed
            float maxSpeedForVisuals = Utils.RemapPercent(0.35f, maxSpeedWhileMaxTurning, maxSpeedWhileStraight);
            float speedPercent = Mathf.Max(Mathf.Abs(currSpeed) - (maxSpeedForVisuals / 4.0f), 0) / (maxSpeedForVisuals - (maxSpeedForVisuals / 4.0f));
            speedLines.UpdateParticleSystem(speedPercent);

            //Set Fov based on speed
            MainCamera.Instance.SetFov(Mathf.Abs(currSpeed) / maxSpeedForVisuals);

            //set speedometer
            speedNumberText.text = (currSpeed * 100).ToString("F1") + " fasts/h";
        }
        else
        {
            if (inLobby == false)
            {
                //Update checkpoint timer to destroy stuck AIs
                if ((canMove) && (raceFinished == false) && (isDead == false))
                {
                    checkpointTimer -= Time.deltaTime;
                    if (checkpointTimer <= 0)
                    {
                        RaceController.Instance.DestroyRacer(this);
                        isDead = true;
                    }
                }
            }
        }


        //Set left/right turn particleSystems
        leftTurnFX.UpdateParticleSystem(Mathf.Abs(Mathf.Min(currDriftSpeed, 0)) / maxDriftSpeed);
        rightTurnFX.UpdateParticleSystem(Mathf.Max(0, currDriftSpeed) / maxDriftSpeed);

        //Set drift anim
        driftAnim.SetFloat("Drift", currDriftSpeed / maxDriftSpeed);



        //Set target up-direction

        //Assume player is not grounded
        int numRaysHit = 0;
        Vector3 upDirection = new Vector3();
        foreach (Transform point in raycastPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position, -transform.up, out hit, raycastLength, groundLayer))
            {
                //player is grounded, rotate to have feet face ground
                upDirection += hit.normal;
                numRaysHit++;

                //(Debug) show up normal
                //Debug.DrawRay(hit.point, hit.normal, Color.red, 1f);
                //Debug.Log("normal dir: "+hit.normal);
            }
        }

        if (numRaysHit > 0)
        {
            //Average all raycasts that hit the ground
            upDirection /= numRaysHit;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            upDirection = -gravityDirection;
        }

        //Lerp up-direction (or jump if close enough)
        if (Vector3.Angle(transform.up, upDirection) >= 2)
        {
            if (isGrounded)  //Angle between current rotation and target rotation big enough to lerp
                upDirection = Vector3.Lerp(transform.up, upDirection, lerpSpeed);
            else
                upDirection = Vector3.Lerp(transform.up, upDirection, lerpSpeed / 1.25f);
        }

        //Rotate the player to face the new "down"
        Quaternion newRotateTransform = Quaternion.FromToRotation(transform.up, upDirection);
        transform.rotation = newRotateTransform * transform.rotation;


        //Set position and scale of "shadow" object
        RaycastHit _hit;
        float newScale = 0;
        if (Physics.Raycast(raycastPoints[4].position, -transform.up, out _hit, 50, groundLayer))
        {
            shadowTransform.position = _hit.point;
            float distToGround = _hit.distance;
            newScale = Mathf.Max(1.5f - 0.1f * distToGround, 0);
        }
        shadowTransform.localScale = new Vector3(newScale, shadowTransform.localScale.y, newScale);


        //Calculate and set race position rank
        if (raceFinished || inLobby)
            return;

        //Ratio of how far the player has traveled from the previous checkpoint to the next one
        float ratioToNextCheck = 1 - (Vector3.Distance(transform.position, nextCheckpoint.transform.position) / nextCheckpoint.distToPrevCheck);
        trackPos = checkpointsHit + ratioToNextCheck;
    }

    //Called when there is only 1 racer left, stops input
    internal void LastRacerLeft()
    {
        canMove = false;
    }

    private void FixedUpdate()
    {
        //TEMP: Set gravity to face ground
        Vector3 trueGravDir = gravityDirection;
        float trueGravStr = gravityStrength;
        if (isGrounded)
        {
            gravityDirection = -transform.up;
            gravityStrength *= groundedGravityMultiplier; //Stupid way to make character not ramp on lil gaps
        }

        //Calculate what the new gravity-component of velocity should be
        Vector3 currGravProjection = Vector3.Project(rb.velocity, gravityDirection);
        Vector3 newGravProjection = currGravProjection + gravityDirection * gravityStrength;

        //Calculate what the new gravityless-component of velocity should be
        Vector3 noGravVelocity = transform.forward * currSpeed;
        noGravVelocity = noGravVelocity - Vector3.Project(noGravVelocity, gravityDirection);

        //calculate and set new composite velocity
        rb.velocity = noGravVelocity + newGravProjection;

        //reset gravity back to normal
        gravityDirection = trueGravDir;
        gravityStrength = trueGravStr;
    }

    /// <summary>
    /// Gets player input for acceleration/deceleration and updates current speed accordingly
    /// </summary>
    private void CalcAcceleration()
    {
        //TODO: Do boost
        if ((acceleratePressed) && (canMove))
        {

        }

        //Accelerate forward
        if ((accelerateHeld) && (canMove))
        {
            if (currSpeed < currMaxSpeed)
            {
                //increase speed, but don't pass maxspeed
                currSpeed = Mathf.Min(currSpeed + accelSpeed * Time.deltaTime, currMaxSpeed);
            }
        }

        //slow down/reverse
        if ((brakeHeld) && (canMove))
        {
            currSpeed = Mathf.Max(currSpeed - brakeSpeed * Time.deltaTime, -maxSpeedReverse);
        }
    }

    /// <summary>
    /// Changes currSpeed based on friction (if player is not pressing accelerate/brake)
    /// </summary>
    private void CalcFriction()
    {
        //not pressing accelerate or brake, apply friction
        if (((brakeHeld == false) && (accelerateHeld == false)) || (canMove == false))
        {
            if (currSpeed > 0)
            {
                //slow down towards 0
                currSpeed = Mathf.Max(currSpeed - frictionSpeed * Time.deltaTime, 0);
            }
            else
            {
                //speed up towards 0
                currSpeed = Mathf.Min(currSpeed + frictionSpeed * Time.deltaTime, 0);
            }
        }

        if (currSpeed > currMaxSpeed)
        {
            currSpeed = Mathf.Max(maxSpeedWhileMaxTurning, currSpeed - turnFriction * Time.deltaTime);
        }

        //turn-based friction
        //currSpeed = Mathf.Max(0, currSpeed - Mathf.Pow(currTurnSpeed + currDriftSpeed, turnFricPow) * turnFrictionMultiplier * Time.deltaTime);
    }


    /// <summary>
    /// Sets currTurnSpeed based on horizontal input and drift input, and turns player
    /// </summary>
    private void UpdateAndApplyTurning()
    {
        float inputAxis = currSteerAxis;

        float targetTurnSpeed = inputAxis * maxTurnSpeed;

        if (targetTurnSpeed < 0)
        {
            currTurnSpeed = Mathf.Max(-maxTurnSpeed, currTurnSpeed - turnSpeedAccel * Time.deltaTime);
            goto AfterTurnCalculation;
        }

        if (targetTurnSpeed > 0)
        {
            currTurnSpeed = Mathf.Min(currTurnSpeed + turnSpeedAccel * Time.deltaTime, maxTurnSpeed);
            goto AfterTurnCalculation;
        }

        //TargetTurnSpeed = 0
        if (currTurnSpeed < 0)
        {
            currTurnSpeed = Mathf.Min(currTurnSpeed + turnSpeedFriction * Time.deltaTime, 0);
            goto AfterTurnCalculation;
        }
        if (currTurnSpeed > 0)
        {
            currTurnSpeed = Mathf.Max(0, currTurnSpeed - turnSpeedFriction * Time.deltaTime);
            goto AfterTurnCalculation;
        }

    AfterTurnCalculation:

        float driftInputAxis = currDriftAxis;

        float targetDriftSpeed = driftInputAxis * maxDriftSpeed;

        float driftMultiplier = 1;

        if (targetDriftSpeed * currDriftSpeed < 0)
        {
            //trying to swap drift direction, make drift faster
            driftMultiplier = 2;
        }

        if (targetDriftSpeed < 0)
        {
            currDriftSpeed = Mathf.Max(-maxDriftSpeed, currDriftSpeed - driftSpeedAccel * Time.deltaTime * driftMultiplier);
            goto AfterDriftCalculation;
        }

        if (targetDriftSpeed > 0)
        {
            currDriftSpeed = Mathf.Min(currDriftSpeed + driftSpeedAccel * Time.deltaTime * driftMultiplier, maxDriftSpeed);
            goto AfterDriftCalculation;
        }

        //TargetDriftSpeed = 0
        if (currDriftSpeed < 0)
        {
            currDriftSpeed = Mathf.Min(currDriftSpeed + driftSpeedFriction * Time.deltaTime, 0);
            goto AfterDriftCalculation;
        }
        if (currDriftSpeed > 0)
        {
            currDriftSpeed = Mathf.Max(0, currDriftSpeed - driftSpeedFriction * Time.deltaTime);
            goto AfterDriftCalculation;
        }

    AfterDriftCalculation:

        //Spin player based on new turn speed
        float amountToTurn = (currTurnSpeed + currDriftSpeed) * Time.deltaTime;
        transform.rotation = Quaternion.AngleAxis(amountToTurn, transform.up) * transform.rotation;

        //spin player model based on if turning
        Vector3 newModelRotation = playerModel.localEulerAngles;
        newModelRotation.y = currTurnSpeed * 0.5f + currDriftSpeed * 0.7f;
        playerModel.localEulerAngles = newModelRotation;
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
        checkpointTimer = timeToHitCheckpoint;
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
            //Debug.Log("Racer " + name + " has completed lap " + lapsFinished);
            if (lapsFinished >= RaceController.Instance.TotalLaps)
            {
                //Race Finished
                raceFinished = true;
                RaceController.Instance.OnRacerFinishedFinalLap(this);

                if (isLocalPlayer)
                {
                    //swap player input for npc input
                    playerController.enabled = false;
                    npcController.enabled = true;

                    StartCoroutine(playerController.CameraRotate.DoRotate());
                }
            }
            if (isLocalPlayer)
                RaceController.Instance.UpdateLapUI(lapsFinished);
        }

        nextCheckpoint = checkpoint.NextCheckpoint;
    }
}
