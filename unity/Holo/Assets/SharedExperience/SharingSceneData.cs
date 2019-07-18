using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class SharingSceneData : NetworkBehaviour
{
    [SyncVar]
    int hostInstanceIndex;

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
    bool hostAnimationState;

    [SyncVar]
    float hostAnimationTime; // maybe we should set it only when pause or any state change

    [SyncVar]
    float hostAnimationSpeed;

    //temporary solution for dataflow
    [SyncVar]
    bool hostDataLayerActive;


    ModelWithPlate ModelManager;
    ModelClippingPlaneControl ClipPlaneManager;
    ColorMap ColorMapManager;

    void Start()
    {
        ModelManager = gameObject.GetComponent<ModelWithPlate>();
        ClipPlaneManager = ModelManager.ModelClipPlane.GetComponent<ModelClippingPlaneControl>();
        ColorMapManager = gameObject.GetComponent<ColorMap>();

        hostInstanceIndex = (ModelManager.instanceIndex.HasValue) ? ModelManager.instanceIndex.Value : -1;
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
            hostInstanceIndex = (ModelManager.instanceIndex.HasValue) ? ModelManager.instanceIndex.Value : -1;
            if (hostInstanceIndex > 0)
            {
                hostModelRotation = ModelManager.rotationBoxRig.transform.localRotation;
                hostClippingPlaneActive = ClipPlaneManager.ClippingPlaneState != ModelClippingPlaneControl.ClipPlaneState.Disabled;
                hostClippingPlaneTransform = ModelManager.ModelClipPlane.transform;                
                hostColorMap = ModelManager.DataVisualizationMaterial.GetTexture("_ColorMap").name;

                hostAnimationState = ModelManager.instanceAnimation.Playing;
                hostAnimationTime = ModelManager.instanceAnimation.CurrentTime;
                hostAnimationSpeed = ModelManager.instanceAnimation.Speed;

                //temporary solution for dataflow
                hostDataLayerActive = ModelManager.dataLayerInstance != null;
            }
        }
        else
        {
            transform.localPosition = hostPlatePosition;
            transform.localScale = hostPlateScale;

            int localInstanceIndex = (ModelManager.instanceIndex.HasValue) ? ModelManager.instanceIndex.Value : -1;
            if (localInstanceIndex != hostInstanceIndex)
            {
                if (hostInstanceIndex >= 0)
                    ModelManager.LoadInstance(hostInstanceIndex, false);
                else
                    ModelManager.UnloadInstance();
            }

            if (hostInstanceIndex >= 0)
            {
                ModelManager.rotationBoxRig.transform.localRotation = hostModelRotation;

                if((ClipPlaneManager.ClippingPlaneState == ModelClippingPlaneControl.ClipPlaneState.Active) && !hostClippingPlaneActive)
                    ClipPlaneManager.ClippingPlaneState = (hostClippingPlaneActive) ? ModelClippingPlaneControl.ClipPlaneState.Active : ModelClippingPlaneControl.ClipPlaneState.Disabled;
                ModelManager.ModelClipPlane.transform.SetPositionAndRotation(hostClippingPlaneTransform.localPosition, hostClippingPlaneTransform.localRotation);
                
                ModelManager.instanceAnimation.Playing = hostAnimationState;
              //ModelManager.instanceAnimation.CurrentTime = hostAnimationTime;
                ModelManager.instanceAnimation.Speed = hostAnimationSpeed;

                //temporary solution for dataflow
                bool isDataLayerLocallyActive = ModelManager.dataLayerInstance != null;
                if (isDataLayerLocallyActive)
                {
                    ModelManager.dataLayerInstanceAnimation.Playing = hostAnimationState;
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
            }
        }
    }
}
