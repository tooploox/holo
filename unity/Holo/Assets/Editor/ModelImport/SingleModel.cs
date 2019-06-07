using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

class SingleModel
{
    private FileSeriesImporter seriesImporter = new FileSeriesImporter();
    public Dictionary<string, Tuple<Mesh, GameObject>> ModelObjects { get; private set; } = new Dictionary<string, Tuple<Mesh, GameObject>>();

    public string ModelName { get; private set; } = "";
    public string ThumbnailDirectory { get; private set; } = "";
    

    public void GetModelData(string rootDirectory)
    {
        ModelObjects.Clear();
        ThumbnailDirectory = "";
        List<string> directoriesList = ReadInfoFile(rootDirectory);
        foreach (string directory in directoriesList)
        {
            ImportData(directory);
        }
    }

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

    private void ImportData(string dataDirectory)
    {
        string dictionaryKey = ModelName + "_" + Path.GetFileName(dataDirectory);
        seriesImporter.ImportData(dataDirectory, dictionaryKey);
        Tuple<Mesh, GameObject> gameObjectData = new Tuple<Mesh, GameObject>(seriesImporter.ModelMesh, seriesImporter.ModelGameObject);
        ModelObjects.Add(dictionaryKey, gameObjectData);
    }
}
