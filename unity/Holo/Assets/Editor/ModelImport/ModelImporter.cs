using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace ModelImport
{
    public abstract class ModelImporter
    {
        public ModelInfo Info { get; protected set; }
        public List<string> AssetPaths { get; protected set; } = new List<string>();

        //Loads a single model, with its body and/or simulationData.
        public ModelImporter()
        {
            string RootDirectory = GetRootDirectory();
        }

        public void GetModelData()
        {
            AssetsPath.Clear();
            ReadInfoFile();
            Log.Debug("Converting model: \"" + Info.Caption + "\"");
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                Log.Debug("Converting layer: \"" + layerInfo.Caption + "\" from: " + layerInfo.Directory);
                ImportLayer(layerInfo);
            }
            ImportIcon();
        }

        protected string GetRootDirectory()
        {
            string rootDirectory = "";
            if (Application.isBatchMode)
            {
                rootDirectory = GetBatchModeRootDir();
            }
            else
            {
                rootDirectory = EditorUtility.OpenFolderPanel("Select model root folder with ModelInfo.json", Application.dataPath, "");
            }
            if (String.IsNullOrEmpty(rootDirectory))
            {
                var exception = new IOException();
                Log.Error("Path cannot be null!", exception);
                throw exception;
            }
            return rootDirectory;
        }

        protected string GetBatchModeRootDir()
        {
            string[] args = Environment.GetCommandLineArgs();
            int directoryFlagIndex = Array.FindIndex(args, a => a.Equals("-rootDirectory"));
            //Debug.Log("rootDirectoryIndex:" + directoryFlagIndex.ToString());
            string rootDirectory = args[directoryFlagIndex + 1];
            if (string.IsNullOrEmpty(rootDirectory))
            {
                var exception = new IOException();
                Log.Error("Model's root directory has not been assigned!", exception);
                throw exception;
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
                Log.Error("No ModelInfo.json found in root folder!", ex);
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
                var ex = new InvalidDataException();
                Log.Error("No layers found in ModelInfo.json file", ex);
                throw ex;
                
            }
        }

        private Texture2D layerAutomaticIcon;

        protected void LayerAutomaticIconGenerate(UnityEngine.Object obj)
        { 
            layerAutomaticIcon = IconGenerator.GetIcon(obj);
        }

        // Imports layer (with body or simulation blendshapes).
        // May also set layerAutomaticIcon.
        abstract protected void ImportLayer(ModelLayerInfo layerInfo);

        // Descendants must use this on all GameObjects representing layers
        protected void AddLayerComponent(GameObject go, ModelLayerInfo layerInfo)
        {
            ModelLayer layer = go.AddComponent<ModelLayer>();
            layer.Caption = layerInfo.Caption;
            layer.Simulation = CheckIfSimulation(layerInfo.DataType);
        }

        private bool CheckIfSimulation(string simulationFlag)
        {
            string[] simulationVariants = { "true", "fibre", "flow"};
            return simulationVariants.Contains(simulationFlag);
        }

        // Load icon from Info.IconFileName, add it to AssetPaths
        private void ImportIcon()
        {
            bool hasIconFileName = !string.IsNullOrEmpty(Info.IconFileName);
            if (hasIconFileName || layerAutomaticIcon != null)
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

                Texture2D texture;
                if (hasIconFileName) {
                    byte[] data = File.ReadAllBytes(Info.IconFileName);
                    texture = new Texture2D(1, 1);
                    texture.LoadImage(data);
                } else
                {
                    texture = layerAutomaticIcon;
                }

                string iconAssetPath = AssetDirs.TempAssetsDir + "/icon.asset";
                AssetDatabase.CreateAsset(texture, iconAssetPath);
                AssetPaths.Add(iconAssetPath);
            }
        }
    }
}
