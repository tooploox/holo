using UnityEditor;
using UnityEngine;

/// A class for importing and converting StL series into an animation
public class STLSeriesConverter
{
    GameObject seriesGameObject;
    Mesh mesh;
    string rootFileName;
    [MenuItem("Holo/Convert STL series to a .prefab")]
    public static void ConvertSTL()
    { 
        STLSeriesConverter stlSeriesConverter = new STLSeriesConverter();
        STLSeriesImporter stlSeriesImporter = new STLSeriesImporter();

        stlSeriesConverter.seriesGameObject = stlSeriesImporter.GetGameObject();
        stlSeriesConverter.rootFileName = stlSeriesImporter.FileName;

        stlSeriesConverter.seriesGameObject.AddComponent<BlendShapeAnimation>();
        

        stlSeriesConverter.mesh = stlSeriesImporter.GetMesh();
        stlSeriesConverter.ExportMesh();

        stlSeriesConverter.ExportToPrefab();
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {

        string savePath = EditorUtility.SaveFilePanelInProject("Export to a  .prefab file", rootFileName, "prefab", "");
        PrefabUtility.SaveAsPrefabAsset(seriesGameObject, savePath);
    }

    private void ExportMesh()
    {
        AssetDatabase.CreateAsset(mesh, "Assets/Hypertrophymesh_test.mesh");
    }
}