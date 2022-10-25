using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
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
    [Space(10)] //ground checking stuff
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundRaycastParent;
    private List<Transform> raycastPoints;
    [SerializeField] private float raycastLength;
    private bool isGrounded;
    [Space(10)] //UI + Effects stuff
    [SerializeField] private TextMeshProUGUI speedNumberText;
    [SerializeField] private ParticleSystemModifier speedLines;

    [Header("Parameters")]
    //Forward/back driving
    private float currMaxSpeed;
    [SerializeField] private float maxSpeedWhileStraight;
    [SerializeField] private float maxSpeedWhileMaxTurning;
    [SerializeField] private float maxSpeedReverse;
    [SerializeField] private float accelSpeed; //acceleration in units/second
    [Space(5)]
    [SerializeField] private float frictionSpeed; //friction in units/second
    [SerializeField] private float brakeSpeed; //deceleration in units/second
    [Space(5)]
    [SerializeField] private float turnFriction;
    //[SerializeField] private float turnFrictionMultiplier; //uses current turn speed to apply friction

    //Turning
    private float currTurnSpeed; //current turning speed in degrees/second
    [Space(25)]
    [SerializeField] private float maxTurnSpeed; //angles/sec to turn when holding max left/right
    [SerializeField] private float turnSpeedAccel; //units/sec to move currTurnSpeed towards target turn speed
    [SerializeField] private float turnSpeedFriction; //units/sec to move currTurnSpeed back towards 0 when not holding left/right

    //Drifting
    private float currDriftSpeed;
    [Space(25)]
    [SerializeField] private float maxDriftSpeed; //angles/sec to turn when holding drift left/right
    [SerializeField] private float driftSpeedAccel; //angles/sec to turn when holding drift left/right
    [SerializeField] private float driftSpeedFriction; //angles/sec to turn when holding drift left/right
    [SerializeField] private float driftScootSpeed; //units/sec to push the player away from turn direction when drifting

    // Start is called before the first frame update
    void Start()
    {
        gravityDirection = Vector3.down;

        raycastPoints = Utils.GetChildrenOfTransform(groundRaycastParent);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAndApplyTurning();

        float turnPercent = Mathf.Max(0, (Mathf.Abs(currTurnSpeed + currDriftSpeed) - maxTurnSpeed)/(maxDriftSpeed));
        //float turnPercent = Mathf.Abs(currDriftSpeed)/(maxDriftSpeed) - ;
        currMaxSpeed = Utils.RemapPercent(1 - turnPercent, maxSpeedWhileMaxTurning, maxSpeedWhileStraight);

        CalcAcceleration();
        CalcFriction();


        //Set speedlines based on speed
        float maxSpeedForVisuals = Utils.RemapPercent(0.35f, maxSpeedWhileMaxTurning, maxSpeedWhileStraight);
        float speedPercent = Mathf.Max(Mathf.Abs(currSpeed) - (maxSpeedForVisuals / 4.0f), 0) / (maxSpeedForVisuals - (maxSpeedForVisuals / 4.0f));
        speedLines.UpdateParticleSystem(speedPercent);

        //Set Fov based on speed
        MainCamera.Instance.SetFov(Mathf.Abs(currSpeed) / maxSpeedForVisuals);

        //set speedometer
        speedNumberText.text = (currSpeed * 100).ToString("F1") + " fasts/h";


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
                Debug.DrawRay(hit.point, hit.normal, Color.red, 1f);
                //Debug.Log("normal dir: "+hit.normal);
            }
        }

        if(numRaysHit > 0)
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
    }

    private void FixedUpdate()
    {
        //TEMP: Set gravity to face ground
        Vector3 trueGravDir = gravityDirection;
        float trueGravStr =  gravityStrength;
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
        if (InputHandler.Instance.AccelerateBoost.down)
        {

        }

        //Accelerate forward
        if (InputHandler.Instance.AccelerateBoost.held)
        {
            if (currSpeed < currMaxSpeed)
            {
                //increase speed, but don't pass maxspeed
                currSpeed = Mathf.Min(currSpeed + accelSpeed * Time.deltaTime, currMaxSpeed);
            }
        }

        //slow down/reverse
        if (InputHandler.Instance.Brake.held)
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
        if ((InputHandler.Instance.Brake.held == false) && (InputHandler.Instance.AccelerateBoost.held == false))
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

        if(currSpeed > currMaxSpeed)
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
        float inputAxis = InputHandler.Instance.Steering;
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

        float driftInputAxis = InputHandler.Instance.DriftAxis();
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
}
