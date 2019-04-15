using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelClippingPlaneControl : MonoBehaviour, IClickHandler
{
    GameObject clippingPlaneQuad;
    GameObject clippingPlane;

    Material currentModelMaterial;
    Material currentModelMaterialClip;
    
    bool clipPlaneOn = false;

    void Start()
    {
        clippingPlaneQuad = GameObject.Find("clipPlaneAdjustQuad");
        clippingPlane = GameObject.Find("modelClippingPlane");
        clippingPlane.SetActive(clipPlaneOn);

        string clipMat = "Materials/DefaultModelYellowTestClip";
        //clipMat = materialsPath + clipMat + ".mat";
        currentModelMaterialClip = Resources.Load<Material>(clipMat);
        if (currentModelMaterialClip == null)
        {
            Debug.LogWarning("Clipping material not loaded for material: " + clipMat);
        }
        else
        {
            Debug.Log("Clipping mat loaded!");
        }
    }

    public void Click(GameObject clickObj)
    {
        if(clickObj.name == "Remove")
        {
            clipPlaneOn = false;
            currentModelMaterial = null;
            currentModelMaterialClip = null;
            return;
        }

        const string materialsPath = "Materials/";
        if (clickObj.name == "clipPlaneControlBtn")
        {
            GameObject currentModel = GameObject.Find("mainModel");
            if (!currentModel)
            {
                Debug.Log("Clipping plane - no model loaded");
                clipPlaneOn = false;
                return;
            }

            SkinnedMeshRenderer renderer = currentModel.GetComponent<SkinnedMeshRenderer>();

            if (!clipPlaneOn)
            {
                
                currentModelMaterial = renderer.material;
                
                string clipMat = currentModelMaterial.name + "Clip";
                clipMat = clipMat.Replace(" (Instance)", "");
                clipMat = materialsPath + clipMat;
                currentModelMaterialClip = Resources.Load<Material>(clipMat);
                if (!currentModelMaterialClip)
                {
                    Debug.LogWarning("Clipping material not loaded for material: " + clipMat);
                }
            }
            clipPlaneOn = !clipPlaneOn;
            clippingPlane.SetActive(clipPlaneOn);

            if (clipPlaneOn && currentModelMaterialClip)
            {
                renderer.material = currentModelMaterialClip;
                clippingPlane.GetComponent<ClippingPlane>().mat = currentModelMaterialClip;
            }
            else if (!clipPlaneOn) renderer.material = currentModelMaterial;
        }
    }
}
