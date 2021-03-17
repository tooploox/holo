﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Input;
using HoloToolkit.Examples.SharingWithUNET;

#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
public class UNetSharedHologram : NetworkBehaviour, IMixedRealityPointerHandler
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
{

    /// <summary>
    /// The position relative to the shared world anchor.
    /// </summary>
#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    [SyncVar(hook="xformchange")]
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    private Vector3 localPosition;

    void xformchange(Vector3 update)
    {
        Debug.Log(localPosition+" xform change "+update);
        /* FIXME Mixed reality
        if (isOpaque)
        {
           LevelControl.Instance.LevelLocalTransformChanging(localPosition, update);
        }
        */
        localPosition = update;
        
    }
    /// <summary>
    /// The rotation relative to the shared world anchor.
    /// </summary>
#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    [SyncVar]
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    private Quaternion localRotation;

    /// <summary>
    /// Sets the localPosition and localRotation on clients.
    /// </summary>
    /// <param name="postion">the localPosition to set</param>
    /// <param name="rotation">the localRotation to set</param>
#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    [Command]
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    public void CmdTransform(Vector3 postion, Quaternion rotation)
    {
        if (!isLocalPlayer)
        {
            localPosition = postion;
            localRotation = rotation;
        }
    }

    bool Moving = false;
    int layerMask;
    public Vector3 movementOffset = Vector3.zero;
    bool isOpaque;
    
    // Use this for initialization
    void Start ()
    {
        isOpaque = UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque;
        transform.SetParent(SharedCollection.Instance.transform, true);
        if (isServer)
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
        }
    }

    // Update is called once per frame
    void Update () {
       
        if (Moving)
        {
            transform.position = Vector3.Lerp(transform.position, ProposeTransformPosition(), 0.2f);
        }
        else
        {
            if (float.IsNaN(localPosition.x) ||
                float.IsNaN(localPosition.y) ||
                float.IsNaN(localPosition.z))
            {
                Debug.LogWarning("UNetSharedHologram localPosition has NaN, ignoring synchronization");
                return;
            }
            
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }
    }

    Vector3 ProposeTransformPosition()
    {
        // Put the model 3m in front of the user.
        Vector3 retval = Camera.main.transform.position + Camera.main.transform.forward * 3 + movementOffset;
        RaycastHit hitInfo;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 5.0f, layerMask))
        {
            retval = hitInfo.point+ movementOffset;
        }
        return retval;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (isOpaque == false)
        {
            Moving = !Moving;
            if (!Moving)
            {

                // Depending on if you are host or client, either setting the SyncVar (host) 
                // or calling the Cmd (client) will update the other users in the session.
                // So we have to do both.
                localPosition = transform.localPosition;
                localRotation = transform.localRotation;
                if (HoloToolkit.Examples.SharingWithUNET.PlayerController.Instance != null)
                {
                    HoloToolkit.Examples.SharingWithUNET.PlayerController.Instance.SendSharedTransform(this.gameObject, localPosition, localRotation);
                }
            }
            eventData.Use();
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}

