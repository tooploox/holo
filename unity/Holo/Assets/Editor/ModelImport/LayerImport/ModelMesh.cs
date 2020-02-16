using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModelImport.LayerImport
{
    public class ModelMesh
    {
        public Mesh mesh = new Mesh();
        private Dictionary<string, Vector3> boundingVertices = new Dictionary<string, Vector3>();
        bool simulationData;
        private int verticesInFacet;
        public ModelMesh(string dataType)
        {
            simulationData = CheckIfSimulation(dataType);
        }

        public Mesh Get()
        {
            return mesh;
        }

        private bool CheckIfSimulation(string dataType)
        {
            string[] simulationVariants = { "true", "fibre", "flow" };
            return simulationVariants.Contains(dataType);
        }

        public void Initiate(int numberOfVertices, int verticesNumberInFacet, int[] indices)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = new Vector3[numberOfVertices];
            verticesInFacet = verticesNumberInFacet;

            switch (verticesInFacet)
            {
                case 2:
                    LineMesh(indices);
                    break;
                case 3:
                    TriangleMesh(numberOfVertices, indices);
                    break;
                default:
                    throw new Exception("Wrong number of indices in a facet!");
            }
            if (simulationData)
            {
                SimulationMesh(numberOfVertices);
            }
        }

        private void LineMesh(int[] indices)
        {
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
        }

        private void TriangleMesh(int numberOfVertices, int[] indices)
        {
            mesh.triangles = indices;
            mesh.normals = new Vector3[numberOfVertices];
        }

        private void SimulationMesh(int numberOfVertices)
        {
            Vector4[] tangents = new Vector4[numberOfVertices];

            for (int i = 0; i < tangents.Length; i++)
            {
                tangents[i].w = 1;
            }
            mesh.tangents = tangents;
        }

        //Checks if topology stays the same between current and first file, sending a warning if it doesn't.
        public void CheckTopology(int meshIndex, int[] importerIndices)
        {
            bool equalTopology = mesh.GetIndices(0).SequenceEqual(importerIndices);
            if (!equalTopology)
            {
                Debug.LogWarning("Topology isn't the same! Mesh nr: " + meshIndex.ToString());
            }
        }

        //Configures mesh into a BlendShape animation after loading all the frames.
        public void Configure()
        { 
            mesh.bounds = CalculateBounds();
            if (verticesInFacet == 3 & !simulationData)
            {
                mesh.RecalculateNormals();
            }
        }

        public void UpdateBounds(bool firstMesh, Dictionary<string, Vector3> importerVertices)
        {
            if (firstMesh)
            {
                boundingVertices["minVertex"] = importerVertices["minVertex"];
                boundingVertices["maxVertex"] = importerVertices["maxVertex"];
            }
            else
            {
                boundingVertices["minVertex"] = Vector3.Min(boundingVertices["minVertex"], importerVertices["minVertex"]);
                boundingVertices["maxVertex"] = Vector3.Max(boundingVertices["maxVertex"], importerVertices["maxVertex"]);
            }
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
