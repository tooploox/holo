using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;


public class ToggleSharingUIButton : MonoBehaviour, IMixedRealityPointerHandler
{

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        Debug.Log("Toggle sharing");
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
