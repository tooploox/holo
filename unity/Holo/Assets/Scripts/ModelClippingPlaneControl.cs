using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.Buttons;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.UX;

public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    public CompoundButton ButtonClippingPlane;
    public CompoundButton ButtonClippingPlaneTranslation;
    public CompoundButton ButtonClippingPlaneRotation;
    public Material ClippingMaterial;
    public GameObject ModelWithPlate;

    BoundingBoxRig clipPlaneQuadBbox;
    HandDraggable HandTranslation;
    
    private enum ClipPlaneState
    {
        Disabled,
        Active,
        Translation,
        Rotation
    }

    ClipPlaneState ClippingPlaneState = ClipPlaneState.Disabled;

    void Start()
    {
        clipPlaneQuadBbox = GetComponent<BoundingBoxRig>();
        HandTranslation = GetComponent<HandDraggable>();
        HandTranslation.enabled = false;

        ResetState();
    }

    public void Click(GameObject clickObj)
    {
        Debug.Log("Clicked obj: " + clickObj.name);
        switch (clickObj.name)
        {
            case "ButtonClipping":
                ClickedClipPlane();
                break;

            case "ButtonClippingTranslation":
                ClickedClipPlaneTranslation();
                break;

            case "ButtonClippingRotation":
                ClickedClipPlaneRotation();
                break;
        }

    }

    void ClickedClipPlaneTranslation()
    {
        Debug.Log("Toggle translation of clip plane");

        ModelWithPlate modelWithPlate = ModelWithPlate.GetComponent<ModelWithPlate>();

        modelWithPlate.SetButtonState(ButtonClippingPlaneRotation, false);
        if (ClippingPlaneState == ClipPlaneState.Translation)
        {
            clipPlaneQuadBbox.Deactivate();
            HandTranslation.enabled = false;
            ClippingPlaneState = ClipPlaneState.Active;
            modelWithPlate.SetButtonState(ButtonClippingPlaneTranslation, false);
        }
        else
        {
            clipPlaneQuadBbox.Activate();
            HandTranslation.enabled = true;
            ClippingPlaneState = ClipPlaneState.Translation;
            modelWithPlate.SetButtonState(ButtonClippingPlaneTranslation, true);
        }
    }

    void ClickedClipPlaneRotation()
    {
        Debug.Log("Toggle rotation of clip plane");
        HandTranslation.enabled = false;

        ModelWithPlate modelWithPlate = ModelWithPlate.GetComponent<ModelWithPlate>();
        modelWithPlate.SetButtonState(ButtonClippingPlaneTranslation, false);
        if (ClippingPlaneState == ClipPlaneState.Rotation)
        {
            clipPlaneQuadBbox.Deactivate();
            ClippingPlaneState = ClipPlaneState.Active;
            modelWithPlate.SetButtonState(ButtonClippingPlaneRotation, false);
        }
        else 
        {
            clipPlaneQuadBbox.Activate();
            ClippingPlaneState = ClipPlaneState.Rotation;
            modelWithPlate.SetButtonState(ButtonClippingPlaneRotation, true);
        }
    }

    void ClickedClipPlane()
    {
        ModelWithPlate modelWithPlate = ModelWithPlate.GetComponent<ModelWithPlate>();
        if (!modelWithPlate.Instance)
        {
            Debug.LogWarning("No model loaded for clipping plane");
            ClippingPlaneState = ClipPlaneState.Disabled;
            return;
        }

        SkinnedMeshRenderer modelRenderer = modelWithPlate.Instance.GetComponent<SkinnedMeshRenderer>();

        ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Disabled ? ClipPlaneState.Active : ClipPlaneState.Disabled;

        if (ClippingPlaneState == ClipPlaneState.Active)
        {
            modelRenderer.material = ClippingMaterial;
            
            ButtonClippingPlaneTranslation.gameObject.SetActive(true);
            ButtonClippingPlaneRotation.gameObject.SetActive(true);
            modelWithPlate.SetButtonState(ButtonClippingPlane, true); 
        }
        else
        {
            modelRenderer.material = modelWithPlate.MaterialNonPreview;
            clipPlaneQuadBbox.Deactivate();
            ButtonClippingPlaneTranslation.gameObject.SetActive(false);
            ButtonClippingPlaneRotation.gameObject.SetActive(false);
            modelWithPlate.SetButtonState(ButtonClippingPlane, false);
        }
    }

    public void ResetState()
    {
        ClippingPlaneState = ClipPlaneState.Disabled;
        ButtonClippingPlaneTranslation.gameObject.SetActive(false);
        ButtonClippingPlaneRotation.gameObject.SetActive(false);
        clipPlaneQuadBbox.Deactivate();
    }
}
