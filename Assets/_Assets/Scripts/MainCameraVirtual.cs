using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainCameraVirtual : Singleton<MainCameraVirtual>
{
    private CinemachineVirtualCamera cam;
    [Header("References")]
    [SerializeField] private Transform lookTransform;
    [Header("Effects")]
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;
    [SerializeField] private float maxLookDisplacement;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
    }

    /// <summary>
    /// Sets the camera's FOV as a percentage from minFov to maxFov
    /// </summary>
    /// <param name="speedPercent"></param> player's current speed in relation to their max speed
    public void SetFov(float speedPercent)
    {
        float newFov = Utils.RemapPercent(speedPercent, minFov, maxFov);
        cam.m_Lens.FieldOfView = newFov;
    }

    public void SetLook(float turnSignedPercent)
    {
        Debug.Log(turnSignedPercent);
        float newLook = Mathf.Sign(turnSignedPercent) * Utils.RemapPercent(Mathf.Abs(turnSignedPercent), 0, maxLookDisplacement);
        lookTransform.localPosition = new Vector3(newLook, lookTransform.localPosition.y, lookTransform.localPosition.z);
    }
}
