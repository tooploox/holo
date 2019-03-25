using System.IO;
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
        STLSeriesConverter seriesConverter = new STLSeriesConverter();
        STLSeriesImporter seriesImporter = new STLSeriesImporter();

        seriesConverter.seriesGameObject = seriesImporter.GetGameObject();

        seriesConverter.seriesGameObject.AddComponent<BlendShapeAnimation>();

        seriesConverter.mesh = seriesImporter.GetMesh();

        seriesConverter.ExportToPrefab();
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {

        string savePath = EditorUtility.SaveFilePanelInProject("Export to a  .prefab file", seriesGameObject.name, "prefab", "");
        CheckIfNameChanged(savePath);
        ExportMesh();

        PrefabUtility.SaveAsPrefabAsset(seriesGameObject, savePath);
    }

    private void CheckIfNameChanged(string savePath)
    {
        int firstChar = savePath.LastIndexOf("/") + 1;
        int nameLength = savePath.LastIndexOf(".")  - firstChar;
        seriesGameObject.name = savePath.Substring(firstChar, nameLength);
    }

    private void ExportMesh()
    {
        AssetDatabase.CreateAsset(mesh, "Assets/" + seriesGameObject.name + ".mesh");
    }

  
}