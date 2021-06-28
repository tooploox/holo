using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationModel : MonoBehaviour
{
    // 360 degrees / 5 sec => 2.4 degree / frame
    public float RotationSpeed = 1.0f;
    public GameObject rotationObject;
    public GameObject clippingPlaneObject;

    private bool rottationRunning = false;
    private Quaternion originalRotation = new Quaternion();
    private Quaternion originalClippingRotation = new Quaternion();

    private void OnEnable()
    {
        originalRotation = rotationObject.transform.rotation;
        originalClippingRotation = clippingPlaneObject.transform.rotation;
        rottationRunning = true;
    }

    private void OnDisable()
    {
        rottationRunning = false;
        rotationObject.transform.rotation = originalRotation;
        clippingPlaneObject.transform.rotation = originalClippingRotation;
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
            var rotDelta = GetRotationDelta();
            rotationObject.transform.Rotate(0, rotDelta, 0, Space.World);
            clippingPlaneObject.transform.Rotate(0, rotDelta, 0, Space.World);
        }
    }
}
