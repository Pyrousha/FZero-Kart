using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Player-Specific References")]
    [SerializeField] private CameraRotate cameraRotate;
    public CameraRotate CameraRotate => cameraRotate;

    [Header("Parameters")]
    [SerializeField] private MechRacer mechRacer;

    // Update is called once per frame
    void Update()
    {
        float currSteering = InputHandler.Instance.Steering;
        float currDrift = InputHandler.Instance.DriftAxis();
        bool accelPressed = InputHandler.Instance.AccelerateBoost.down;
        bool accelHeld = InputHandler.Instance.AccelerateBoost.held;
        bool brakeHeld = InputHandler.Instance.Brake.held;

        mechRacer.SetInput(currSteering, currDrift, accelPressed, accelHeld, brakeHeld);
    }
}
