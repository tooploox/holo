using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;


public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    public PressableButtonHoloLens2 ButtonClippingPlane;
    public PressableButtonHoloLens2 ButtonClippingPlaneTranslation;
    public PressableButtonHoloLens2 ButtonClippingPlaneRotation;
    public ModelWithPlate ModelWithPlate;

    private BoundingBox clipPlaneQuadBbox;

    ManipulationHandler HandTranslation;

    public enum ClipPlaneState
    {
        Disabled,
        Active,
        Translation,
        Rotation
    }

    //

    public void FocusEnter(GameObject focusObject) { }
    public void FocusExit(GameObject focusObject) { }

    private ClipPlaneState clippingPlaneState = ClipPlaneState.Disabled;

    public ClipPlaneState ClippingPlaneState {
        get { return clippingPlaneState; }
        set {
            if (!ModelWithPlate.InstanceLoaded)
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
                clipPlaneQuadBbox.Active = false;
            else
                clipPlaneQuadBbox.Active = true;

            if (value == ClipPlaneState.Disabled)
            {
                ModelWithPlate.DefaultModelMaterial.DisableKeyword("CLIPPING_ON");
				ModelWithPlate.DefaultModelTransparentMaterial.DisableKeyword("CLIPPING_ON");
                ModelWithPlate.DataVisualizationMaterial.DisableKeyword("CLIPPING_ON");

                ButtonClippingPlaneTranslation.gameObject.SetActive(false);
                ButtonClippingPlaneRotation.gameObject.SetActive(false);
                HoloUtilities.SetButtonState(ButtonClippingPlane, false);
            }
            else
            {
                ModelWithPlate.DefaultModelMaterial.EnableKeyword("CLIPPING_ON");
				ModelWithPlate.DefaultModelTransparentMaterial.EnableKeyword("CLIPPING_ON");
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
        // FIXME
        //HandTranslation = GetComponent<ManipulationHandler>();
        //HandTranslation.enabled = false;
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //clipPlaneQuadBbox = cube.AddComponent<BoundingBox>();
        //ButtonClippingPlaneTranslation.gameObject.SetActive(false);
        //ButtonClippingPlaneRotation.gameObject.SetActive(false);
        //clipPlaneQuadBbox.Active = false;
    }

    public void Click(GameObject clickObj)
    {
        Debug.Log("Clicked obj: " + clickObj.name);
        switch (clickObj.name)
        {
            case "ButtonClipping":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Disabled ? ClipPlaneState.Active : ClipPlaneState.Disabled;
                break;

            case "ButtonClippingTranslation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Translation ? ClipPlaneState.Active : ClipPlaneState.Translation;
                ModelWithPlate.GetComponent<ManipulationHandler>().enabled = true;
                break;

            case "ButtonClippingRotation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Rotation ? ClipPlaneState.Active : ClipPlaneState.Rotation;
                ModelWithPlate.GetComponent<ManipulationHandler>().enabled = false;
                break;
        }
    }
}
