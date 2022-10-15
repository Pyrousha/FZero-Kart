using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private float currSpeed;

    private Vector3 gravityDirection;
    [SerializeField] private float gravityStrength;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerModel;
    [SerializeField] private TextMeshProUGUI speedNumberText;
    [SerializeField] private Animator driftAnim;
    [SerializeField] private ParticleSystemModifier speedLines;

    [Header("Parameters")]
    //Forward/back driving
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSpeedReverse;
    [SerializeField] private float accelSpeed; //acceleration in units/second
    [Space(5)]
    [SerializeField] private float frictionSpeed; //friction in units/second
    [SerializeField] private float brakeSpeed; //deceleration in units/second

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
    }

    // Update is called once per frame
    void Update()
    {
        CalcAcceleration();
        CalcFriction();
        UpdateAndApplyTurning();

        //Set speedlines based on speed
        float speedPercent = Mathf.Max(Mathf.Abs(currSpeed)-(maxSpeed/4.0f),0) / (maxSpeed - (maxSpeed / 4.0f));

        speedLines.UpdateParticleSystem(speedPercent);

        //Set Fov based on speed
        MainCamera.Instance.SetFov(Mathf.Abs(currSpeed) / maxSpeed);

        //set speedometer
        speedNumberText.text = (currSpeed * 100).ToString("F1");

        //Move player based on speed
        Vector3 currGravProjection = Vector3.Project(rb.velocity, gravityDirection);
        rb.velocity = (transform.forward * currSpeed) + ((currDriftSpeed / maxDriftSpeed) * driftScootSpeed * transform.right);
        //rb.velocity = Utils.ModifyVector(rb.velocity, null, currYVelocity, null);
        rb.velocity += gravityDirection * (gravityStrength * Time.deltaTime) + currGravProjection;

        //Set drift anim
        driftAnim.SetFloat("Drift", currDriftSpeed/maxDriftSpeed);
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
            if (currSpeed < maxSpeed)
            {
                //increase speed, but don't pass maxspeed
                currSpeed = Mathf.Min(currSpeed + accelSpeed * Time.deltaTime, maxSpeed);
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
    }

    /// <summary>
    /// Sets currTurnSpeed based on horizontal input and drift input, and turns player
    /// </summary>
    private void UpdateAndApplyTurning()
    {
        float inputAxis = InputHandler.Instance.Steering;
        float targetTurnSpeed = inputAxis * maxTurnSpeed;

        if(targetTurnSpeed < 0)
        {
            currTurnSpeed = Mathf.Max(-maxTurnSpeed, currTurnSpeed - turnSpeedAccel * Time.deltaTime);
            goto AfterTurnCalculation;
        }

        if(targetTurnSpeed > 0)
        {
            currTurnSpeed = Mathf.Min(currTurnSpeed + turnSpeedAccel * Time.deltaTime, maxTurnSpeed);
            goto AfterTurnCalculation;
        }

        //TargetTurnSpeed = 0
        if(currTurnSpeed < 0)
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

        if(targetDriftSpeed * currDriftSpeed < 0)
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
        transform.localEulerAngles += new Vector3(0, (currTurnSpeed + currDriftSpeed) * Time.deltaTime, 0);

        //spin player model based on if turning
        Vector3 newModelRotation = playerModel.localEulerAngles;
        newModelRotation.y = currTurnSpeed * 0.5f + currDriftSpeed * 0.7f;
        playerModel.localEulerAngles = newModelRotation;
    }
}
