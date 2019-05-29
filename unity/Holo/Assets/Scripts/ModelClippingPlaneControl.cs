using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.Buttons;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.UX;

public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    public CompoundButton ClippingPlaneBtn;
    public CompoundButton ClippingPlaneTranslationBtn;
    public CompoundButton ClippingPlaneRotationBtn;
    public Material ClippingMaterial;
    public Material DefaultModelMaterial;

    public GameObject LoadedModel;

    BoundingBoxRig clipPlaneQuadBbox;

    private HandDraggable HandTranslation;
    
    
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

        ModelWithPlate.SetButtonState(ClippingPlaneRotationBtn, false);
        if (ClippingPlaneState == ClipPlaneState.Translation)
        {
            clipPlaneQuadBbox.Deactivate();
            HandTranslation.enabled = false;
            ClippingPlaneState = ClipPlaneState.Active;
            ModelWithPlate.SetButtonState(ClippingPlaneTranslationBtn, false);
        }
        else
        {
            clipPlaneQuadBbox.Activate();
            HandTranslation.enabled = true;
            ClippingPlaneState = ClipPlaneState.Translation;
            ModelWithPlate.SetButtonState(ClippingPlaneTranslationBtn, true);
        }
    }

    void ClickedClipPlaneRotation()
    {
        Debug.Log("Toggle rotation of clip plane");
        HandTranslation.enabled = false;

        ModelWithPlate.SetButtonState(ClippingPlaneTranslationBtn, false);
        if (ClippingPlaneState == ClipPlaneState.Rotation)
        {
            clipPlaneQuadBbox.Deactivate();
            ClippingPlaneState = ClipPlaneState.Active;
            ModelWithPlate.SetButtonState(ClippingPlaneRotationBtn, false);
        }
        else 
        {
            clipPlaneQuadBbox.Activate();
            ClippingPlaneState = ClipPlaneState.Rotation;
            ModelWithPlate.SetButtonState(ClippingPlaneRotationBtn, true);
        }
    }

    void ClickedClipPlane()
    { 
        if (!LoadedModel)
        {
            Debug.LogWarning("No model loaded for clipping plane");
            ClippingPlaneState = ClipPlaneState.Disabled;
            return;
        }

        SkinnedMeshRenderer modelRenderer = LoadedModel.GetComponent<SkinnedMeshRenderer>();

        ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Disabled ? ClipPlaneState.Active : ClipPlaneState.Disabled;

        if (ClippingPlaneState == ClipPlaneState.Active)
        {
            modelRenderer.material = ClippingMaterial;
            
            ClippingPlaneTranslationBtn.gameObject.SetActive(true);
            ClippingPlaneRotationBtn.gameObject.SetActive(true);
            ModelWithPlate.SetButtonState(ClippingPlaneBtn, true); 
        }
        else
        {
            modelRenderer.material = DefaultModelMaterial;
            clipPlaneQuadBbox.Deactivate();
            ClippingPlaneTranslationBtn.gameObject.SetActive(false);
            ClippingPlaneRotationBtn.gameObject.SetActive(false);
            ModelWithPlate.SetButtonState(ClippingPlaneBtn, false);
        }
    }

    public void ResetState()
    {
        LoadedModel = null;
        ClippingPlaneState = ClipPlaneState.Disabled;
        ClippingPlaneTranslationBtn.gameObject.SetActive(false);
        ClippingPlaneRotationBtn.gameObject.SetActive(false);
        clipPlaneQuadBbox.Deactivate();
    }
}
