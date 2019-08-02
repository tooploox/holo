using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
public class SharingSceneData : NetworkBehaviour
{
    [SyncVar]
    string hostInstanceName;

    [SyncVar]
    uint hostInstanceLayers;

    [SyncVar]
    Vector3 hostPlatePosition;

    [SyncVar]
    Vector3 hostPlateScale;

    [SyncVar]
    Quaternion hostModelRotation;

    [SyncVar]
    bool hostClippingPlaneActive;

    [SyncVar]
    Vector3 hostClippingPlanePosition;

    [SyncVar]
    Quaternion hostClippingPlaneRotation;

    [SyncVar]
    string hostColorMap;

    [SyncVar]
    bool hostAnimationPlaying;

    [SyncVar]
    float hostAnimationTime;

    [SyncVar]
    float hostAnimationSpeed;

    [SyncVar]
    bool hostTransparent;

#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)

    ModelWithPlate ModelManager;
    ModelClippingPlaneControl ClipPlaneManager;
    ColorMap ColorMapManager;

    void Start()
    {
        ModelManager = gameObject.GetComponent<ModelWithPlate>();
        ClipPlaneManager = ModelManager.ModelClipPlane.GetComponent<ModelClippingPlaneControl>();
        ColorMapManager = gameObject.GetComponent<ColorMap>();

        hostInstanceName = ModelManager.InstanceName;
        hostInstanceLayers = ModelManager.InstanceLayers;
        hostPlatePosition = transform.localPosition;
        hostPlateScale = transform.localScale;

        hostClippingPlaneActive = ClipPlaneManager.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled;
        hostClippingPlanePosition = ModelManager.ModelClipPlane.transform.localPosition;
        hostClippingPlaneRotation = ModelManager.ModelClipPlane.transform.localRotation;

        hostColorMap = ColorMapManager.MapName;
    }

    void Update()
    {
        if (isServer)
        {
            hostPlatePosition = transform.localPosition;
            hostPlateScale = transform.localScale;
            hostClippingPlaneActive = ClipPlaneManager.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled;
            hostInstanceName = ModelManager.InstanceName;
            hostInstanceLayers = ModelManager.InstanceLayers;
            if (!string.IsNullOrEmpty(ModelManager.InstanceName))
            {
                hostModelRotation = ModelManager.ModelRotation;
                hostClippingPlanePosition = ModelManager.ModelClipPlane.transform.localPosition;
                hostClippingPlaneRotation = ModelManager.ModelClipPlane.transform.localRotation;
                hostColorMap = ColorMapManager.MapName;
                hostTransparent = ModelManager.Transparent;
                hostAnimationPlaying = ModelManager.AnimationPlaying;
                hostAnimationTime = ModelManager.AnimationTime;
                hostAnimationSpeed = ModelManager.AnimationSpeed;
            }
        }
        else if(isClient)
        {
            transform.localPosition = hostPlatePosition;
            transform.localScale = hostPlateScale;

            string localInstanceName = ModelManager.InstanceName;
            string finalHostInstanceName = hostInstanceName;
            /* Turn "" into null for finalHostInstanceName.
             * It would work anyway without it (ModelManager.SetInstance accepts both "" and null),
             * but would unnecessarily keep calling ModelManager.SetInstance every frame).
             */
            if (finalHostInstanceName == "") {
                finalHostInstanceName = null;
            }
            if (localInstanceName != finalHostInstanceName)
            {
                ModelManager.SetInstance(finalHostInstanceName);
            }

            if (!string.IsNullOrEmpty(finalHostInstanceName))
            {
                ModelManager.InstanceLayers = hostInstanceLayers;
                ModelManager.ModelRotation = hostModelRotation;

                bool isClipingPlaneLocallyActive = ClipPlaneManager.ClippingPlaneState == ModelClippingPlaneControl.ClipPlaneState.Active;
                if (isClipingPlaneLocallyActive != hostClippingPlaneActive) {
                    ClipPlaneManager.ClippingPlaneState = hostClippingPlaneActive ? 
                        ModelClippingPlaneControl.ClipPlaneState.Active : 
                        ModelClippingPlaneControl.ClipPlaneState.Disabled;
                }
                ModelManager.ModelClipPlane.transform.localPosition = hostClippingPlanePosition;
                ModelManager.ModelClipPlane.transform.localRotation = hostClippingPlaneRotation;
                ModelManager.Transparent =  hostTransparent;
                ModelManager.AnimationPlaying = hostAnimationPlaying;
                // synchronize AnimationTime only when paused,  otherwise it would make jittering animation on clients
                if (hostAnimationSpeed == 0f || !hostAnimationPlaying) { 
                    ModelManager.AnimationTime = hostAnimationTime;
                }
                ModelManager.AnimationSpeed = hostAnimationSpeed;
                ColorMapManager.MapName = hostColorMap;
            }
        }
    }
}
