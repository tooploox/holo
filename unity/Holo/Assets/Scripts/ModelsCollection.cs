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
    private string[] bundlesFiles;
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
        bundlesFiles = new string[] { };
        LocalConfig localConfig = Resources.Load<LocalConfig>("localConfig");
        if (localConfig != null && !string.IsNullOrEmpty(localConfig.BundlesDirectory))
        {
            string dir = localConfig.BundlesDirectory;
            bundlesFiles = Directory.GetFiles(dir, "*" + bundleFileSuffix);
            if (bundlesFiles.Length == 0)
            {
                Debug.LogWarning("No asset bundles found in directory \"" + dir + "\". Make sure to set correct BundlesDirectory in LocalConfig in Assets/Resources/LocalConfig.asset.");
            }
            else
            {
                Debug.Log("Found " + bundlesFiles.Length.ToString() + " asset bundles in \"" + dir + "\".");                
            }
        }
        else
        {
            Debug.LogWarning("No Assets/Resources/LocalConfig.asset. Create it from Unity Editor by \"Holo -> Create Local Configuration\"");
        }

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
        string result = Path.GetFileName(bundlesFiles[i]);
        result = result.Substring(0, result.Length - bundleFileSuffix.Length);
        return result;
    }

    public GameObject BundleLoad(int i, bool isPreview)
    {
        if (bundles[i] == null)
        {
            bundles[i] = new AssetBundleLoader();
            bundles[i].LoadBundle(bundlesFiles[i]);
        }
        return bundles[i].LoadMainGameObject();

        // TODO: material unset, isPreview unused
    }

    private void Update()
    {
        // TODO: scan asset bundles directory for changes (additions etc.)
    }
}
