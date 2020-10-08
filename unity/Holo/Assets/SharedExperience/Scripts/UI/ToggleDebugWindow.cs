// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.WSA.Input;

public class ToggleDebugWindow : MonoBehaviour, IMixedRealityPointerHandler
{
    bool debugEnabled = false;
    public GameObject DebugWindow;
	
    // Use this for initialization
	void Start ()
    {
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque)
        {
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        }
        UpdateChildren();
	}

    int resetFrame = 0;
    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
    {
        if (obj.state.source.supportsThumbstick && obj.state.thumbstickPressed && Time.frameCount - resetFrame > 30)
        {
            resetFrame = Time.frameCount;
            OnPointerClicked(null);
        }
    }

    private void Update()
    {
        if (Input.GetButtonUp("Fire3"))
        {
            OnPointerClicked(null);
        }

        
    }

    private void UpdateChildren()
    {
        DebugWindow.SetActive(debugEnabled);
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

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        debugEnabled = !debugEnabled;
        UpdateChildren();
    }
}
