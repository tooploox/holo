using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    public PressableButtonHoloLens2 ButtonClippingPlane;
    public PressableButtonHoloLens2 ButtonClippingPlaneTranslation;
    public PressableButtonHoloLens2 ButtonClippingPlaneRotation;
    public ModelWithPlate ModelWithPlate;

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

    public ClipPlaneState ClippingPlaneState
    {
        get { return clippingPlaneState; }
        set
        {
            if (!ModelWithPlate.InstanceLoaded)
            {
                // This is normal if you try to turn on clipping plane before a model is loaded
                Debug.Log("No model loaded for clipping plane");
                return;
            }

            clippingPlaneState = value;

            HoloUtilities.SetButtonState(ButtonClippingPlaneTranslation, value == ClipPlaneState.Translation);
            HoloUtilities.SetButtonState(ButtonClippingPlaneRotation, value == ClipPlaneState.Rotation);

            if (value == ClipPlaneState.Translation)
            {
                GetComponent<MoveAxisConstraint>().enabled = false;
                GetComponent<RotationAxisConstraint>().enabled = true;
            }
            if (value == ClipPlaneState.Rotation)
            {

                GetComponent<BoundsControl>().enabled = true;
                GetComponent<RotationAxisConstraint>().enabled = false;
                GetComponent<MoveAxisConstraint>().enabled = true;
            }
            else
            {
                GetComponent<BoundsControl>().enabled = false;
            }

            if (value == ClipPlaneState.Disabled)
            {
                //            ModelWithPlate.DefaultModelMaterial.DisableKeyword("CLIPPING_ON");
                //ModelWithPlate.DefaultModelTransparentMaterial.DisableKeyword("CLIPPING_ON");
                //            ModelWithPlate.DataVisualizationMaterial.DisableKeyword("CLIPPING_ON");

                gameObject.SetActive(false);

                ButtonClippingPlaneTranslation.gameObject.SetActive(false);
                ButtonClippingPlaneRotation.gameObject.SetActive(false);
                HoloUtilities.SetButtonState(ButtonClippingPlane, false);
            }
            else
            {
                //            ModelWithPlate.DefaultModelMaterial.EnableKeyword("CLIPPING_ON");
                //ModelWithPlate.DefaultModelTransparentMaterial.EnableKeyword("CLIPPING_ON");
                //            ModelWithPlate.DataVisualizationMaterial.EnableKeyword("CLIPPING_ON");
                gameObject.SetActive(true);

                ButtonClippingPlaneTranslation.gameObject.SetActive(true);
                ButtonClippingPlaneRotation.gameObject.SetActive(true);
                HoloUtilities.SetButtonState(ButtonClippingPlane, true);
            }
        }
    }

    void Start()
    {

    }

    public void Click(GameObject clickObj)
    {
        Debug.Log("Clicked obj: " + clickObj.name + "Current Clipping State: " + clippingPlaneState);
        switch (clickObj.name)
        {
            case "ButtonClipping":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Disabled ? ClipPlaneState.Active : ClipPlaneState.Disabled;
                break;

            case "ButtonClippingTranslation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Translation ? ClipPlaneState.Active : ClipPlaneState.Translation;
                break;

            case "ButtonClippingRotation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Rotation ? ClipPlaneState.Active : ClipPlaneState.Rotation;
                break;
        }
    }
}
