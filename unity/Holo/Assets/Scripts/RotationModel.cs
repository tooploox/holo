using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationModel : MonoBehaviour
{
    // 360 degrees / 5 sec => 2.4 degree / frame
    public float RotationSpeed = 1.0f;
    public GameObject rotationObject;

    private bool rottationRunning = false;
    private Quaternion originalRotation = new Quaternion();

    private void OnEnable()
    {
        originalRotation = rotationObject.transform.rotation;
        rottationRunning = true;
    }

    private void OnDisable()
    {
        rottationRunning = false;
        rotationObject.transform.rotation = originalRotation;
    }

    private float GetRotationDelta()
    {
        float baseDelta = 0.4f;
        return baseDelta * RotationSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (rottationRunning)
        {
            rotationObject.transform.Rotate(0, GetRotationDelta(), 0);
        }
    }
}
