using HoloToolkit.Examples.SharingWithUNET;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class ResetAnchorButton : MonoBehaviour, IMixedRealityPointerHandler
{
    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque == false && NetworkDiscoveryWithAnchors.Instance.isServer)
        {
            UNetAnchorManager.Instance.MakeNewAnchor();
            eventData.Use();
        }
        else
        {
            Debug.Log("Only the server on hololens for now");
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

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
