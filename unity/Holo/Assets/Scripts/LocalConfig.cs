using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/* Local (not committed to GIT) configuration settings. */
class LocalConfig : ScriptableObject
{
	public string BundlesDirectory;

    #if UNITY_EDITOR
    [MenuItem("Holo/Create Local Configuration (to specify BundlesDirectory)")]
    public static void CreateLocalConfigAsset()
    {
        LocalConfig localConfig = new LocalConfig();
        AssetDatabase.CreateAsset(localConfig, "Assets/Resources/localConfig.asset");
    }
    #endif
}
