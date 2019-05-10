using UnityEngine;
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

    // Suffix specifically used on Hololens
    public string HoloLensBundlesDirectorySuffix;

    #if UNITY_EDITOR
    [MenuItem("Holo/Create Local Configuration (to specify BundlesDirectory)")]
    public static void CreateLocalConfigAsset()
    {
        LocalConfig localConfig = ScriptableObject.CreateInstance<LocalConfig>();
        AssetDatabase.CreateAsset(localConfig, "Assets/Resources/LocalConfig.asset");
    }
    #endif

    public string FinalBundlesDirectory
    {
        get
        {
            /* See https://docs.unity3d.com/Manual/windowsstore-scripts.html about using WinRT API,
             * see https://docs.microsoft.com/en-us/uwp/api/windows.storage about Windows.Storage
             * possibilities.
             * Note that code under ENABLE_WINMD_SUPPORT is not compiled inside Unity Editor,
             * check by building for UWP.
             */
            #if ENABLE_WINMD_SUPPORT
            return Windows.Storage.KnownFolders.Objects3D.Path + HoloLensBundlesDirectorySuffix;
            #else
            return BundlesDirectory;
            #endif
        }
    }
}
