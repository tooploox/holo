using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using System;

public class ScrollSessionListButton : MonoBehaviour, IMixedRealityPointerHandler
{

    public int Direction;


    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        ScrollingSessionListUIController.instance.ScrollSessions(Direction);
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
