using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using HoloToolkit.Examples.SharingWithUNET;

public class JoinSelectedSessionButton : MonoBehaviour, IMixedRealityPointerHandler
{
    TextMesh textMesh;
    Material textMaterial;
    int textColorId;
    ScrollingSessionListUIController scrollingUIControl;
    NetworkDiscoveryWithAnchors networkDiscovery;

    private void Start()
    {
        scrollingUIControl = ScrollingSessionListUIController.Instance;
        textMesh = transform.parent.GetComponentInChildren<TextMesh>();
        textMaterial = textMesh.GetComponent<MeshRenderer>().material;
        textColorId = Shader.PropertyToID("_Color");
        textMaterial.SetColor(textColorId, Color.grey);
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
    }

    private void Update()
    {
        if (networkDiscovery.running && networkDiscovery.isClient)
        {
            if (scrollingUIControl.SelectedSession != null)
            {
                textMaterial.SetColor(textColorId, new Color(0,.9f,.88f));
            }
            else
            {
                textMaterial.SetColor(textColorId, Color.grey);
            }
        }
        else
        {
            textMaterial.SetColor(textColorId, Color.grey);
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        ScrollingSessionListUIController.Instance.JoinSelectedSession();

        //disable side menu

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
