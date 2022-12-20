using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : Singleton<MainCamera>
{
    private Camera cam;
    [Header("Effects")]
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
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
