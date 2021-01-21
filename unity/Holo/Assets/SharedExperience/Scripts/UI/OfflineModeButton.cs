using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class OfflineModeButton : MonoBehaviour, IMixedRealityPointerHandler
{
    public GameObject ActivateOnStart;
    public GameObject DeactivateOnStart;

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (DeactivateOnStart != null)
        {
            DeactivateOnStart.SetActive(false);
        }
        if (ActivateOnStart != null)
        {
            ActivateOnStart.SetActive(true);
        }
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
