using System.IO;
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
        STLSeriesConverter seriesConverter = new STLSeriesConverter();
        STLSeriesImporter seriesImporter = new STLSeriesImporter();

        seriesConverter.seriesGameObject = seriesImporter.GetGameObject();
        seriesConverter.rootFileName = seriesImporter.FileName;

        seriesConverter.seriesGameObject.AddComponent<BlendShapeAnimation>();

        seriesConverter.mesh = seriesImporter.GetMesh();

        seriesConverter.ExportToPrefab();
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {

        string savePath = EditorUtility.SaveFilePanelInProject("Export to a  .prefab file", rootFileName, "prefab", "");

        ExportMesh();

        PrefabUtility.SaveAsPrefabAsset(seriesGameObject, savePath);
    }

    private void ExportMesh()
    {
        AssetDatabase.CreateAsset(mesh, "Assets/" + rootFileName + ".mesh");
    }

  
}