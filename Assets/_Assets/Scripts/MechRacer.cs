using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

/// <summary>
/// Represents a racer for the current race, handles non-input related things related to racing (checkpoints, race position, etc)
/// </summary>
public class MechRacer : NetworkBehaviour
{
    [SerializeField] private Color playerNameColor;
    public Color PlayerNameColor => playerNameColor;
    public bool IsLocalPlayer => isLocalPlayer;

    [SyncVar] private bool isHuman;
    public bool IsHuman => isHuman;

    //States
    [SyncVar] private bool inLobby = true;
    public bool InLobby => inLobby;
    [SyncVar] private bool raceFinished = false;
    [SyncVar] private int lapsFinished = 0;
    [SyncVar] private int checkpointsHit = 0;
    [SyncVar] private bool isDead;
    [SyncVar] private bool canMove = true;

    [Server]
    public void SetPosNum(int _newPosNum)
    {
        posNum = _newPosNum;
    }
    [SyncVar] private int posNum = 0; //Position in the race (1 = 1st, 2 = 2nd, etc. 0 indicates no races have been played, default to last)

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
    private static readonly Vector3 trueGravDir = Vector3.down;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float groundedGravityMultiplier;
    [SerializeField] private float lerpSpeed;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI namePlateText;
    private Transform namePlateTransform;
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

    [SyncVar] private int score;
    public int Score => score;
    [SyncVar] private int lastScore;
    public int LastScore => lastScore;
    public void AddPoints(int pointsToAdd)
    {
        lastScore = score;
        score += pointsToAdd;
    }

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        npcController = GetComponent<NPCController>();
        isHuman = (playerController != null);

        raycastPoints = Utils.GetChildrenOfTransform(groundRaycastParent);

        //Set nameplate vars
        namePlateTransform = namePlateText.transform;
    }

    [Client]
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        SceneTransitioner.Instance.SetLocalPlayer(gameObject);
    }

    void Start()
    {
        LoadStatsFromFile(mechStats);
        gravityDirection = trueGravDir;

        namePlateText.text = gameObject.name;
        namePlateText.color = playerNameColor;

        DisableNameplate();
    }

    /// <summary>
    /// Called when the post-race lobby is loaded, enables local player's nameplate
    /// </summary>
    public void EnableNameplate()
    {
        if (isLocalPlayer)
            namePlateText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when the player is created, or when they load into the pre-race lobby
    /// </summary>
    public void DisableNameplate()
    {
        if (isLocalPlayer)
        {
            namePlateText.gameObject.SetActive(false);
        }
    }

    private void DisableSpeedometer()
    {
        if (isLocalPlayer)
            speedNumberText.gameObject.SetActive(false);
    }

    private void EnableSpeedometer()
    {
        if (isLocalPlayer)
            speedNumberText.gameObject.SetActive(true);
    }

    /// <summary>
    /// called when the race has ended for all players, and the next race scene is about to be loaded. 
    /// Disable input, reset speed, etc.
    /// </summary>
    [Server]
    public void OnNewRaceLoading_Server()
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
        }

        OnNewRaceLoading_Client();
    }

    [ClientRpc]
    private void OnNewRaceLoading_Client()
    {
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

            if (isLocalPlayer)
                StartCoroutine(playerController.CameraRotate.UndoRotate());
        }
    }

    /// <summary>
    /// called by the RaceController when the new race scene has finished loading and racers have been placed
    /// </summary>
    public void OnRaceFinishedLoading()
    {
        inLobby = false;
        canMove = false;

        raceFinished = false;
        checkpointsHit = 0;
        lapsFinished = 0;

        currTurnSpeed = 0;
        currDriftAxis = 0;
        currSpeed = 0;

        EnableSpeedometer();
    }

    /// <summary>
    /// Called when the lobby scene is about to be loaded, resets varaibles to allow movement.
    /// If this is called after the score scene, resets all player scores back to 0. (AIs should be destroyed instead)
    /// </summary>
    /// <param name="resetScore"> Should human scores be reset to 0 (if called in score scene) </param>
    public void OnEnterLobby(bool resetScore)
    {
        if (resetScore)
        {
            ResetScore();

            if (!isHuman)
                Debug.LogError("Tried to send nonplayer racer " + name + " to the preRaceLobby scene");
        }

        inLobby = true;
        canMove = true;

        DisableNameplate();

        EnableSpeedometer();
    }

    public void ResetScore()
    {
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

    [SyncVar] private float currSteerAxis;
    [SyncVar] private float currDriftAxis;
    [SyncVar] private bool acceleratePressed;
    [SyncVar] private bool accelerateHeld;
    [SyncVar] private bool brakeHeld;

    #region Input sending/recieving 
    /// <summary>
    /// Called on the server from an AI. Processes their input and send it to all clients (since input vars are synced)
    /// </summary>
    /// <param name="_turning"></param>
    /// <param name="_drifting"></param>
    /// <param name="_acceleratePressed"></param>
    /// <param name="_accelerateHeld"></param>
    /// <param name="_brakeHeld"></param>
    [Server]
    public void SetInputFromAIServer(float _turning, float _drifting, bool _acceleratePressed, bool _accelerateHeld, bool _brakeHeld)
    {
        if (isDead == false)
        {
            //No need to clamp, AI can't cheat
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

    /// <summary>
    /// Called by the player that owns this racer
    /// Sets the client-side input
    /// </summary>
    /// <param name="_turning"> Turning axis from -1 to 1 </param>
    /// <param name="_drifting"> Drifting axis from -1 to 1 </param>
    /// <param name="_acceleratePressed"> Did the player start pressing accelerate this frame </param>
    /// <param name="_accelerateHeld"> Is the player holding accelerate </param>
    /// <param name="_brakeHeld"> Is the player holding brake </param>
    [Client]
    public void SetInputOnAndFromClient(float _turning, float _drifting, bool _acceleratePressed, bool _accelerateHeld, bool _brakeHeld)
    {
        if (isDead == false)
        {
            currSteerAxis = Mathf.Clamp(_turning, -1.0f, 1.0f); //clamp to prevent cheatin
            currDriftAxis = Mathf.Clamp(_drifting, -1.0f, 1.0f);
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

        //Send input to the server, and then to any other connected players (since input vars are synced)
        SetInputOnServer(currSteerAxis, currDriftAxis, acceleratePressed, accelerateHeld, brakeHeld);
    }

    /// <summary>
    /// Called by the player that owns this racer, validates input and sets it on the server.
    /// Then sends input to all other connected players.
    /// </summary>
    /// <param name="_turning"> Turning axis from -1 to 1 </param>
    /// <param name="_drifting"> Drifting axis from -1 to 1 </param>
    /// <param name="_acceleratePressed"> Did the player start pressing accelerate this frame </param>
    /// <param name="_accelerateHeld"> Is the player holding accelerate </param>
    /// <param name="_brakeHeld"> Is the player holding brake </param>
    [Command]
    public void SetInputOnServer(float _turning, float _drifting, bool _acceleratePressed, bool _accelerateHeld, bool _brakeHeld)
    {
        if (isDead == false)
        {
            currSteerAxis = Mathf.Clamp(_turning, -1.0f, 1.0f); //clamp to prevent cheatin
            currDriftAxis = Mathf.Clamp(_drifting, -1.0f, 1.0f);
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
    #endregion

    void LateUpdate()
    {
        //Make nameplate face camera
        Transform cameraTransform = MainCameraTracker.MainCamTransform;
        if (cameraTransform != null && namePlateTransform != null)
        {
            namePlateTransform.LookAt(namePlateTransform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
        }
    }

    void Update()
    {
        UpdateAndApplyTurning();

        float turnPercent = Mathf.Max(0, (Mathf.Abs(currTurnSpeed + currDriftSpeed) - maxTurnSpeed) / (maxDriftSpeed));
        currMaxSpeed = Utils.RemapPercent(1 - turnPercent, maxSpeedWhileMaxTurning, maxSpeedWhileStraight);

        CalcAcceleration();
        CalcFriction();

        //Set left/right turn particles
        leftTurnFX.UpdateParticleSystem(Mathf.Abs(Mathf.Min(currDriftSpeed, 0)) / maxDriftSpeed);
        rightTurnFX.UpdateParticleSystem(Mathf.Max(0, currDriftSpeed) / maxDriftSpeed);

        //Set drift anim based on drift input
        driftAnim.SetFloat("Drift", currDriftSpeed / maxDriftSpeed);

        Update_Client();
        Update_Server();
    }

    [Client]
    /// <summary>
    /// Update method for the client.
    /// Sets client-side visuals and effects, such as camera FOV and speednumber text.
    /// </summary>
    private void Update_Client()
    {
        if (isHuman && isLocalPlayer)
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
    }

    [Server]
    /// <summary>
    /// Update method for the server
    /// Calculates current position in the race, and determines when to disable AI racers
    /// </summary>
    private void Update_Server()
    {
        //Calculate and set race position rank
        if (raceFinished || inLobby)
            return;

        //Ratio of how far the player has traveled from the previous checkpoint to the next one
        float ratioToNextCheck = 1 - (Vector3.Distance(transform.position, nextCheckpoint.transform.position) / nextCheckpoint.distToPrevCheck);
        trackPos = checkpointsHit + ratioToNextCheck;

        //Checkpoint timer stuff for AI racers
        if ((inLobby == false) && (isHuman == false))
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

    //Called when there is only 1 racer left, stops input
    public void LastRacerLeft()
    {
        canMove = false;
        raceFinished = true;

        DisableSpeedometer();
    }

    /// <summary>
    /// Called on all AIs that have not finished the race once the last human finishes.
    /// </summary>
    public void AIFinishRaceEarly()
    {
        raceFinished = true;
    }

    private void FixedUpdate()
    {
        #region determine if player is grounded or not, and calulate desired new "up" direction accordingly
        int numRaysHit = 0; //number of raycasts that hit the ground 
        Vector3 compositeNormalDirection = new Vector3(); // sum of all normal directions from all raycastst that hit the ground

        foreach (Transform point in raycastPoints)
        {
            if (Physics.Raycast(point.position, -transform.up, out RaycastHit hit, raycastLength, groundLayer))
            {
                //this ray hit the ground, save to calculate average normal direction of all rays that hit the ground
                compositeNormalDirection += hit.normal;
                numRaysHit++;
            }
        }

        Vector3 targetUpDir; // new up direction for the player to rotate towards

        if (numRaysHit > 0) //at least 1 ray hit the ground, so player is grounded
        {
            //Average normals from all raycasts that hit the ground
            isGrounded = true;

            compositeNormalDirection /= numRaysHit;
            targetUpDir = compositeNormalDirection;
        }
        else //no rays hit the ground, so player is not grounded
        {
            isGrounded = false;

            targetUpDir = -trueGravDir;
        }

        targetUpDir.Normalize(); //probably not needed, but can't hurt to normalize after calculation
        #endregion


        #region lerp player to rotate towards new up direction
        //Calculate new up-direction and rotate player
        Vector3 newPlayerForward = (transform.forward - Vector3.Project(transform.forward, targetUpDir)).normalized;
        Quaternion targRotation = Quaternion.LookRotation(newPlayerForward, targetUpDir);

        //Lerp up-direction (or jump if close enough)
        if (Quaternion.Angle(transform.rotation, targRotation) >= 2)
        {
            //Angle between current rotation and target rotation big enough to lerp

            if (isGrounded) //lerps slightly faster if grounded
                transform.rotation = Quaternion.Lerp(transform.rotation, targRotation, lerpSpeed);
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, targRotation, lerpSpeed / 1.25f);
        }
        else
        {
            //current and target rotation are close enough, jump value to stop lerp from going forever
            transform.rotation = targRotation;
        }
        #endregion


        #region Set position and scale of "shadow" object
        float newScale = 0;
        if (Physics.Raycast(raycastPoints[4].position, -transform.up, out RaycastHit _hit, 50, groundLayer))
        {
            shadowTransform.position = _hit.point;
            float distToGround = _hit.distance;

            //Shadow should be smaller the further away the character is from the ground
            newScale = Mathf.Max(1.5f - 0.1f * distToGround, 0);
        }
        shadowTransform.localScale = new Vector3(newScale, shadowTransform.localScale.y, newScale);
        #endregion


        #region update rigidbody velocity based on gravity direction and current mech speed
        //set gravity direction and strength based on if character is grounded
        float currGravStr = gravityStrength;
        if (isGrounded)
        {
            gravityDirection = -transform.up;
            //Gravity is significatnly stronger while the player is grounded, ensures player doesn't fly off ground when moving at high speeds
            currGravStr *= groundedGravityMultiplier;
        }
        else
        {
            gravityDirection = trueGravDir; // Vector3.Down
        }

        //Calculate what the new gravity-only-component of velocity should be
        Vector3 currGravProjection = Vector3.Project(rb.velocity, gravityDirection);
        Vector3 newGravProjection = currGravProjection + gravityDirection * currGravStr;

        //Calculate what the new gravityless-component of velocity should be
        Vector3 noGravVelocity = transform.forward * currSpeed;
        noGravVelocity -= Vector3.Project(noGravVelocity, gravityDirection);

        //calculate and set new composite velocity
        rb.velocity = noGravVelocity + newGravProjection;
        #endregion
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
            //Not hard capped to allow for things like boosts to move the player above max speed
            if (currSpeed < currMaxSpeed)
            {
                //increase speed, but don't pass maxspeed
                currSpeed = Mathf.Min(currSpeed + accelSpeed * Time.deltaTime, currMaxSpeed);
            }
        }

        //slow down/reverse
        if ((brakeHeld) && (canMove))
        {
            //Hard cap reverse speed
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
        Debug.Log("Racer " + name + " has hit checkpoint " + checkpointsHit);

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
                DisableSpeedometer();
            }
            if (isLocalPlayer)
                RaceController.Instance.UpdateLapUI(lapsFinished);
        }

        nextCheckpoint = checkpoint.NextCheckpoint;
    }
}
