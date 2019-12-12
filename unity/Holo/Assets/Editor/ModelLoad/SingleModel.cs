using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace ModelLoad
{
    public abstract class SingleModel
    {
        public ModelInfo Info { get; protected set; }
        public List<string> AssetPaths { get; protected set; } = new List<string>();

        //Loads a single model, with its body and/or simulationData.
        public void GetModelData()
        {
            string rootDirectory = GetRootDirectory();

            AssetPaths.Clear();
            ReadInfoFile(rootDirectory);
            //Debug.Log("Reading model " + Info.Caption);
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                //Debug.Log("Reading layer " + layerInfo.Caption + " " + layerInfo.Directory + " " + layerInfo.Simulation.ToString());
                ImportLayer(layerInfo);
            }
			ImportIcon();
        }

        // Gets root directory of the model.
        protected string GetRootDirectory()
        {
            string rootDirectory = "";
            if (Application.isBatchMode)
            {
                Debug.Log("It's Batchmode!");
                string[] args = Environment.GetCommandLineArgs();
                int directoryFlagIndex = Array.FindIndex(args, a => a.Equals("-rootDirectory"));
                //Debug.Log("rootDirectoryIndex:" + directoryFlagIndex.ToString());
                rootDirectory = args[directoryFlagIndex + 1];
                if (String.IsNullOrEmpty(rootDirectory)) throw new Exception("Model's root directory has not been assigned!");
            }
            else
            {
                Debug.Log("It's not Batchmode!");
                rootDirectory = EditorUtility.OpenFolderPanel("Select model root folder with ModelInfo.json", Application.dataPath, "");
            }
            if (String.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentException("Path cannot be null!");
            }
            return rootDirectory;
        }

        protected void ReadInfoFile(string rootDirectory)
        {
            if (!File.Exists(rootDirectory + @"\" + "ModelInfo.json"))
            {
                throw new Exception("No ModelInfo.json found in root folder!");
            }

            using (StreamReader r = new StreamReader(rootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = r.ReadToEnd();
                Info = JsonConvert.DeserializeObject<ModelInfo>(json);
            }

            // convert some paths to be absolute filenames
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = rootDirectory + @"\" + layerInfo.Directory;
            }
            if (!string.IsNullOrEmpty(Info.IconFileName))
            {
                Info.IconFileName = rootDirectory + @"\" + Info.IconFileName;
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0)
            {
                throw new Exception("No layers found in ModelInfo.json file");
            }
        }

        // Imports layer (with body or simulation blendshapes).
        abstract protected void ImportLayer(ModelLayerInfo layerInfo);

        // Descendants must use this on all GameObjects representing layers
        protected void AddLayerComponent(GameObject go, ModelLayerInfo layerInfo)
        {
            ModelLayer layer = go.AddComponent<ModelLayer>();
            layer.Caption = layerInfo.Caption;
            layer.Simulation = layerInfo.Simulation;
        }
        
		// Load icon from Info.IconFileName, add it to AssetPaths
		private void ImportIcon()
		{
            if (!string.IsNullOrEmpty(Info.IconFileName))
            {
                /* Note: At one point I tried to optimize it, by detecting when
                 * icon is already in the assets
                 * ( Info.IconFileName.StartsWith(Application.dataPath) )
                 * and then just adding the existing asset-relative path to AssetPaths.
                 * 
                 * But it doesn't work: we need the file to be called "icon.asset"
                 * ("icon.png" is ignored by Unity bundle building, as it has unrecognized
                 * extension). So we need to read + write the file anyway.
                 */

                byte[] data = File.ReadAllBytes(Info.IconFileName);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(data);

                string iconAssetPath = "Assets/icon.asset";
                AssetDatabase.CreateAsset(texture, iconAssetPath);
                AssetPaths.Add(iconAssetPath);
            }
		}
    }
}
