using UnityEditor;
using UnityEngine;

/// A class for importing and converting StL series into an animation
public class STLSeriesConverter
{
    GameObject seriesGameObject;
    Mesh mesh;
    [MenuItem("Holo/Convert STL series to a .prefab")]
    public static void ConvertSTL()
    {
        Debug.Log("AAAAAAAAAA");
        STLSeriesConverter stlSeriesConverter = new STLSeriesConverter();
        STLSeriesImporter stlImporter = new STLSeriesImporter();

        stlSeriesConverter.seriesGameObject = stlImporter.GetGameObject();
        
        stlSeriesConverter.seriesGameObject.AddComponent<BlendShapeAnimation>();
        Debug.Log((stlSeriesConverter.seriesGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh != null).ToString() + "!!!");

        stlSeriesConverter.mesh = stlImporter.GetMesh();
        stlSeriesConverter.ExportMesh();

        stlSeriesConverter.ExportToPrefab();
        
        
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {
        //string save_path = EditorUtility.SaveFilePanel("Export to a  .prefab file", Application.dataPath, "", ".prefab");
        string save_path = "Assets/Hypertrophy_test.prefab";
        PrefabUtility.SaveAsPrefabAsset(seriesGameObject, save_path);
    }

    private void ExportMesh()
    {
        AssetDatabase.CreateAsset(mesh, "Assets/Hypertrophymesh_test.mesh");
    }
}