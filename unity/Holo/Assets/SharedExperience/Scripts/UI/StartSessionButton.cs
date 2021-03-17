using HoloToolkit.Examples.SharingWithUNET;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;


public class StartSessionButton : MonoBehaviour, IMixedRealityPointerHandler
{

    NetworkDiscoveryWithAnchors networkDiscovery;

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (networkDiscovery.running)
        {
            networkDiscovery.StartHosting("SuperRad");
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
    void Start () {
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
    }
}
