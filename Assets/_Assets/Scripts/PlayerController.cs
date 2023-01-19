using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [Header("Player-Specific References")]
    [SerializeField] private CameraRotate cameraRotate;
    public CameraRotate CameraRotate => cameraRotate;

    [Header("Parameters")]
    [SerializeField] private MechRacer mechRacer;

    [Header("misc")]
    [SerializeField] private List<GameObject> thingsToDisableIfNotMine;
    [SerializeField] private List<GameObject> thingsToDisableIfMine;


    void Start()
    {
        if (!isLocalPlayer)
        {
            foreach (GameObject obj in thingsToDisableIfNotMine)
                Destroy(obj);
        }
        else
        {
            foreach (GameObject obj in thingsToDisableIfMine)
                Destroy(obj);
        }
    }

    // Update is called once per frame
    [Client]
    void Update()
    {
        if (!isLocalPlayer)
            return;

        float currSteering = InputHandler.Instance.Steering;
        float currDrift = InputHandler.Instance.DriftAxis();
        bool accelPressed = InputHandler.Instance.AccelerateBoost.down;
        bool accelHeld = InputHandler.Instance.AccelerateBoost.held;
        bool brakeHeld = InputHandler.Instance.Brake.held;

        mechRacer.SetInputOnAndFromClient(currSteering, currDrift, accelPressed, accelHeld, brakeHeld);
    }
}
