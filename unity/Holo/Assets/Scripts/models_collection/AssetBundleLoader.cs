using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetBundleLoader
{
    private AssetBundle assetBundle;
    private string bundlePath;
    private List<ModelLayer> layers;
    private Bounds? bounds;
    private int blendShapeCount;

    // All layers, available after LoadLayer
    public IEnumerable<ModelLayer> Layers
    {
        get { return new ReadOnlyCollection<ModelLayer>(layers); }
    }

    /* Bounding box of all layers, available after LoadLayer.
     * May be null, if no visible layers are present.
     */
    public Bounds? Bounds { get { return bounds;  } }

    public int BlendShapeCount { get { return blendShapeCount;  } }

    public void LoadBundle(string aBundlePath)
    {
        bundlePath = aBundlePath;
        var loadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);
        if (loadedAssetBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }
        assetBundle = loadedAssetBundle;
        LoadLayers();
    }

    private void LoadLayers()
    {
        layers = new List<ModelLayer>();
        bounds = null;
        int? newBlendShapesCount = null;
        foreach (string bundleObjectName in assetBundle.GetAllAssetNames())
        {
            if (!bundleObjectName.EndsWith(".prefab")) { continue; } // ignore other objects

            GameObject layerGameObject = assetBundle.LoadAsset<GameObject>(bundleObjectName);
            string layerDebugName = "Prefab '" + bundleObjectName + "' layer '" + layerGameObject.name + "'";

            // update bounds
            SkinnedMeshRenderer skinnedMesh = layerGameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMesh == null)
            {
                Debug.LogWarning(layerDebugName + " does not contain SkinnedMeshRenderer component, ignoring whole layer");
                continue;
            }
            // Note that we use skinnedMesh.bounds, not skinnedMesh.localBounds, because we want to preserve local rotations
            if (bounds.HasValue) {
                bounds.Value.Encapsulate(skinnedMesh.bounds);
            } else {
                bounds = skinnedMesh.bounds;
            }
            
            // check and update newBlendShapesCount
            if (newBlendShapesCount.HasValue && newBlendShapesCount.Value != skinnedMesh.sharedMesh.blendShapeCount)
            {
                Debug.LogWarning(layerDebugName + " have different number of blend shapes, " +
                    newBlendShapesCount.Value.ToString() + " versus " +
                    skinnedMesh.sharedMesh.blendShapeCount.ToString());
            }
            newBlendShapesCount = skinnedMesh.sharedMesh.blendShapeCount;

            // check BlendShapeAnimation
            BlendShapeAnimation animation = layerGameObject.GetComponent<BlendShapeAnimation>();
            if (animation == null)
            {
                Debug.LogWarning(layerDebugName + " does not contain BlendShapeAnimation component, adding");
                animation = layerGameObject.AddComponent<BlendShapeAnimation>();
            }

            // check ModelLayer existence
            ModelLayer layer = layerGameObject.GetComponent<ModelLayer>();
            if (layer == null)
            {
                layer = layerGameObject.AddComponent<ModelLayer>();
                layer.Caption = Path.GetFileNameWithoutExtension(bundleObjectName);
                layer.Simulation = bundleObjectName.Contains("dataflow") || bundleObjectName.Contains("simulation");
                if (layer.Simulation)
                {
                    int simulationsCount = layers.Count(c => c.Simulation);
                    layer.Caption = "Simulation " + (simulationsCount + 1).ToString();
                }
                Debug.LogWarning(layerDebugName + " does not contain ModelLayer component, guessing layer Caption (" + 
                    layer.Caption + ") and simulation (" + 
                    layer.Simulation.ToString() + ")");
            }

            // check Animator does not exist (we do not animate using Mecanim)
            Animator animator = layerGameObject.GetComponent<Animator>();
            if (animator != null)
            {
                Debug.LogWarning(layerDebugName + " contains Animator component, removing");
                UnityEngine.Object.Destroy(animator);
            }

            // add to layers list
            layers.Add(layer);
        }

        if (!bounds.HasValue) {
            Debug.LogWarning("Empty model, no layers with something visible");            
        } else { 
            Debug.Log("Loaded model with bounds " + bounds.ToString());
        }

        if (!newBlendShapesCount.HasValue) {
            Debug.LogWarning("Empty model, no layers with blend shapes");
            blendShapeCount = 0;
        } else { 
            blendShapeCount = newBlendShapesCount.Value;
            Debug.Log("Loaded model with blend shapes " + blendShapeCount.ToString());
        }
    }

    public void InstantiateAllLayers()
    {
        foreach (ModelLayer layer in Layers)
        {
            layer.InstantiateGameObject(null);
        }
    }
}
