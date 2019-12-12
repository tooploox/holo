using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public enum LoadState {
    None,
    Metadata,
    Full
}

public class AssetBundleLoader
{
    private AssetBundle assetBundle;
    private string bundlePath;
    private List<ModelLayer> layers;
    private Bounds? bounds;
    private int blendShapeCount;
    public Texture2D Icon { get; private set; }

    /* Uniquely identifies this bundle (across all running instances of the application). */
    public string Name { get; private set; }

    public AssetBundleLoader(string aName, string aBundlePath)
    {
        Name = aName;
        bundlePath = aBundlePath;
        LoadState = LoadState.None;
    }
    
    public LoadState LoadState;

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

    public void LoadBundleMetadata()
    {
        /* Do nothing if LoadState is already at least Metadata.
         * This way this can be called multiple times. */
        if (LoadState >= LoadState.Metadata) {
            return;
        }

        assetBundle = AssetBundle.LoadFromFile(bundlePath);
        if (assetBundle == null)
        {
            throw new Exception("Failed to load AssetBundle from " + bundlePath);
        }
        LoadIcon();
        LoadState = LoadState.Metadata;
    }

    public void LoadBundle()
    {        
        /* Do nothing if LoadState is already Full.
         * This way this can be called multiple times. */
        if (LoadState >= LoadState.Full) {
            return;
        }
        LoadBundleMetadata();
        LoadLayers();
        LoadState = LoadState.Full;
    }

    // Enlarge bounds to contain newBounds.
    // bounds may be null if empty.
    private void BoundsAdd(ref Bounds? bounds, Bounds newBounds)
    {
        if (bounds.HasValue) {
            bounds.Value.Encapsulate(newBounds);
        } else {
            bounds = newBounds;
        }
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

            SkinnedMeshRenderer skinnedMesh = layerGameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMesh != null && 
                skinnedMesh.sharedMesh != null && 
                skinnedMesh.sharedMesh.blendShapeCount != 0)
            {
                // Update bounds
                // Note that we use skinnedMesh.bounds, not skinnedMesh.localBounds, because we want to preserve local rotations
                BoundsAdd(ref bounds, skinnedMesh.bounds);
            
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

                // check Animator does not exist (we do not animate using Mecanim)
                Animator animator = layerGameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.LogWarning(layerDebugName + " contains Animator component, removing");
                    UnityEngine.Object.Destroy(animator);
                }
            } else
            {
                // Model not animated using BlendShapeAnimation, search for meshes (skinned or not) inside
                var renderers = layerGameObject.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    Debug.LogWarning(layerDebugName + " has nothing visible, ignoring");
                    continue;
                }
                foreach (Renderer renderer in renderers)
                { 
                    BoundsAdd(ref bounds, renderer.bounds);
                }
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

            layer.LayerIndex = layers.Count;

            // add to layers list
            layers.Add(layer);
        }

        if (!bounds.HasValue) {
            Debug.LogWarning("Empty model, no layers with something visible");            
        } else { 
            Debug.Log("Loaded model with bounds " + bounds.ToString());
        }

        if (!newBlendShapesCount.HasValue) {
            Debug.LogWarning("Not animated model, no layers with blend shapes");
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

    public void LoadIcon()
    {
        Icon = assetBundle.LoadAsset<Texture2D>("icon.asset");
        /*
        if (Icon != null)
        {
            Debug.Log("Found icon inside bundle, size " + Icon.width + " x " + Icon.height);
        }
        */
    }
}
