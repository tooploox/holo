using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

class AssetBundleLoaderTest
{
    [MenuItem("Holo/Load an Asset Bundle")]
    public static void Load()
    {
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader();

        string bundlePath = EditorUtility.OpenFilePanel("Choose AssetBundle to be loaded.", "", "");
        assetBundleLoader.LoadBundle(bundlePath);
        assetBundleLoader.LoadAssetFromBundle();
    }
}
