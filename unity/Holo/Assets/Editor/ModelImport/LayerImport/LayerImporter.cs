using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport.LayerImport.VTKImport;
using ModelImport.LayerImport.VTKConvertedImport;

namespace ModelImport.LayerImport
{
    public class LayerImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            ImportFrames();
            AddMeshToGameObject();
        }

        //To be removed.
        private bool CheckIfSimulation(string simulationFlag)
        {
            string[] simulationVariants = { "true", "fibre", "flow" };
            return simulationVariants.Contains(simulationFlag);
        }

        private void GetFilepaths(string layerDirectory)
        {
            try
            {
                filePaths = Directory.GetFiles(layerDirectory + @"\");
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.ThrowError("Directory does not exist at: " + layerDirectory, ex);
            }
            if (filePaths == null)
            {
                Log.ThrowError("No files found in: " + layerDirectory, new FileNotFoundException());
            }
        }

        //Loads meshes from separate files into Mesh Object as BlendShapeFrames
        private void ImportFrames()
        {
            bool cancelImport = EditorUtility.DisplayCancelableProgressBar("Import in progress: " + ModelGameObject.name, "Importing file nr: " + 0.ToString(), 0);
            try
            {
                // the FileImporter constructor can already initialize progress bar,
                // so it's inside try..finally to make sure we clear progress bar in case of error.
                IFrameImporter frameImporter = InitializeImporter(Path.GetExtension(filePaths[0]));

                //Configuring progress bar
                float progressChunk = (float)1 / filePaths.Length;

                bool firstMesh = true;
                for (int i = 0; i < filePaths.Length; i++)
                {
                    string logMessage ="Importing file nr: " + i.ToString();
                    Log.Debug(ModelGameObject.name + "- Import in progress. " + logMessage);
                    cancelImport = EditorUtility.DisplayCancelableProgressBar(ModelGameObject.name + " - Import in progress. ", logMessage, i * progressChunk);
                    if (Path.GetExtension(filePaths[i]).Equals(".meta"))
                    {
                        continue;
                    }
                    if (cancelImport)
                    {
                        AbortImport();
                    }
                    frameImporter.ImportFile(filePaths[i]);

                    if (firstMesh)
                    {
                        ModelMesh.Initiate(frameImporter.Vertices.Length, frameImporter.VerticesInFacet, frameImporter.Indices);
                    }
                    ModelMesh.CheckTopology(i, frameImporter.Indices);
                    ModelMesh.UpdateBounds(firstMesh, frameImporter.BoundingVertices);
                    if (simulationData)
                    {
                        ModelMesh.mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, frameImporter.Vertices, frameImporter.Normals, frameImporter.DeltaTangents);
                    }
                    else
                    {
                        ModelMesh.mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, frameImporter.Vertices, frameImporter.Normals, null);
                    }
                    firstMesh = false;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private IFrameImporter InitializeImporter(string extension)
        {
            IFrameImporter frameImporter;
            switch (extension)
            {
                case ".vtk":
                    if (simulationData)
                    {
                        frameImporter = new PolydataImporter("POLYDATA");
                    }
                    else
                    {
                        frameImporter = new UnstructuredGridImporter("UNSTRUCTURED_GRID");
                    }
                    break;
                case ".txt":
                    frameImporter = new ConvertedDataImporter();
                    break;
                case ".stl":
                default:
                    var ex = new IOException();
                    Log.Error("Type not supported!", ex);
                    throw ex;
            }
            return frameImporter;
        }

        //Function for aborting the import of a model
        private void AbortImport()
        {
            UnityEngine.Object.DestroyImmediate(ModelGameObject);
            EditorUtility.ClearProgressBar();
            Log.ThrowError("Import aborted by the user!", new OperationCanceledException());
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
