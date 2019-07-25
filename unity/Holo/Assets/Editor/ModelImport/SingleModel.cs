using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace ModelImport
{
    public class SingleModel
    {
        private FileSeriesImporter seriesImporter = new FileSeriesImporter();
        public Dictionary<string, Tuple<Mesh, GameObject>> ModelObjects { get; private set; } = new Dictionary<string, Tuple<Mesh, GameObject>>();

        public ModelInfo Info { get; private set; }

        //Loads a single model, with its body and/or simulationData.
        public void GetModelData()
        {
            string rootDirectory = GetRootDirectory();

            ModelObjects.Clear();
            ReadInfoFile(rootDirectory);
            //Debug.Log("Reading model " + Info.Caption);
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                //Debug.Log("Reading layer " + layerInfo.Caption + " " + layerInfo.Directory + " " + layerInfo.Simulation.ToString());
                ImportLayer(layerInfo);
            }
        }

        // Gets root directory of the model.
        private string GetRootDirectory()
        {
            string rootDirectory = "";
            if (Application.isBatchMode)
            {
                Debug.Log("It's Batchmode!");
                string[] args = Environment.GetCommandLineArgs();
                int directoryFlagIndex = Array.FindIndex(args, a => a.Equals("-rootDirectory"));
                Debug.Log("rootDirectoryIndex:" + directoryFlagIndex.ToString());
                rootDirectory = args[directoryFlagIndex + 1];
                if (String.IsNullOrEmpty(rootDirectory)) throw new Exception("Model's root directory has not been assigned!");
            }
            else
            {
                Debug.Log("It's not Batchmode!");
                rootDirectory = EditorUtility.OpenFolderPanel("Select model root folder (with ModelInfo.json or ModelInfo.txt)", Application.dataPath, "");
            }
            if (String.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentException("Path cannot be null!");
            }
            return rootDirectory;
        }

        // Reads info file and gets a list of files to load into assetbundle.
        private void ReadInfoFile(string rootDirectory)
        {
            if (File.Exists(rootDirectory + @"\" + "ModelInfo.txt")) {
                Info = ReadInfoTxtFile(rootDirectory);
            } else
            if (File.Exists(rootDirectory + @"\" + "ModelInfo.json")) {
                Info = ReadInfoJsonFile(rootDirectory);
            } else
            {
                throw new Exception("No models found in info file!");
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0) {
                throw new Exception("No layers found in ModelInfo.{json,txt} file");
            }
        }

        private ModelInfo ReadInfoTxtFile(string rootDirectory)
        {
            ModelInfo result = new ModelInfo();
            using (StreamReader streamReader = new StreamReader(rootDirectory + @"\" + "ModelInfo.txt"))
            {
                result.Caption = streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    string modelElement = streamReader.ReadLine();
                    if (String.IsNullOrWhiteSpace(modelElement))
                    {
                        continue; // ignore empty lines
                    }

                    ModelLayerInfo layerInfo = new ModelLayerInfo();
                    result.Layers.Add(layerInfo);

                    layerInfo.Directory = rootDirectory + @"\" + modelElement;
                    layerInfo.Caption = modelElement;
                    layerInfo.Simulation = modelElement.Contains("simulation") || modelElement.Contains("dataflow");
                }                
            }
            return result;
        }

        private ModelInfo ReadInfoJsonFile(string rootDirectory)
        {
            ModelInfo result;
            using (StreamReader r = new StreamReader(rootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = r.ReadToEnd();
                result = JsonConvert.DeserializeObject<ModelInfo>(json);
            }
            foreach (ModelLayerInfo layerInfo in result.Layers)
            {
                layerInfo.Directory = rootDirectory + @"\" + layerInfo.Directory;
            }
            return result;
        }

        // Imports layer (with body or simulation blendshapes).
        private void ImportLayer(ModelLayerInfo layerInfo)
        {
            string dictionaryKey = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            seriesImporter.ImportData(layerInfo, dictionaryKey);
            Tuple<Mesh, GameObject> gameObjectData = new Tuple<Mesh, GameObject>(seriesImporter.ModelMesh, seriesImporter.ModelGameObject);
            ModelObjects.Add(dictionaryKey, gameObjectData);
        }
    }
}
