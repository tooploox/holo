﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ModelImport
{
    public abstract class ModelImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ModelInfo Info { get; protected set; }
        public Dictionary<string, string> AssetsPath { get; protected set; } = new Dictionary<string, string>();
        public string RootDirectory { get; protected set; }

        //Loads a single model, with its body and/or simulationData.
        public ModelImporter(string rootDirectory)
        {
            RootDirectory = rootDirectory;
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
        }

        protected void ReadInfoFile()
        {
            try
            {
                ReadModelInfoJson();
            }
            catch (FileNotFoundException ex)
            {
                Log.Error("No ModelInfo.json found in root folder!", ex);
                throw;
            }

            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = RootDirectory + @"\" + layerInfo.Directory;
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0)
            {
                var ex = new InvalidDataException();
                Log.Error("No layers found in ModelInfo.json file", ex);
                throw ex;
                
            }
        }

        private void ReadModelInfoJson()
        {
            using (StreamReader r = new StreamReader(RootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = r.ReadToEnd();
                Info = JsonConvert.DeserializeObject<ModelInfo>(json);
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
