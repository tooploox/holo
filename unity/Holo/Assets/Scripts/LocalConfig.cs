using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
#endif

/* Local (not committed to GIT) configuration settings. */
class LocalConfig : ScriptableObject
{
    // Disable warning: this field is never set by code, but it is set in Unity Editor and (de)serialized
    #pragma warning disable CS0649
    // Directory with asset bundles (on non-Hololens)
    public string BundlesDirectory;
#pragma warning restore CS0649

    public static string GetBundlesDirectory()
    {
#if ENABLE_WINMD_SUPPORT
        // On Hololens, do not require the Resources/LocalConfig.asset to even exist
        return KnownFolders.Objects3D.Path;
#else
        // On PC, rely on Resources/LocalConfig.asset to define bundles path
        LocalConfig instance = Resources.Load<LocalConfig>("LocalConfig");
        if (instance == null || string.IsNullOrEmpty(instance.BundlesDirectory))
        {
            Debug.LogWarning("No \"Assets/Resources/LocalConfig.asset\", or \"BundlesDirectory\" not set. Create LocalConfig.asset from Unity Editor by \"Holo -> Create Local Configuration\"");
            return null;
        }
        return instance.BundlesDirectory;
#endif
    }

#if UNITY_EDITOR
    [MenuItem("Holo/Create Local Configuration (to specify BundlesDirectory)")]
    public static void CreateLocalConfigAsset()
    {
        LocalConfig localConfig = ScriptableObject.CreateInstance<LocalConfig>();
        AssetDatabase.CreateAsset(localConfig, "Assets/Resources/LocalConfig.asset");
    }
#endif
}
