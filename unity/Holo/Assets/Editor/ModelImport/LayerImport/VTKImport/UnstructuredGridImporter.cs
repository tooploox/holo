using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModelImport.LayerImport.VTKImport
{
    // A class for reading VTK files of UNSTRUCTURED_GRID datatype.
    class UnstructuredGridImporter : VTKImporter
    {
        public UnstructuredGridImporter(string datasetType) : base(datasetType)
        {
            this.datasetType = datasetType;
        }

        ////Imports a single body mesh file.
        protected override void ImportData(StreamReader streamReader)
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
                    verticesFlag = GetPoints(streamReader, line);
                }

                if (line.IndexOf("CELLS", StringComparison.CurrentCultureIgnoreCase) >= 0 & verticesFlag == true)
                {
                    indicesFlag = GetCells(streamReader, line);
                }
            }
        }

        private bool GetCells(StreamReader streamReader, string line)
        {
            string[] cellsData = line.Split(' ');
            int numberOfLines = int.Parse(cellsData[1]);
            GetBodyIndices(streamReader, numberOfLines);
            if (VerticesInFacet == 3)
            {
                GetBodyNormals();
            }
            return true;
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
    }
}
