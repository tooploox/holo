using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.Input;
using HoloToolkit.Examples.SharingWithUNET;

public class JoinSelectedSessionButton : MonoBehaviour, IMixedRealityPointerHandler
{
    TextMesh textMesh;
    Material textMaterial;
    int textColorId;
    ScrollingSessionListUIController scrollingUIControl;
    NetworkDiscoveryWithAnchors networkDiscovery;

    private void Start()
    {
        scrollingUIControl = ScrollingSessionListUIController.instance;
        textMesh = transform.parent.GetComponentInChildren<TextMesh>();
        textMaterial = textMesh.GetComponent<MeshRenderer>().material;
        textColorId = Shader.PropertyToID("_Color");
        textMaterial.SetColor(textColorId, Color.grey);
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
    }

    private void Update()
    {
        if (networkDiscovery.running && networkDiscovery.isClient)
        {
            if (scrollingUIControl.SelectedSession != null)
            {
                textMaterial.SetColor(textColorId, new Color(0,.9f,.88f));
            }
            else
            {
                textMaterial.SetColor(textColorId, Color.grey);
            }
        }
        else
        {
            textMaterial.SetColor(textColorId, Color.grey);
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

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        ScrollingSessionListUIController.instance.JoinSelectedSession();

        //disable side menu
    }
}
