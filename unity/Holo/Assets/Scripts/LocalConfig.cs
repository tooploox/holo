using System;
using System.IO;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

/* Local (not committed to GIT) configuration settings. */
class LocalConfig : ScriptableObject
{
    // Disable warning: this field is never by code, but it set in Unity Editor and (de)serialized
#pragma warning disable CS0649
    // Directory with asset bundles (on non-Hololens)
    public string BundlesDirectory;
#pragma warning restore CS0649

    public string GetBundlesDirectory()
    {
#if ENABLE_WINMD_SUPPORT
        return Path.Combine(KnownFolders.Objects3D.Path, "EssentialVision");

#else
        return BundlesDirectory;
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
