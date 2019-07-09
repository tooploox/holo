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
    public ModelWithPlate ModelWithPlate;

    BoundingBoxRig clipPlaneQuadBbox;
    HandDraggable HandTranslation;

    public enum ClipPlaneState
    {
        Disabled,
        Active,
        Translation,
        Rotation
    }

    public void FocusEnter(GameObject focusObject) { }
    public void FocusExit(GameObject focusObject) { }

    private ClipPlaneState clippingPlaneState = ClipPlaneState.Disabled;

    public ClipPlaneState ClippingPlaneState {
        get { return clippingPlaneState; }
        set {
            if (!ModelWithPlate.Instance)
            {
                // This is normal if you try to turn on clipping plane before a model is loaded
                Debug.Log("No model loaded for clipping plane");
                return;
            }

            clippingPlaneState = value;

            HoloUtilities.SetButtonState(ButtonClippingPlaneTranslation, value == ClipPlaneState.Translation);
            HoloUtilities.SetButtonState(ButtonClippingPlaneRotation, value == ClipPlaneState.Rotation);

            HandTranslation.enabled = value == ClipPlaneState.Translation;

            if (value == ClipPlaneState.Active || value == ClipPlaneState.Disabled)
                clipPlaneQuadBbox.Deactivate();
            else
                clipPlaneQuadBbox.Activate();

            if (value == ClipPlaneState.Disabled)
            {
                ModelWithPlate.DefaultModelMaterial.DisableKeyword("CLIPPING_ON");
                ModelWithPlate.DataVisualizationMaterial.DisableKeyword("CLIPPING_ON");

                ButtonClippingPlaneTranslation.gameObject.SetActive(false);
                ButtonClippingPlaneRotation.gameObject.SetActive(false);
                HoloUtilities.SetButtonState(ButtonClippingPlane, false);
            }
            else
            {
                ModelWithPlate.DefaultModelMaterial.EnableKeyword("CLIPPING_ON");
                ModelWithPlate.DataVisualizationMaterial.EnableKeyword("CLIPPING_ON");

                ButtonClippingPlaneTranslation.gameObject.SetActive(true);
                ButtonClippingPlaneRotation.gameObject.SetActive(true);
                HoloUtilities.SetButtonState(ButtonClippingPlane, true);
            }

            // disable model transformation if clipping plane transformation enabled
            if (value == ClipPlaneState.Translation || value == ClipPlaneState.Rotation) 
                ModelWithPlate.ClickChangeTransformationState(ModelWithPlate.TransformationState.None);
        }
    }

    void Start()
    {
        clipPlaneQuadBbox = GetComponent<BoundingBoxRig>();
        HandTranslation = GetComponent<HandDraggable>();
        HandTranslation.enabled = false;

        ButtonClippingPlaneTranslation.gameObject.SetActive(false);
        ButtonClippingPlaneRotation.gameObject.SetActive(false);
        clipPlaneQuadBbox.Deactivate();
    }

    public void Click(GameObject clickObj)
    {
        //Debug.Log("Clicked obj: " + clickObj.name);
        switch (clickObj.name)
        {
            case "ButtonClipping":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Disabled ? ClipPlaneState.Active : ClipPlaneState.Disabled;
                break;

            case "ButtonClippingTranslation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Translation ? ClipPlaneState.Active : ClipPlaneState.Translation;
                ModelWithPlate.GetComponent<HandDraggable>().enabled = true;
                ModelWithPlate.GetComponent<HandDraggable>().IsDraggingEnabled = false;
                break;

            case "ButtonClippingRotation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Rotation ? ClipPlaneState.Active : ClipPlaneState.Rotation;
                ModelWithPlate.GetComponent<HandDraggable>().enabled = false;
                break;
        }
    }
}
