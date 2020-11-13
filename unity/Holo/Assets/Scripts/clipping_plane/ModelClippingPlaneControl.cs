using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    public PressableButtonHoloLens2 ButtonClippingPlane;
    public PressableButtonHoloLens2 ButtonClippingPlaneManipulation;
    public ModelWithPlate ModelWithPlate;

    public enum ClipPlaneState
    {
        Disabled,
        Active,
        Manipulation
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

            HoloUtilities.SetButtonState(ButtonClippingPlaneManipulation, value == ClipPlaneState.Manipulation);

            if (value == ClipPlaneState.Manipulation)
            {
                GetComponent<ObjectManipulator>().enabled = true;
                GetComponent<BoundsControl>().enabled = true;
                gameObject.transform.Find("Quad").gameObject.SetActive(true);
            }
            else
            {
                GetComponent<ObjectManipulator>().enabled = false;
                GetComponent<BoundsControl>().enabled = false;
                gameObject.transform.Find("Quad").gameObject.SetActive(false);
            }

            if (value == ClipPlaneState.Disabled)
            {
                gameObject.SetActive(false);

                ButtonClippingPlaneManipulation.gameObject.SetActive(false);
                HoloUtilities.SetButtonState(ButtonClippingPlane, false);
            }
            else
            {
                gameObject.SetActive(true);

                ButtonClippingPlaneManipulation.gameObject.SetActive(true);
                HoloUtilities.SetButtonState(ButtonClippingPlane, true);
            }
        }
    }

    public void Click(GameObject clickObj)
    {
        switch (clickObj.name)
        {
            case "ButtonClipping":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Disabled ? ClipPlaneState.Active : ClipPlaneState.Disabled;
                break;

            case "ButtonClippingManipulation":
                ClippingPlaneState = ClippingPlaneState == ClipPlaneState.Manipulation ? ClipPlaneState.Active : ClipPlaneState.Manipulation;
                break;
        }
    }
}
