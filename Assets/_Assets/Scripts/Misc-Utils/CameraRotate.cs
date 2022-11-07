using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    private Quaternion initialRotation;
    [SerializeField] private Transform targRotation;
    [SerializeField] private float lerpSpeed;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    public IEnumerator DoRotate()
    {
        while (Quaternion.Angle(transform.rotation, targRotation.rotation) > 2f)
        {
            //Debug.Log("DoRotate");

            transform.rotation = Quaternion.Lerp(transform.rotation, targRotation.rotation, lerpSpeed);
            yield return null;
        }

        transform.rotation = targRotation.rotation;
    }

    public IEnumerator UndoRotate()
    {
        while (Quaternion.Angle(transform.rotation, initialRotation) > 2f)
        {
            //Debug.Log("UndoRotate");

            transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation, lerpSpeed);
            yield return null;
        }

        transform.localRotation = initialRotation;
    }
}
