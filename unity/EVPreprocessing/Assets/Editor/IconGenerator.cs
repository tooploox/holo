using System.IO;
using UnityEditor;
using UnityEngine;

public static class IconGenerator 
{
    // Process icon to be nicely displayed on our Holohgraphic button.
    // This adds proper alpha channel.
    private static void ProcessIcon(int w, int h, Color32[] pixels)
    {
        for (int i = 0; i < w * h; i++)
        {
            Color32 c = pixels[i];
            if (c.r == 82 &&
                c.g == 82 &&
                c.b == 82)
            {
                c.a = 0;
                pixels[i] = c;
            }
        }
    }

    // Reliably generate icon for any Unity asset.
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
         * Later: we actually use this opportunity to do some preprocessing of the texture.
         */
        Color32[] pixels = preview.GetPixels32();
        ProcessIcon(preview.width, preview.height, pixels);
        Texture2D result = new Texture2D(preview.width, preview.height, TextureFormat.ARGB32, false);
        result.SetPixels32(pixels);

        return result;
    }

    [MenuItem("EVPreprocessing/Test Generating Icon For Selected Item")]
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
