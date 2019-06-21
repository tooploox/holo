using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModelImport.VTKImport
{
    public class UnstructuredGridImporter
    {
        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector3[] DeltaTangents { get; private set; }
        public int[] Indices { get; private set; }
        public int IndicesInFacet { get; private set; }
        public Dictionary<string, Vector3> BoundingVertices { get; private set; } = new Dictionary<string, Vector3>()
        {
            { "minVertex", new Vector3()},
            { "maxVertex", new Vector3()}
        };
        private bool dataflow = false;

        //Imports a single file. Checks if it's a body mesh or dataflow object.
        public void ImportFile(StreamReader streamReader, bool IsDataflow)
        {
            dataflow = IsDataflow;
            if (IsDataflow)
            {
                ImportDataFlow(streamReader);
            }
            else
            {
                ImportModelBody(streamReader);
            }

        }
        //Imports a single dataflow mesh file.
        private void ImportDataFlow(StreamReader streamReader)
        {
            bool verticesFlag = false;
            bool vectorsFlag = false;
            bool tangentsAlpha = false;
            bool tangentsBeta = false;
            bool tangentsFlag = false;
            var line = "";
            while (!streamReader.EndOfStream)
            {
                if (verticesFlag & vectorsFlag & tangentsFlag)
                {
                    break;
                }
                line = streamReader.ReadLine();

                if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    string[] pointsData = line.Split(' ');
                    int numberOfVertices = int.Parse(pointsData[1]);
                    GetVertices(streamReader, numberOfVertices);
                    verticesFlag = true;
                }
                if (line.IndexOf("Vectors fn float", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    GetDataflowVectors(streamReader);
                    vectorsFlag = true;
                }
                if (line.IndexOf("LOOKUP_TABLE alpha", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    GetDataflowTangents(streamReader, "alpha");
                    tangentsAlpha = true;
                }
                if (line.IndexOf("LOOKUP_TABLE beta", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    GetDataflowTangents(streamReader, "beta");
                    tangentsBeta = true;
                }
                tangentsFlag = tangentsAlpha & tangentsBeta;
            }
            if (!verticesFlag | !vectorsFlag | !tangentsFlag)
            {
                throw new Exception("Insufficient data in a file!");
            }
            SetDataflowIndices();
        }

        ////Imports a single body mesh file.
        private void ImportModelBody(StreamReader streamReader)
        {
            bool verticesFlag = false;
            bool indicesFlag = false;
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (verticesFlag & indicesFlag)
                {
                    break;
                }

                if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    string[] pointsData = line.Split(' ');
                    int numberOfVertices = int.Parse(pointsData[1]);
                    GetVertices(streamReader, numberOfVertices);
                    verticesFlag = true;
                }

                if (line.IndexOf("CELLS", StringComparison.CurrentCultureIgnoreCase) >= 0 & verticesFlag == true)
                {
                    string[] cellsData = line.Split(' ');
                    int numberOfLines = int.Parse(cellsData[1]);
                    GetBodyIndicesAndNormals(streamReader, numberOfLines);
                    indicesFlag = true;
                }
            }
        }

        //Get meshes' verices. 
        private void GetVertices(StreamReader streamReader, int numberOfVertices)
        {
            Vertices = new Vector3[numberOfVertices];
            bool firstVertex = true;
            for (int i = 0; i < numberOfVertices; i++)
            {
                Vector3 currentVertex = streamReader.GetLineVertex();
                Vertices[i] = currentVertex;
                BoundingVertices.UpdateBoundingVertices(firstVertex, currentVertex);
                firstVertex = false;
            }
        }

        //Gets body indices in the file and calculates its normals.
        private void GetBodyIndicesAndNormals(StreamReader streamReader, int numberOfLines)
        {
            List<int> indices = new List<int>();
            List<int> facetIndices = new List<int>();

            bool firstVertex = true;
            for (int i = 0; i < numberOfLines; i++)
            {
                facetIndices = streamReader.GetLineIndices();
                if (firstVertex)
                {
                    IndicesInFacet = facetIndices.Count;
                }
                if (facetIndices.Count > 2)
                {
                    UpdateFacetNormals(facetIndices);
                }
                indices.AddRange(facetIndices);
            }
            if (facetIndices.Count > 2)
            {
                foreach (Vector3 normal in Normals)
                {
                    normal.Normalize();
                }
            }
            Indices = indices.ToArray();
        }

        //Updates facet normals.
        private void UpdateFacetNormals(List<int> facetIndices)
        {
            Vector3 currentNormal = new Vector3();
            currentNormal = CalculateFacetNormal(facetIndices);
            foreach (int index in facetIndices)
            {
                Normals[index] += currentNormal;
            }
        }

        //Calculates a normal of a facet.
        private Vector3 CalculateFacetNormal(List<int> facetIndices)
        {
            Vector3 normal = new Vector3();
            List<Vector3> facetVertices = new List<Vector3>();
            foreach (int index in facetIndices)
            {
                facetVertices.Add(Vertices[index]);
            }
            normal = Vector3.Cross(facetVertices[0] - facetVertices[2], facetVertices[1] - facetVertices[0]);
            normal.Normalize();
            return normal;
        }

        //Gets dataflow vectors.
        private void GetDataflowVectors(StreamReader streamReader)
        {
            Normals = new Vector3[Vertices.Length];
            for (int i = 0; i < Normals.Length; i++)
            {
                Vector3 currentVertex = streamReader.GetLineVertex();
                Normals[i] = currentVertex;
            }
        }

        //Gets angles for dataflow visualisation and stores them in deltaTangents for blendshapeanimation.
        private void GetDataflowTangents(StreamReader streamReader, string angleName)
        {
            DeltaTangents = new Vector3[Vertices.Length];
            if (angleName.Equals("alpha"))
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    DeltaTangents[i].x = streamReader.GetLineFloat();
                }
            }

            if (angleName.Equals("beta"))
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    DeltaTangents[i].y = streamReader.GetLineFloat();
                }
            }
        }

        // Sets dataflow indices. We use single vertices for dataflow so it's an array of incrementing numbers ranged from 0, Vertices.Length. 
        // Each point is a triangle so indices array is three times longer, each triangle pointing to only one vertex.
        private void SetDataflowIndices()
        {
            IndicesInFacet = 3;
            Indices = new int[Vertices.Length * 3];
            int currentVertexNumber = 0;
            for (int i = 0; i < Indices.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    Indices[i+j] = currentVertexNumber;
                }
                currentVertexNumber++;
            }
        }
    }
}
