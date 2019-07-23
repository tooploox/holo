using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ModelImport
{    
    //A class for importing a file series from a directory.
    public class FileSeriesImporter
    {
        public GameObject ModelGameObject { get; private set; }
        public Mesh ModelMesh { get; private set; }

        public GameObject[] gameObjectArray;
        public Mesh[] meshArray;
        private string[] filePaths;
        private string extension;
        private FileImporter fileImporter;
        private Dictionary<string, Vector3> boundingVertices = new Dictionary<string, Vector3>();
        private bool simulationData = false;
        
        //Imports data from file series.
        public void ImportData(ModelLayerInfo layerInfo, string gameObjectName)
        {
            ModelGameObject = new GameObject(gameObjectName);
            ModelMesh = new Mesh();
            simulationData = layerInfo.Simulation;
            bool cancelImport = EditorUtility.DisplayCancelableProgressBar("Conversion in progress: " + ModelGameObject.name, "Converting file nr: " + 0.ToString(), 0);
            //TODO: Progress bar
            GetFilepaths(layerInfo.Directory);
            ImportFiles();
            ConfigureMesh();
        }

        //Gets filepaths of particular frames and their extension
        private void GetFilepaths(string rootDirectory)
        {
            filePaths = Directory.GetFiles(rootDirectory + @"\");
            if (filePaths == null)
            {
                throw new Exception("No files found in: " + ModelGameObject.name);
            }
            extension = Path.GetExtension(filePaths[0]);

        }

        //Loads meshes from separate files into Mesh Object as BlendShapeFrames
        private void ImportFiles()
        {
			try {
			    // the FileImporter constructor can already initialize progress bar,
				// so it's inside try..finally to make sure we clear progress bar in case of error.
	            fileImporter = new FileImporter(extension, simulationData);

    	        //Configuring progress bar
        	    float progressChunk = (float) 1 / filePaths.Length;           
			
				bool firstMesh = true;
				for(int i = 0; i < filePaths.Length; i++)
				{
					bool cancelImport = EditorUtility.DisplayCancelableProgressBar("Conversion in progress: " + ModelGameObject.name, "Converting file nr: " + i.ToString(), i * progressChunk);
					if (cancelImport)
					{
						AbortImport();
					}
					fileImporter.ImportFile(filePaths[i], firstMesh);

					if (firstMesh)
					{
						InitiateMesh();
					}
					CheckTopology(i);
					UpdateBounds(firstMesh);
					if (simulationData)
					{
						ModelMesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, fileImporter.Vertices, fileImporter.Normals, fileImporter.DeltaTangents);
					}
					else
					{
						ModelMesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, fileImporter.Vertices, fileImporter.Normals, null);
					}
					firstMesh = false;
					cancelImport = EditorUtility.DisplayCancelableProgressBar("Conversion in progress", "Converting file nr: " + (i + 1).ToString(), (i + 1) * progressChunk);
				}
			} finally {
            	EditorUtility.ClearProgressBar();
			}
        }

        //Function for aborting the import of a model
        private void AbortImport()
        {
            UnityEngine.Object.DestroyImmediate(ModelGameObject);
            EditorUtility.ClearProgressBar();
            throw new Exception("Convertion aborted");
        }

        // Mesh initiation.
        private void InitiateMesh()
        {
            ModelMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            if (fileImporter.VerticesInFacet == 2)
            {
                ModelMesh.vertices = new Vector3[fileImporter.Vertices.Length];
                ModelMesh.SetIndices(fileImporter.Indices, MeshTopology.Lines, 0);
            }
            else if (fileImporter.VerticesInFacet == 3)
            {
                ModelMesh.vertices = new Vector3[fileImporter.Vertices.Length];
                ModelMesh.triangles = fileImporter.Indices;
                ModelMesh.normals = new Vector3[fileImporter.Vertices.Length];
            }
            else
            {
                throw new Exception("Wrong number of indices in a facet!");
            }
            if (simulationData)
            {
                Vector4[] tangents = new Vector4[fileImporter.Vertices.Length];
                
                for (int i = 0; i < tangents.Length; i++)
                {
                    tangents[i].w = 1;
                }
                ModelMesh.tangents = tangents;
                ModelMesh.uv = new Vector2[fileImporter.Vertices.Length];
            }
        }

        //Checks if topology stays the same between current and first file, sending a warning if it doesn't.
        private void CheckTopology(int meshIndex)
        {
            bool equalTopology = ModelMesh.GetIndices(0).SequenceEqual(fileImporter.Indices);  
            if (!equalTopology)
            {
                Debug.LogWarning("Topology isn't the same! Mesh nr: " + meshIndex.ToString());
            }
        }

        //After each frame updates Boundingbox borders of the object.
        private void UpdateBounds(bool firstMesh)
        {
            if (firstMesh)
            {
                boundingVertices["minVertex"] = fileImporter.BoundingVertices["minVertex"];
                boundingVertices["maxVertex"] = fileImporter.BoundingVertices["maxVertex"];
            }
            else
            {
                boundingVertices["minVertex"] = Vector3.Min(boundingVertices["minVertex"], fileImporter.BoundingVertices["minVertex"]);
                boundingVertices["maxVertex"] = Vector3.Max(boundingVertices["maxVertex"], fileImporter.BoundingVertices["maxVertex"]);
            }
        }

        //Configures mesh into a BlendShape animation after loading all the frames.
        private void ConfigureMesh()
        {
            SkinnedMeshRenderer skinnedMesh = ModelGameObject.AddComponent<SkinnedMeshRenderer>();
            skinnedMesh.sharedMesh = ModelMesh;
            if (fileImporter.VerticesInFacet == 3 & !simulationData)
            {
                skinnedMesh.sharedMesh.RecalculateNormals();
            }
            skinnedMesh.sharedMesh.bounds = CalculateBounds();
            ModelGameObject.AddComponent<BlendShapeAnimation>();
        }

        //Calculates Bounds for the GameObject after final extremities of the mesh series is known.
        private Bounds CalculateBounds()
        {
            Bounds meshBounds = new Bounds();
            Vector3 minVertex = boundingVertices["minVertex"];
            Vector3 maxVertex = boundingVertices["maxVertex"];
            meshBounds.center = (maxVertex + minVertex) / 2.0F;
            Vector3 extents = (maxVertex - minVertex) / 2.0F;
            for (int i = 0; i < 3; i++)
            {
                extents[i] = Math.Abs(extents[i]);
            }
            meshBounds.extents = extents;
            return meshBounds;
        }
    }
}
