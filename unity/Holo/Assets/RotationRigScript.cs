using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationRigScript : MonoBehaviour
{
    public void OnScalingStopped()
    {
        gameObject.transform.localPosition = new Vector3(0f, gameObject.transform.localPosition.y, 0f);
    }
}
