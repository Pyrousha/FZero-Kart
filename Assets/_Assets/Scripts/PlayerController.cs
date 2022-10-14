using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private float currSpeed;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TextMeshProUGUI speedNumberText;

    [Header("Parameters")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSpeedReverse;
    [SerializeField] private float accelSpeed; //acceleration in units/second
    [SerializeField] private float frictionSpeed; //friction in units/second
    [SerializeField] private float brakeSpeed; //deceleration in units/second
    [SerializeField] private float turnSpeed; //angles/sec to turn when holding max left/right
    [SerializeField] private float driftSpeed; //angles/sec to turn when holding drift left/right

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(InputHandler.Instance.AccelerateBoost.down)
        {
            //TODO: Do boost
        }

        if(InputHandler.Instance.AccelerateBoost.held)
        {
            //Accelerate

            if (currSpeed < maxSpeed)
            {
                //increase speed, but don't pass maxspeed
                currSpeed = Mathf.Min(currSpeed + accelSpeed * Time.deltaTime, maxSpeed);
            }
        }

        if(InputHandler.Instance.Brake.held)
        {
            //slow down/reverse

            currSpeed = Mathf.Max(currSpeed - brakeSpeed * Time.deltaTime, -maxSpeedReverse);
        }

        if((InputHandler.Instance.Brake.held == false) && (InputHandler.Instance.AccelerateBoost.held == false))
        {
            //not pressing accelerate or brake, apply friction

            if(currSpeed > 0)
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

        float dRotY = InputHandler.Instance.Steering * turnSpeed * Time.deltaTime; //degrees to rotate along Y this frame
        transform.localEulerAngles += new Vector3(0, dRotY, 0);

        //set speedometer
        speedNumberText.text = (currSpeed * 100).ToString("F1");

        //Move player based on speed
        transform.position += transform.forward * currSpeed * Time.deltaTime;
    }
}
