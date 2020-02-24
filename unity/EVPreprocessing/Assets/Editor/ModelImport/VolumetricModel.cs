using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ModelImport;
using Newtonsoft.Json;

namespace ModelImport
{
    public class VolumetricModel : ModelImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public VolumetricModel(string rootDirectory) : base(rootDirectory) { }

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            GameObject modelGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ImportData(modelGameObject, layerInfo);
            AddLayerComponent(modelGameObject, layerInfo);
            SaveFilesForExport(modelGameObject, layerInfo, objectName);
        }

        private void ImportData(GameObject modelGameObject, ModelLayerInfo layerInfo)
        {
            // FIXME: Scale of display cube should depend on scale of data  and should be set at load !!!
            modelGameObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.1f);          
            VolumetricMedata metadata;
            using (StreamReader r = new StreamReader(layerInfo.Directory + @"\" + "metadata.json"))
            {
                string json = r.ReadToEnd();
                metadata = JsonConvert.DeserializeObject<VolumetricMedata>(json);
            }

            VolumetricModelLayer modelLayer = modelGameObject.AddComponent<VolumetricModelLayer>();
            modelLayer.Width = metadata.width;
            modelLayer.Height = metadata.height;
            modelLayer.Depth = metadata.depth;
            modelLayer.Channels = metadata.channels;
        }

        // Prepare the go for taking preview icon.
        // We need to configure blend shapes state, material -- otherwise icon would look bad.
        private void PrepareForPreview(GameObject go)
        {
            // FIXME: Preview icon generation !!!
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
        private void SaveFilesForExport(GameObject modelGameObject, ModelLayerInfo layerInfo, string objectName)
        {
            string rootAssetsDir = AssetDirs.TempAssetsDir + "/" + Info.Caption;
			AssetDirs.CreateAssetDirectory(rootAssetsDir);

            Mesh modelMesh = modelGameObject.GetComponent<MeshFilter>().mesh;
            string meshPath = rootAssetsDir + "/" + objectName + ".asset";
            AssetPaths.Add(meshPath);
            AssetDatabase.CreateAsset(modelMesh, meshPath);

            string rawDataPath = layerInfo.Directory + @"\" + "data.raw";
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
            PrefabUtility.SaveAsPrefabAsset(modelGameObject, gameObjectPath);

            if (layerInfo.UseAsIcon) {
                PrepareForPreview(modelGameObject);
                LayerAutomaticIconGenerate(modelGameObject);
            }
        }
    }
}