using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

/* Add this to any GameObject in a scene, to initialize and keep updating available model bundles. */
public class ModelsCollection : MonoBehaviour
{
    private static ModelsCollection singleton;
    public static ModelsCollection Singleton
    {
        get { return singleton; }
    }

    /* Absolute filenames to asset bundles with models. */
    private string[] bundlesFiles = { }; //< never null
    private AssetBundleLoader[] bundles;

    /* Suffix to recognize bundle filename. May be an extension (with dot) or a normal filename suffix. */
    private const string bundleFileSuffix = "_bundle";

    /* Initialize bundlesFiles */
    private void Start()
    {
        singleton = this;
        LoadBundlesFiles();
    }

    private void LoadBundlesFiles()
    {
        // Set empty, but valid, state. This way we work OK even when indicated directory is not valid.
        bundlesFiles = new string[] { };
        bundles = new AssetBundleLoader[] { };

        LocalConfig localConfig = Resources.Load<LocalConfig>("LocalConfig");
        if (localConfig == null || string.IsNullOrEmpty(localConfig.GetBundlesDirectory()))
        {
            Debug.LogWarning("No \"Assets/Resources/LocalConfig.asset\", or \"BundlesDirectory\" not set. Create LocalConfig.asset from Unity Editor by \"Holo -> Create Local Configuration\"");
            return;
        }

        string dir = localConfig.GetBundlesDirectory();
        try {
            bundlesFiles = Directory.GetFiles(dir, "*" + bundleFileSuffix);
        } catch (Exception e) {
            Debug.LogWarning("Cannot read directory \"" + dir + "\":" + e.Message);
            return;
        }
        if (bundlesFiles.Length == 0) {
            Debug.LogWarning("No asset bundles found in directory \"" + dir + "\". Make sure to set correct BundlesDirectory in LocalConfig in Assets/Resources/LocalConfig.asset.");
            return;
        }

        // success, we found asset bundles
        Debug.Log("Found " + bundlesFiles.Length.ToString() + " asset bundles in \"" + dir + "\".");                
        bundles = new AssetBundleLoader[bundlesFiles.Length];
        for (int i = 0; i < bundles.Length; i++)
        {
            bundles[i] = new AssetBundleLoader(BundleName(i), bundlesFiles[i]);
        }
    }

    public int BundlesCount
    {
        get { return bundlesFiles.Length; }
    }

    /* Nice user-friendly name of the model bundle.
     * i is an index of the bundle, 0 <= i < BundlesCount.
     */
    public string BundleCaption(int i)
    {
        return BundleName(i); // TODO for now return the filename
    }

    /* Internal model bundle name (to uniquely identify it in the application).
     * i is an index of the bundle, 0 <= i < BundlesCount.
     */
    public string BundleName(int i)
    {
        if (i < 0 || i >= BundlesCount) {
            throw new Exception("Invalid bundle index " + i.ToString());
        }

        string result = Path.GetFileName(bundlesFiles[i]);
        result = result.Substring(0, result.Length - bundleFileSuffix.Length);
        return result;
    }

    // Icon associated with the bundle.
    public Texture2D BundleIcon(int i)
    {
        if (i < 0 || i >= BundlesCount) {
            throw new Exception("Invalid bundle index " + i.ToString());
        }
        bundles[i].LoadBundleMetadata();
        return bundles[i].Icon;
    }

    public AssetBundleLoader BundleLoad(string name)
    {
        int? index = FindBundle(name);
        if (!index.HasValue)
        {
            throw new Exception("Bundle named " + name + " not found in data. If this happens with shared experience, make sure that *all* devices have the same asset bundle uploaded.");
        }
        return BundleLoadByIndex(index.Value);
    }

    private AssetBundleLoader BundleLoadByIndex(int i)
    {
        if (i < 0 || i >= BundlesCount) {
            throw new Exception("Invalid bundle index " + i.ToString());
        }
        bundles[i].LoadBundle();
        return bundles[i];
    }

    /* Find bundle with matching name.
     * BundleName(result) returns this name,
     * BundleLoad(result) returns AssetBundleLoader with matching AssetBundleLoader.Name.
     * 
     * Returns null if not found.
     */
    public int? FindBundle(string name)
    {
        for (int i = 0; i < BundlesCount; i++) {
            if (BundleName(i) == name) {
                return i;
            }
        }
        return null;
    }

    private void Update()
    {
        // TODO: scan asset bundles directory for changes (additions etc.)
    }
}
