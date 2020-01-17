using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelLoad.ModelImport.VTKImport;
using ModelLoad.ModelImport.VTKConvertedImport;

namespace ModelLoad.ModelImport
{
    public class FileSeriesImporter
    {
        public GameObject ModelGameObject { get; private set; }
        public ModelMesh ModelMesh { get; private set; }
        private string[] filePaths;
        private Dictionary<string, Vector3> boundingVertices = new Dictionary<string, Vector3>();
        private bool simulationData = false;

        public void ImportData(ModelLayerInfo layerInfo, string gameObjectName)
        {
            ModelGameObject = new GameObject(gameObjectName);
            simulationData = CheckIfSimulation(layerInfo.DataType);
            ModelMesh = new ModelMesh(simulationData);
            
            GetFilepaths(layerInfo.Directory);
            ImportFiles();
            AddMeshToGameObject();
        }

        private bool CheckIfSimulation(string simulationFlag)
        {
            string[] simulationVariants = { "true", "fibre", "flow" };
            return simulationVariants.Contains(simulationFlag);
        }

        private void GetFilepaths(string rootDirectory)
        {
            filePaths = Directory.GetFiles(rootDirectory + @"\");
            if (filePaths == null)
            {
                throw new Exception("No files found in: " + ModelGameObject.name);
            }
        }

        //Loads meshes from separate files into Mesh Object as BlendShapeFrames
        private void ImportFiles()
        {
            bool cancelImport = EditorUtility.DisplayCancelableProgressBar("Conversion in progress: " + ModelGameObject.name, "Converting file nr: " + 0.ToString(), 0);
            try
            {
                // the FileImporter constructor can already initialize progress bar,
                // so it's inside try..finally to make sure we clear progress bar in case of error.
                IFileImporter fileImporter = ChooseImporter(Path.GetExtension(filePaths[0]));

                //Configuring progress bar
                float progressChunk = (float)1 / filePaths.Length;

                bool firstMesh = true;
                for (int i = 0; i < filePaths.Length; i++)
                {
                    cancelImport = EditorUtility.DisplayCancelableProgressBar("Conversion in progress: " + ModelGameObject.name, "Converting file nr: " + i.ToString(), i * progressChunk);
                    if (Path.GetExtension(filePaths[i]).Equals(".meta"))
                    {
                        continue;
                    }
                    if (cancelImport)
                    {
                        AbortImport();
                    }
                    fileImporter.ImportFile(filePaths[i]);

                    if (firstMesh)
                    {
                        ModelMesh.Initiate(fileImporter.Vertices.Length, fileImporter.VerticesInFacet, fileImporter.Indices);
                    }
                    ModelMesh.CheckTopology(i, fileImporter.Indices);
                    ModelMesh.UpdateBounds(firstMesh, fileImporter.BoundingVertices);
                    if (simulationData)
                    {
                        ModelMesh.mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, fileImporter.Vertices, fileImporter.Normals, fileImporter.DeltaTangents);
                    }
                    else
                    {
                        ModelMesh.mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, fileImporter.Vertices, fileImporter.Normals, null);
                    }
                    firstMesh = false;
                    cancelImport = EditorUtility.DisplayCancelableProgressBar("Conversion in progress", "Converting file nr: " + (i + 1).ToString(), (i + 1) * progressChunk);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private IFileImporter ChooseImporter(string extension)
        {
            IFileImporter fileImporter;
            switch (extension)
            {
                case ".vtk":
                    if (simulationData)
                    {
                        fileImporter = new PolydataImporter("POLYDATA");
                    }
                    else
                    {
                        fileImporter = new UnstructuredGridImporter("UNSTRUCTURED_GRID");
                    }
                    break;
                case ".txt":
                    fileImporter = new VTKConvertedImporter();
                    break;
                //case ".stl"
                //    break;
                default:
                    throw new Exception("Type not supported!");
            }
            return fileImporter;
        }

        //Function for aborting the import of a model
        private void AbortImport()
        {
            UnityEngine.Object.DestroyImmediate(ModelGameObject);
            EditorUtility.ClearProgressBar();
            throw new Exception("Convertion aborted");
        }

        private void AddMeshToGameObject()
        {
            ModelMesh.Configure();
            SkinnedMeshRenderer skinnedMesh = ModelGameObject.AddComponent<SkinnedMeshRenderer>();
            skinnedMesh.sharedMesh = ModelMesh.Get();
            ModelGameObject.AddComponent<BlendShapeAnimation>();
        }
    }
}
