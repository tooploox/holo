using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModelLoad
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
            if (layerInfo.UseAsIcon) {
                LayerAutomaticIconGenerate(modelInstance);
            }
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
			string prefabPath = rootAssetsDir + @"/" + objectName + ".prefab";
			AssetPaths.Add(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(modelGameObject, prefabPath);
        }
    }
}
