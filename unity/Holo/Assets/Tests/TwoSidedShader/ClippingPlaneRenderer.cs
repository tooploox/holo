using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClippingPlaneRenderer : MonoBehaviour
{
    // mesh we pass the values to
    public MeshRenderer meshRenderer;

    //execute every frame
    void Update()
    {
        //create plane
        Plane plane = new Plane(transform.up, transform.position);
        //transfer values from plane to vector4
        Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        //pass vector to shader
        meshRenderer.sharedMaterial.SetVector("_Plane", planeRepresentation);
    }
}

