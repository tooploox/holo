using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Place this as a sibling of ClippingPlane component.
 * It will enable the appropriate material (shader) behavior to perform clipping.
 */
[RequireComponent(typeof(ClippingPlane))]
public class ClippingPlaneTurnOn : MonoBehaviour
{
    void Start()
    {
        GetComponent<ClippingPlane>().mat.EnableKeyword("CLIPPING_ON");
    }
}
