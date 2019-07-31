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
    Vector3 hostPlatePosition;

    [SyncVar]
    Vector3 hostPlateScale;

    [SyncVar]
    Quaternion hostModelRotation;

    [SyncVar]
    bool hostClippingPlaneActive;

    [SyncVar]
    Transform hostClippingPlaneTransform;

    [SyncVar]
    string hostColorMap;

    [SyncVar]
    bool hostAnimationPlaying;

    [SyncVar]
    float hostAnimationTime;

    [SyncVar]
    float hostAnimationSpeed;

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
        hostPlatePosition = transform.localPosition;
        hostPlateScale = transform.localScale;

        hostClippingPlaneActive = ClipPlaneManager.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled;
        hostClippingPlaneTransform = ModelManager.ModelClipPlane.transform;

        hostColorMap = ModelManager.DataVisualizationMaterial.GetTexture("_ColorMap").name;
    }

    void Update()
    {
        if (isServer)
        {
            hostPlatePosition = transform.localPosition;
            hostPlateScale = transform.localScale;
            hostClippingPlaneActive = ClipPlaneManager.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled;
            hostInstanceName = ModelManager.InstanceName;
            if (!string.IsNullOrEmpty(ModelManager.InstanceName))
            {
                hostModelRotation = ModelManager.ModelRotation;
                hostClippingPlaneTransform = ModelManager.ModelClipPlane.transform;                
                hostColorMap = ModelManager.DataVisualizationMaterial.GetTexture("_ColorMap").name;
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
                ModelManager.ModelRotation = hostModelRotation;

                bool isClipingPlaneLocallyActive = ClipPlaneManager.ClippingPlaneState == ModelClippingPlaneControl.ClipPlaneState.Active;
                if (isClipingPlaneLocallyActive != hostClippingPlaneActive)
                    ClipPlaneManager.ClippingPlaneState = (hostClippingPlaneActive) ? ModelClippingPlaneControl.ClipPlaneState.Active : ModelClippingPlaneControl.ClipPlaneState.Disabled;
                ModelManager.ModelClipPlane.transform.SetPositionAndRotation(hostClippingPlaneTransform.localPosition, hostClippingPlaneTransform.localRotation);
                
                ModelManager.AnimationPlaying = hostAnimationPlaying;
                // synchronize AnimationTime only when paused,  otherwise it would make jittering animation on clients
                if (hostAnimationSpeed == 0f || !hostAnimationPlaying) { 
                    ModelManager.AnimationTime = hostAnimationTime;
                }
                ModelManager.AnimationSpeed = hostAnimationSpeed;

                // TODO: not synched now
                /*
                //temporary solution for dataflow
                bool isDataLayerLocallyActive = ModelManager.dataLayerInstance != null;
                if (isDataLayerLocallyActive)
                {
                    ModelManager.dataLayerInstanceAnimation.Playing = hostAnimationPlaying;
                  //ModelManager.dataLayerInstanceAnimation.CurrentTime = hostAnimationTime;
                    ModelManager.dataLayerInstanceAnimation.Speed = hostAnimationSpeed;

                    if (hostColorMap != ModelManager.DataVisualizationMaterial.GetTexture("_ColorMap").name)
                    {
                        Texture2D colorMapTex = Resources.Load<Texture2D>("Colormaps/" + hostColorMap);
                        ModelManager.DataVisualizationMaterial.SetTexture("_ColorMap", colorMapTex);
                    }
                }

                if (isDataLayerLocallyActive != hostDataLayerActive)
                {
                    if (hostDataLayerActive)
                        ModelManager.LoadDataLayerInstance(hostInstanceIndex, "dataflow");
                    else
                        ModelManager.UnloadDataLayerInstance();
                }
                */
            }
        }
    }
}
