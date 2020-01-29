using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport.LayerImport;

namespace ModelImport
{
    public class ConvertedModel : ModelImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private LayerImporter layerImporter = new LayerImporter();
        private string tmpAssetDirectory;

        public ConvertedModel(string rootDirectory) : base(rootDirectory)
        {
            tmpAssetDirectory = @"Assets/tmp";
            if (!AssetDatabase.IsValidFolder(tmpAssetDirectory))
            {
                AssetDatabase.CreateFolder("Assets", "tmp");
            }
            AssetDatabase.Refresh();

        }

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            layerImporter.ImportData(layerInfo, objectName);
            AddLayerComponent(layerImporter.ModelGameObject, layerInfo);
            SaveFilesForExport(layerInfo, objectName, layerImporter.ModelMesh.Get(), layerImporter.ModelGameObject);
        }

        // Saves imported model to a Unity-friendly files, to be put in AssetBundles.
        private void SaveFilesForExport(ModelLayerInfo layerInfo, string objectName, Mesh modelMesh, GameObject modelGameObject)
        {
            string rootAssetsDir = tmpAssetDirectory + @"/" + Info.Caption;
            
            if (!AssetDatabase.IsValidFolder(rootAssetsDir))
            {
                AssetDatabase.CreateFolder(tmpAssetDirectory, Info.Caption);
            }
            AssetDatabase.Refresh();
            AssetsPath.Add(objectName + "_mesh", rootAssetsDir + @"/" + objectName + ".asset");
            AssetDatabase.CreateAsset(modelMesh, AssetsPath[objectName + "_mesh"]);

            AssetsPath.Add(objectName + "_GameObject", rootAssetsDir + @"/" + objectName + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(modelGameObject, AssetsPath[objectName + "_GameObject"]);
        }
    }
}