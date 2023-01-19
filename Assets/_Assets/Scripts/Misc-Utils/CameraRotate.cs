using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    [SerializeField] private Transform targRotation;
    [SerializeField] private float lerpSpeed;

    public IEnumerator DoRotate()
    {
        while (Quaternion.Angle(transform.rotation, targRotation.rotation) > 2f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targRotation.rotation, lerpSpeed);
            yield return null;
        }

        transform.rotation = targRotation.rotation;
    }
}
