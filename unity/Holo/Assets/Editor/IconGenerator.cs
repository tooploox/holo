using System.IO;
using UnityEditor;
using UnityEngine;

public static class IconGenerator {
    public static Texture2D GetIcon(UnityEngine.Object obj)
    {
        try {
            while (
                (AssetPreview.GetAssetPreview(obj) == null ||
                 AssetPreview.IsLoadingAssetPreview(obj.GetInstanceID())
                ) &&
                !EditorUtility.DisplayCancelableProgressBar("Generating preview",
                    "Waiting for icon to be generated", 0f))
            {
                /* TODO: this is a hack, busy waiting here.
                 * We can't use coroutine here to wait, without complicating the outside code.
                 */
            }
        } finally
        {
            EditorUtility.ClearProgressBar();
        }
        Texture2D preview = AssetPreview.GetAssetPreview(obj);

        /* Simply using AssetPreview.GetAssetPreview(obj) for result
         * results in Unity errors at later CreateAsset,
         *
         * Assertion failed on expression: '!(o->TestHideFlag(Object::kDontSaveInEditor) && (options & kAllowDontSaveObjectsToBePersistent) == 0)'
         * Unrecognized assets cannot be included in AssetBundles: "Assets/icon.asset".
         *
         * Instead we copy this texture.
         */
        Color[] pixels = preview.GetPixels();
        Texture2D result = new Texture2D(preview.width, preview.height, TextureFormat.ARGB32, false);
        result.SetPixels(pixels);

        return result;
    }

    [MenuItem("Holo/Test Generating Icon For Selected Item")]
    private static void TestGenerate()
    {
        UnityEngine.Object obj = Selection.activeObject;
        string iconPath = Application.streamingAssetsPath + "/test-icon-" + obj.name + ".png";

        Texture2D icon = GetIcon(obj);
        byte[] bytes = icon.EncodeToPNG();
        File.WriteAllBytes(iconPath, bytes);
        Debug.Log("Saved icon (" + icon.width + "x" + icon.height + ") to " + iconPath);

        UnityEngine.Object.DestroyImmediate(icon, true);
    }
}
