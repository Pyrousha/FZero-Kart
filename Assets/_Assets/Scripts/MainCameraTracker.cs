using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainCameraTracker: MonoBehaviour
{
    private static Transform mainCamTransform;
    public static Transform MainCamTransform => mainCamTransform;

    /// <summary>
    /// Called whenever a new scene is loaded, calculates what camera has the highest priority and saves it
    /// <summary>
    public static void CalculateHighestPriorityCamera()
    {
        float highestPrio = -10000;
        foreach (Camera cam in Camera.allCameras)
        {
            if(cam.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Base)
            if(cam.depth > highestPrio)
            {
                highestPrio = cam.depth;
                mainCamTransform = cam.transform;
            }
        }

        Debug.Log("Highest Prio Camera: "+mainCamTransform.name);
    }

    void Start()
    {
        CalculateHighestPriorityCamera();
    }
}
