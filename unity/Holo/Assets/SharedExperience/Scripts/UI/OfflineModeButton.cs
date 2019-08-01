using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using HoloToolkit.Examples.SharingWithUNET;

public class OfflineModeButton : MonoBehaviour, IInputClickHandler
{
    public GameObject ActivateOnStart;
    public GameObject DeactivateOnStart;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (DeactivateOnStart != null) {
            DeactivateOnStart.SetActive(false);
        }
        if (ActivateOnStart != null) {
            ActivateOnStart.SetActive(true);
        }
    }
}
