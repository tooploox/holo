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
            string absolutePath = Directory.GetFiles(layerInfo.Directory)[0].Replace(@"\", "/");
            string objectPath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

            var modelGameObject = (GameObject)AssetDatabase.LoadAssetAtPath(objectPath, typeof(GameObject));
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
