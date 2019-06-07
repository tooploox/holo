using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class AssetBundleEditorLoader
{
    [MenuItem("Holo/Load model from a AssetBundle's GameObject")]
    public static void LoadAssetBundle()
    {
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader();
        string bundlePath = EditorUtility.OpenFilePanel("Get The Bundle","","");
        assetBundleLoader.LoadBundle(bundlePath);
        assetBundleLoader.LoadMainGameObject();
        assetBundleLoader.InstantiateMainGameObject();
    }
}
