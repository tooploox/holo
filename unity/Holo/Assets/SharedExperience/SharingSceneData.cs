using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
public class SharingSceneData : NetworkBehaviour
{
    [SyncVar(hook = "OnChangeHostInstanceName")]
    string hostInstanceName;

    [SyncVar(hook = "OnChangeHostInstanceLayers")]
    uint hostInstanceLayers;

    [SyncVar(hook = "OnChangeHostPlatePosition")]
    Vector3 hostPlatePosition;

    [SyncVar(hook = "OnChangeHostPlateScale")]
    Vector3 hostPlateScale;

    [SyncVar(hook = "OnChangeHostModelRotation")]
    Quaternion hostModelRotation;

    [SyncVar(hook = "OnChangeHostClippingPlaneActive")]
    bool hostClippingPlaneActive;

    [SyncVar(hook = "OnChangeHostClippingPlanePostition")]
    Vector3 hostClippingPlanePosition;

    [SyncVar(hook = "OnChangeHostClippingPlaneRotation")]
    Quaternion hostClippingPlaneRotation;

    [SyncVar(hook = "OnChangeHostColorMap")]
    string hostColorMap;

    [SyncVar(hook = "OnChangeHostAnimationPlaying")]
    bool hostAnimationPlaying;

    [SyncVar(hook = "OnChangeHostAnimationTime")]
    float hostAnimationTime;

    [SyncVar(hook = "OnChangeHostAnimationSpeed")]
    float hostAnimationSpeed;

    [SyncVar(hook = "OnChangeHostTransparent")]
    bool hostTransparent;

#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)

    ModelWithPlate ModelManager;
    ModelClippingPlaneControl ClipPlaneManager;
    ColorMap ColorMapManager;

    void Start()
    {
        ModelManager = gameObject.GetComponent<ModelWithPlate>();;
        ClipPlaneManager = ModelManager.ModelClipPlane.GetComponent<ModelClippingPlaneControl>();
        ColorMapManager = gameObject.GetComponent<ColorMap>();
        hostInstanceName = ModelManager.InstanceName;
        Debug.Log("Boink: " + hostInstanceName);
        hostInstanceLayers = ModelManager.InstanceLayers;
        hostPlatePosition = transform.localPosition;
        hostPlateScale = transform.localScale;

        hostClippingPlaneActive = ClipPlaneManager.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled;
        hostClippingPlanePosition = ModelManager.ModelClipPlane.transform.localPosition;
        hostColorMap = ColorMapManager.MapName;
        singleton = this;

    }

    static private SharingSceneData singleton;
    static public SharingSceneData Singleton { get { return singleton; } }

    bool dataChanged()
    {
        return true;
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
    }

    void OnChangeHostPlatePosition(Vector3 positionChange)
    {
        if (!isServer)
        {
            transform.localPosition = positionChange;
        }
    }

    void OnChangeHostPlateScale(Vector3 scaleChange)
    {
        if (!isServer)
        {
            transform.localScale = scaleChange;
        }
    }

    void OnChangeHostInstanceName(string hostInstanceNameChange) 
    {
        if (!isServer)
        {
            string localInstanceName = ModelManager.InstanceName;
            hostInstanceName = hostInstanceNameChange;
            if (localInstanceName != hostInstanceName)
            {
                ModelManager.SetInstance(hostInstanceName);
            }
        }
    }
        
    void OnChangeHostInstanceLayers(uint hostInstanceLayersChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
        {
            ModelManager.InstanceLayers = hostInstanceLayersChange;
        }
    }
    
    void OnChangeHostModelRotation(Quaternion hostModelRotationChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
        {
            ModelManager.ModelRotation = hostModelRotationChange;
        }
    }

    void OnChangeHostClippingPlaneActive(bool hostClippingPlaneActiveChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
        {
            bool isClipingPlaneLocallyActive = ClipPlaneManager.ClippingPlaneState == ModelClippingPlaneControl.ClipPlaneState.Active;
            if (isClipingPlaneLocallyActive != hostClippingPlaneActiveChange)
            {
                ClipPlaneManager.ClippingPlaneState = hostClippingPlaneActiveChange ?
                    ModelClippingPlaneControl.ClipPlaneState.Active :
                    ModelClippingPlaneControl.ClipPlaneState.Disabled;
            }
        }
    }

    void OnChangeHostClippingPlanePostition(Vector3 hostClippingPlanePositionChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
        {
            ModelManager.ModelClipPlane.transform.localPosition = hostClippingPlanePositionChange;
        }
    }
    
    void OnChangeHostClippingPlaneRotation(Quaternion hostClippingPlaneRotationChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
            ModelManager.ModelClipPlane.transform.localRotation = hostClippingPlaneRotationChange;
    }
    
    void OnChangeHostTransparent(bool hostTransparentChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
            ModelManager.Transparent = hostTransparentChange;
    }

    void OnChangeHostAnimationPlaying(bool AnimationPlayingChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
            ModelManager.AnimationPlaying = AnimationPlayingChange;
    }
    
    void OnChangeHostAnimationTime(float hostAnimationTimeChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName) && (hostAnimationSpeed == 0f || !hostAnimationPlaying))
        {
            ModelManager.AnimationTime = hostAnimationTimeChange;
        }
    }

    void OnChangeHostAnimationSpeed(float hostAnimationSpeedChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
            ModelManager.AnimationSpeed = hostAnimationSpeedChange;
    }
    void OnChangeHostColorMap(string hostColorMapChange) 
    {
        if (!isServer && !string.IsNullOrEmpty(hostInstanceName))
            ColorMapManager.MapName = hostColorMapChange;
    }
}
