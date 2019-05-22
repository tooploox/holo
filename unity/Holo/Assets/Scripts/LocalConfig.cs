using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/* Local (not committed to GIT) configuration settings. */
class LocalConfig : ScriptableObject
{
    // Disable warning: this field is never by code, but it set in Unity Editor and (de)serialized
    #pragma warning disable CS0649
    public string BundlesDirectory;
    #pragma warning restore CS0649

    public string GetBundlesDirectory()
    {
    #if ENABLE_WINMD_SUPPORT
        return Path.Combine(Application.persistentDataPath, "Bundles");
    #else
        return BundlesDirectory;
    #endif
    }
    private void OnEnable()
    {
        BundlesDirectory = Application.persistentDataPath;
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
