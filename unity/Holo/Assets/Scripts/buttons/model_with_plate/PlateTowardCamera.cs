using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Change orientation to always see the plate buttons. */
public class PlateTowardCamera : MonoBehaviour
{
    private void Update()
    {
        /* Rotate the plate so that the menu is facing the user. */
        Vector3 Target = Camera.main.transform.position;
        Target.y = transform.position.y;
        transform.LookAt(Target);
        transform.Rotate(-90, 0, 0);
    }
}
