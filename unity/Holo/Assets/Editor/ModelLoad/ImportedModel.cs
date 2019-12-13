using System.IO;
using UnityEditor;
using UnityEngine;
using ModelLoad.ModelImport;

namespace ModelLoad
{
    public class ImportedModel : SingleModel
    {
        private FileSeriesImporter seriesImporter = new FileSeriesImporter();

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            seriesImporter.ImportData(layerInfo, objectName);
            AddLayerComponent(seriesImporter.ModelGameObject, layerInfo);
            SaveFilesForExport(layerInfo, objectName, seriesImporter.ModelMesh.Get(), seriesImporter.ModelGameObject);
        }

        // Saves imported model to a Unity-friendly files, to be put in AssetBundles.
        private void SaveFilesForExport(ModelLayerInfo layerInfo, string objectName, Mesh modelMesh, GameObject modelGameObject)
        {
            string rootAssetsDir = AssetDirs.TempAssetsDir + "/" + Info.Caption;
			AssetDirs.CreateDirectory(rootAssetsDir);
		
        	string meshPath = rootAssetsDir + "/" + objectName + ".asset";
			AssetPaths.Add(meshPath);
            AssetDatabase.CreateAsset(modelMesh, meshPath);

            string gameObjectPath = rootAssetsDir + "/" + objectName + ".prefab";
            AssetPaths.Add(gameObjectPath);
            PrefabUtility.SaveAsPrefabAsset(modelGameObject, gameObjectPath);

            if (layerInfo.UseAsIcon) {
                LayerAutomaticIconGenerate(modelGameObject);
            }
        }
    }
}
