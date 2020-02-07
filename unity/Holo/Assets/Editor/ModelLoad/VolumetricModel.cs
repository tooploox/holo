using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ModelLoad.ModelImport;

namespace ModelLoad
{
    public class VolumetricModel : SingleModel
    {
        private FileSeriesImporter seriesImporter = new FileSeriesImporter();

        private GameObject ModelGameObject;

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            ImportData(layerInfo);
            AddLayerComponent(ModelGameObject, layerInfo);
            SaveFilesForExport(layerInfo, objectName);
        }

        private const string DefaultMaterialAsset = "Assets/GFX/Materials/RaycastMat.mat";

        private void ImportData(ModelLayerInfo layerInfo)
        {
            ModelGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ModelGameObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.1f);
            MeshRenderer meshRenderer = ModelGameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialAsset);
            VolumetricLoader volumetricLoader = ModelGameObject.AddComponent<VolumetricLoader>();

            // All this data should be set according to metadata info for microscopy data!!!
            // Now it's hardcoded !!!
            volumetricLoader.Width = 512;
            volumetricLoader.Height = 512;
            volumetricLoader.Depth = 26;

            volumetricLoader.Channels = 2;
            volumetricLoader.channel1 = new Color(255.0f, 0.0f, 0.0f);
            volumetricLoader.channel2 = new Color(0.0f, 255.0f, 0.0f);
        }

        // Prepare the go for taking preview icon.
        // We need to configure blend shapes state, material -- otherwise icon would look bad.
        private void PrepareForPreview(GameObject go)
        {
            //MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            //if (renderer != null && 
            //    renderer.sharedMesh != null &&
            //    renderer.sharedMesh.blendShapeCount != 0)
            //{
            //    renderer.SetBlendShapeWeight(0, 100f);
            //    Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialAsset);
            //    if (defaultMaterial == null)
            //    {
            //        throw new Exception("Cannot read default material asset from " + DefaultMaterialAsset);
            //    }
            //    renderer.material = defaultMaterial;
            //}
        }

        // Saves imported model to a Unity-friendly files, to be put in AssetBundles.
        private void SaveFilesForExport(ModelLayerInfo layerInfo, string objectName)
        {
            string rootAssetsDir = AssetDirs.TempAssetsDir + "/" + Info.Caption;
			AssetDirs.CreateDirectory(rootAssetsDir);

            Mesh modelMesh = ModelGameObject.GetComponent<MeshFilter>().mesh;
            string meshPath = rootAssetsDir + "/" + objectName + ".asset";
            AssetPaths.Add(meshPath);
            AssetDatabase.CreateAsset(modelMesh, meshPath);

            // TODO: FIXME!!!
            var rawDataPath = "C:/work/Hololens/holo/models/Microscopy/micro/data.raw";
            // This is strange to copy data first as Asset then copy asset to build dir
            string tmpDataPath = rootAssetsDir + "/tmp_data.bytes";
            string dataPath = rootAssetsDir + "/" + objectName + "_data.bytes";
            
            if (File.Exists(tmpDataPath))
                FileUtil.DeleteFileOrDirectory(tmpDataPath);
            FileUtil.CopyFileOrDirectory(rawDataPath, tmpDataPath);
            AssetPaths.Add(dataPath);
            AssetDatabase.Refresh();
            AssetDatabase.CopyAsset(tmpDataPath, dataPath);


            string gameObjectPath = rootAssetsDir + "/" + objectName + ".prefab";
            AssetPaths.Add(gameObjectPath);
            PrefabUtility.SaveAsPrefabAsset(ModelGameObject, gameObjectPath);

            if (layerInfo.UseAsIcon) {
                PrepareForPreview(ModelGameObject);
                LayerAutomaticIconGenerate(ModelGameObject);
            }
        }
    }
}