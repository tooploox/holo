using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModelImport.VTKImport
{
    // A class for reading VTK files of UNSTRUCTURED_GRID datatype.
    public class UnstructuredGridImporter
    {
        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector3[] DeltaTangents { get; private set; }
        public int[] Indices { get; private set; }
        public int VerticesInFacet { get; private set; }
        public Dictionary<string, Vector3> BoundingVertices { get; private set; } = new Dictionary<string, Vector3>()
        {
            { "minVertex", new Vector3()},
            { "maxVertex", new Vector3()}
        };

        //Imports a single file. Checks if it's a body mesh or dataflow object.
        public void ImportFile(StreamReader streamReader, bool dataflow)
        {
            if (dataflow)
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
                if (verticesFlag && vectorsFlag && tangentsFlag)
                {
                    break;
                }
                line = streamReader.ReadLine();

                if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    string[] pointsData = line.Split(' ');
                    int numberOfVertices = int.Parse(pointsData[1]);
                    GetVertices(streamReader, numberOfVertices);
                    DeltaTangents = new Vector3[Vertices.Length];
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
                if (line.IndexOf("CELL_DATA", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    GetDataflowColors(streamReader);
                    tangentsFlag = true;
                }
                if (tangentsAlpha && tangentsBeta)
                {
                    tangentsFlag = true;
                }
            }

            if (!verticesFlag || !vectorsFlag || !tangentsFlag)
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
                    GetBodyIndices(streamReader, numberOfLines);
                    if (VerticesInFacet == 3)
                    {
                        GetBodyNormals();
                    }
                    indicesFlag = true;
                }
            }
        }

        //Get meshes' vertices of the body. 
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

        //Gets body indices in the file.
        private void GetBodyIndices(StreamReader streamReader, int numberOfLines)
        {
            List<int> indices = new List<int>();
            List<int> facetIndices = new List<int>(3);

            bool firstVertex = true;
            for (int i = 0; i < numberOfLines; i++)
            {
                facetIndices = streamReader.GetLineIndices();
                indices.AddRange(facetIndices);
                if (firstVertex)
                {
                    VerticesInFacet = facetIndices.Count;
                    firstVertex = false;
                }
            }

            Indices = indices.ToArray();
        }
        //Derives vertice's normals from their facets.
        private void GetBodyNormals()
        {
            Normals = new Vector3[Vertices.Length];
            int[] facetIndices = new int[3];
            for (int i = 0; i < Indices.Length; i += VerticesInFacet)
            {
                for (int j = 0; j < VerticesInFacet; j++)
                {
                    facetIndices[j] = Indices[i + j];
                }
                UpdateNormals(facetIndices);
            }
            foreach (Vector3 normal in Normals)
            {
                normal.Normalize();
            }
        }
        //Updates normals of the vertices belonging to the input facet.
        private void UpdateNormals(int[] facetIndices)
        {
            Vector3 currentNormal = new Vector3();
            currentNormal = CalculateFacetNormal(facetIndices);
            foreach (int index in facetIndices)
            {
                Normals[index] += currentNormal;
            }
        }

        //Calculates a normal of a facet.
        private Vector3 CalculateFacetNormal(int[] facetIndices)
        {
            Vector3[] facetVertices = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                facetVertices[i] = Vertices[facetIndices[i]];
            }
            Vector3 normal = Vector3.Cross(facetVertices[0] - facetVertices[2], facetVertices[1] - facetVertices[0]);
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
            VerticesInFacet = 3;
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

        private void GetDataflowColors(StreamReader streamReader)
        {
            streamReader.ReadLine();
            for (int i = 0; i < DeltaTangents.Length; i++)
            {
                Vector3 currentVertex = streamReader.GetLineVertex();
                DeltaTangents[i] = currentVertex;
            }
        }
    }
}
