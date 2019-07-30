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
        public Dictionary<string, string> AssetsPath { get; protected set; } = new Dictionary<string, string>();

        //Loads a single model, with its body and/or simulationData.
        public void GetModelData()
        {
            string rootDirectory = GetRootDirectory();

            AssetsPath.Clear();
            ReadInfoFile(rootDirectory);
            //Debug.Log("Reading model " + Info.Caption);
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                //Debug.Log("Reading layer " + layerInfo.Caption + " " + layerInfo.Directory + " " + layerInfo.Simulation.ToString());
                ImportLayer(layerInfo);
            }
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
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = rootDirectory + @"\" + layerInfo.Directory;
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0)
            {
                throw new Exception("No layers found in ModelInfo.{json,txt} file");
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

    }
}
