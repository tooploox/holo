using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModelImport
{
    public class SingleModel
    {
        private FileSeriesImporter seriesImporter = new FileSeriesImporter();
        public Dictionary<string, Tuple<Mesh, GameObject>> ModelObjects { get; private set; } = new Dictionary<string, Tuple<Mesh, GameObject>>();

        public string ModelName { get; private set; } = "";
        public string ThumbnailDirectory { get; private set; } = "";

        //Loads a single model, with its body and/or dataflow.
        public void GetModelData()
        {
            string rootDirectory = GetRootDirectory();

            ModelObjects.Clear();
            ThumbnailDirectory = "";
            List<string> directoriesList = ReadInfoFile(rootDirectory);
            foreach (string directory in directoriesList)
            {
                ImportData(directory);
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
                rootDirectory = EditorUtility.OpenFolderPanel("Select STL series root folder", Application.dataPath, "");
            }
            if (String.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentException("Path cannot be null!");
            }
            return rootDirectory;
        }

        //Reads info file and gets a list of files to load into assetbundle.
        private List<string> ReadInfoFile(string rootDirectory)
        {
            List<string> directoriesList = new List<string>();
            using (StreamReader streamReader = new StreamReader(rootDirectory + @"\" + "ModelInfo.txt"))
            {
                ModelName = streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    string modelElement = streamReader.ReadLine();
                    if (modelElement == "body" || modelElement == "dataflow")
                    {
                        directoriesList.Add(rootDirectory + @"\" + modelElement);
                    }
                    else
                    {
                        ThumbnailDirectory = rootDirectory + @"\" + modelElement;
                    }
                }
                if (directoriesList.Count == 0)
                {
                    throw new Exception("No models found in info file!");
                }
            }
            return directoriesList;
        }
        //Imports data for body or dataflow blendshape.
        private void ImportData(string dataDirectory)
        {
            string dictionaryKey = ModelName + "_" + Path.GetFileName(dataDirectory);
            seriesImporter.ImportData(dataDirectory, dictionaryKey);
            Tuple<Mesh, GameObject> gameObjectData = new Tuple<Mesh, GameObject>(seriesImporter.ModelMesh, seriesImporter.ModelGameObject);
            ModelObjects.Add(dictionaryKey, gameObjectData);
        }
    }
}
