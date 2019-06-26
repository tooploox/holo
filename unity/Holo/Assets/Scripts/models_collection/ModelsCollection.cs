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
    }

    public int BundlesCount
    {
        get { return bundlesFiles.Length; }
    }

    /* Nice user-friendly name of the model bundle.
     * i is an index of the bundle, 0 <= i < BundlesCount.
     * */
    public string BundleCaption(int i)
    {
        if (i < 0 || i >= BundlesCount) {
            throw new Exception("Invalid bundle index " + i.ToString());
        }

        string result = Path.GetFileName(bundlesFiles[i]);
        result = result.Substring(0, result.Length - bundleFileSuffix.Length);
        return result;
    }

    public GameObject BundleLoad(int i, bool isPreview)
    {
        if (i < 0 || i >= BundlesCount) {
            throw new Exception("Invalid bundle index " + i.ToString());
        }

        if (bundles[i] == null)
        {
            bundles[i] = new AssetBundleLoader();
            bundles[i].LoadBundle(bundlesFiles[i]);
        }

        return bundles[i].LoadMainGameObject();
    }

    public GameObject BundleLoadDataLayer(int i, string dataLayerSufix)
    {
        if (i < 0 || i >= BundlesCount)
        {
            throw new Exception("Invalid bundle index " + i.ToString());
        }

        if (bundles[i] == null)
        {
            bundles[i] = new AssetBundleLoader();
            bundles[i].LoadBundle(bundlesFiles[i]);
        }

        return bundles[i].LoadGameObject(dataLayerSufix);

    }

    private void Update()
    {
        // TODO: scan asset bundles directory for changes (additions etc.)
    }
}
