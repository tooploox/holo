using UnityEditor;
using UnityEngine;

/// A class for importing and converting StL series into an animation
public class STLSeriesConverter
{
    GameObject seriesGameObject;
    
    [MenuItem("Holo/Convert STL series to a .prefab")]
    public static void ConvertSTL()
    {
        STLSeriesConverter stlSeriesConverter = new STLSeriesConverter();
        STLSeriesImporter stlImporter = new STLSeriesImporter();

        stlSeriesConverter.seriesGameObject = stlImporter.GetGameObject();
        stlSeriesConverter.seriesGameObject.AddComponent<BlendShapeAnimation>();

        stlSeriesConverter.ExportToPrefab();
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {
        string save_path = EditorUtility.SaveFilePanel("Export to a  .prefab file", Application.dataPath, "", ".prefab");
        PrefabUtility.SaveAsPrefabAsset(seriesGameObject, save_path);
    }
}