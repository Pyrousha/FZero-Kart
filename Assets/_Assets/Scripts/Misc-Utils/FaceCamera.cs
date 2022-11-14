using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Transform cameraTransform;


    // Update is called once per frame
    void Update()
    {
        cameraTransform = MainCameraTracker.MainCamTransform;

        if (cameraTransform != null)
            //transform.rotation = Quaternion.LookRotation(cameraTransform.position - transform.position);
            transform.LookAt(transform.position * 2 - cameraTransform.position, Vector3.up);
        //transform.LookAt(cameraTransform.position, Vector3.up);
    }
}
