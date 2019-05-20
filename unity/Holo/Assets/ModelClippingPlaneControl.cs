using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.UX;

public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    public GameObject ClippingPlane;
    public GameObject ClippingPlaneModifyBtn;

    public GameObject LoadedModel;

    BoundingBoxRig clipPlaneQuadBbox;
    Material currentModelMaterial;
    Material currentModelMaterialClip;
    
    bool clipPlaneOn = false;
    bool clipPlaneModifyActive = false;

    void Start()
    {
        clipPlaneQuadBbox = ClippingPlane.GetComponent<BoundingBoxRig>();
        ClippingPlane.GetComponent<HandDraggable>().enabled = false;

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

            case "ButtonClippingManipulator":
                ClickedClipPlaneModify();
                break;
        }

    }

    void ClickedClipPlaneModify()
    {
        Debug.Log("Toggle modification of clip plane");
        clipPlaneModifyActive = !clipPlaneModifyActive;
        if (clipPlaneModifyActive)
        {
            clipPlaneQuadBbox.Activate();
            ClippingPlane.GetComponent<HandDraggable>().enabled = true;
        }
        else
        {
            clipPlaneQuadBbox.Deactivate();
            ClippingPlane.GetComponent<HandDraggable>().enabled = false;
        }
    }

    void ClickedClipPlane()
    { 
        const string materialsPath = "Materials/";
        GameObject currentModel = GameObject.Find("mainModel");
        if (!currentModel)
        {
            Debug.Log("No model loaded for clipping plane");
            clipPlaneOn = false;
            return;
        }

        SkinnedMeshRenderer renderer = currentModel.GetComponent<SkinnedMeshRenderer>();
        if (!clipPlaneOn)
        {
            currentModelMaterial = renderer.material;
            string clipMat = currentModelMaterial.name + "Clip";
            clipMat = clipMat.Replace(" (Instance)", ""); // We instantiate models from prefabs.
            clipMat = materialsPath + clipMat;
            currentModelMaterialClip = Resources.Load<Material>(clipMat);
            if (!currentModelMaterialClip)
            {
                Debug.LogWarning("Clipping material not loaded for material: " + clipMat);
            }
        }
        clipPlaneOn = !clipPlaneOn;
        ClippingPlane.SetActive(clipPlaneOn);
        ClippingPlaneModifyBtn.SetActive(clipPlaneOn);

        if (clipPlaneOn && currentModelMaterialClip)
        {
            renderer.material = currentModelMaterialClip;
            ClippingPlane.GetComponent<ClippingPlane>().mat = currentModelMaterialClip;
        }
        else if (!clipPlaneOn)
        {
            renderer.material = currentModelMaterial;
        }
    }

    public void ResetState()
    {
        LoadedModel = null;
        clipPlaneOn = false;
        clipPlaneModifyActive = false;
        currentModelMaterial = null;
        currentModelMaterialClip = null;

        ClippingPlane.SetActive(false);
        ClippingPlaneModifyBtn.SetActive(false);
        clipPlaneQuadBbox.Deactivate();
    }
}
