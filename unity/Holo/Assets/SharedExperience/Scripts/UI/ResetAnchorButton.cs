using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using HoloToolkit.Examples.SharingWithUNET;

public class ResetAnchorButton : MonoBehaviour, IMixedRealityPointerHandler
{

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque == false && NetworkDiscoveryWithAnchors.Instance.isServer)
        {
            UNetAnchorManager.Instance.MakeNewAnchor();
            eventData.Use();
        }
        else
        {
            Debug.Log("Only the server on hololens for now");
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
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
