using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using HoloToolkit.Examples.SharingWithUNET;

public class StartSessionButton : MonoBehaviour, IMixedRealityPointerHandler
{

    NetworkDiscoveryWithAnchors networkDiscovery;


    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (networkDiscovery.running)
        {
            networkDiscovery.StartHosting("SuperRad");
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

    // Use this for initialization
    void Start () {
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
    }
}
