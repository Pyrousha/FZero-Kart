using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : Singleton<MainCamera>
{
    [SerializeField] private Transform playerHitbox;

    private Camera cam;
    [Header("Effects")]
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;

    [Header("Camera Follow Settings")]
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float rotationLerpSpeed;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = targetPoint.position;

        // Vector3 newForward;
        // // float angleBetween = Vector3.Angle(transform.forward, targetPoint.forward);
        // // if(angleBetween >= 2)
        // // {
        // //     newForward = Vector3.Lerp(transform.forward, targetPoint.forward, rotationLerpSpeed);
        // // }
        // // else
        // {
        //     newForward = targetPoint.forward;
        // }

        // //make "horizontal" turning to be facing the hitbox
        // //Vector3 hProjection = Vector3.Project(newForward, playerHitbox.right);

        // //newForward -= hProjection;

        // transform.forward = newForward;
    }

    /// <summary>
    /// Sets the camera's FOV as a percentage from minFov to maxFov
    /// </summary>
    /// <param name="speedPercent"></param> player's current speed in relation to their max speed
    public void SetFov(float speedPercent)
    {
        float newFov = Utils.RemapPercent(speedPercent, minFov, maxFov);
        cam.fieldOfView = newFov;
    }
}
