using System;
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
        private string pathToTmp;
        public ConvertedModel(bool vtkConversion) : base()
        {
            pathToTmp = Path.GetFullPath(Application.dataPath + "/tmp/");
            if (vtkConversion)
            {
                ConvertVTKToTemp();
            }
        }

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            layerImporter.ImportData(layerInfo, objectName);
            AddLayerComponent(layerImporter.ModelGameObject, layerInfo);
            SaveFilesForExport(layerInfo, objectName, layerImporter.ModelMesh.Get(), layerImporter.ModelGameObject);
        }

        private const string DefaultMaterialAsset = "Assets/GFX/Materials/DefaultModelMaterial.mat";

        // Prepare the go for taking preview icon.
        // We need to configure blend shapes state, material -- otherwise icon would look bad.
        private void PrepareForPreview(GameObject go)
        {
            SkinnedMeshRenderer renderer = go.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null && 
                renderer.sharedMesh != null &&
                renderer.sharedMesh.blendShapeCount != 0)
            {
                renderer.SetBlendShapeWeight(0, 100f);
                Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialAsset);
                if (defaultMaterial == null)
                {
                    throw new Exception("Cannot read default material asset from " + DefaultMaterialAsset);
                }
                renderer.material = defaultMaterial;
            }
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
                PrepareForPreview(modelGameObject);
                LayerAutomaticIconGenerate(modelGameObject);
            }
        }
    }
}