using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;


public class OfflineModeButton : MonoBehaviour, IMixedRealityPointerHandler
{
    public GameObject ActivateOnStart;
    public GameObject DeactivateOnStart;

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (DeactivateOnStart != null)
        {
            DeactivateOnStart.SetActive(false);
        }
        if (ActivateOnStart != null)
        {
            ActivateOnStart.SetActive(true);
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {

    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {

    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {

    }
}
