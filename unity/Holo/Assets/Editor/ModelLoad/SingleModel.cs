﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace ModelLoad
{
    public abstract class SingleModel
    {
        public ModelInfo Info { get; protected set; }
        public Dictionary<string, string> AssetsPath { get; protected set; } = new Dictionary<string, string>();
        public string RootDirectory { get; protected set; }

        //Loads a single model, with its body and/or simulationData.
        public SingleModel()
        {
            string RootDirectory = GetRootDirectory();
        }

        public void GetModelData()
        {
            AssetsPath.Clear();
            ReadInfoFile();
            //Debug.Log("Reading model " + Info.Caption);
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                //Debug.Log("Reading layer " + layerInfo.Caption + " " + layerInfo.Directory + " " + layerInfo.Simulation.ToString());
                ImportLayer(layerInfo);
            }
        }

        protected string GetRootDirectory()
        {
            if (Application.isBatchMode)
            {
                RootDirectory = GetBatchModeRootDir();
            }
            else
            {
                RootDirectory = EditorUtility.OpenFolderPanel("Select model root folder with ModelInfo.json", Application.dataPath, "");
            }
            if (string.IsNullOrEmpty(RootDirectory))
            {
                throw new ArgumentException("Path cannot be null!");
            }
            return RootDirectory;
        }

        protected string GetBatchModeRootDir()
        {
            string[] args = Environment.GetCommandLineArgs();
            int directoryFlagIndex = Array.FindIndex(args, a => a.Equals("-rootDirectory"));
            //Debug.Log("rootDirectoryIndex:" + directoryFlagIndex.ToString());
            string rootDirectory = args[directoryFlagIndex + 1];
            if (String.IsNullOrEmpty(rootDirectory))
            {
                throw new Exception("Model's root directory has not been assigned!");
            }
            return rootDirectory;
        }

        protected void ReadInfoFile()
        {
            if (!File.Exists(RootDirectory + @"\" + "ModelInfo.json"))
            {
                throw new Exception("No ModelInfo.json found in root folder!");
            }

            using (StreamReader r = new StreamReader(RootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = r.ReadToEnd();
                Info = JsonConvert.DeserializeObject<ModelInfo>(json);
            }
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = RootDirectory + @"\" + layerInfo.Directory;
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
            layer.Simulation = CheckIfSimulation(layerInfo.DataType);
        }

        private bool CheckIfSimulation(string simulationFlag)
        {
            string[] simulationVariants = { "true", "fibre", "flow"};
            return simulationVariants.Contains(simulationFlag);
        }
    }
}
