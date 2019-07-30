using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModelImport
{
    class GOModel: SingleModel
    {
        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            string absolutePath = layerInfo.Directory + "/" + layerInfo.AssetFileName;
            string objectPath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

            GameObject modelGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(objectPath);
            if (modelGameObject == null)
            {
                throw new System.Exception("Cannot load model from " + objectPath);
            }
            GameObject modelInstance = Object.Instantiate(modelGameObject);
            AddLayerComponent(modelInstance, layerInfo);
            CreatePrefab(layerInfo, modelInstance, objectName);
            GameObject.DestroyImmediate(modelInstance);
        }

        // Exports finished GameObject to a .prefab
        private void CreatePrefab(ModelLayerInfo layerInfo, GameObject modelGameObject, string objectName)
        {
            string rootAssetsDir = @"Assets/Resources/" + Info.Caption;

            if (!AssetDatabase.IsValidFolder(rootAssetsDir))
            {
                AssetDatabase.CreateFolder("Assets/Resources", Info.Caption);
            }
            AssetsPath.Add(objectName + "_GameObject", rootAssetsDir + @"/" + objectName + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(modelGameObject, AssetsPath[objectName + "_GameObject"]);
        }
    }
}
