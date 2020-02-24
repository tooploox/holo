using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ModelImport.LayerImport.VTKConvertedImport
{
    public class ConvertedDataImporter : IFrameImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Vector3[] Vertices { get; protected set; }
        public Vector3[] Normals { get; protected set; } = null;
        public Vector3[] DeltaTangents { get; protected set; } = null;
        public int[] Indices { get; protected set; }
        public int VerticesInFacet { get; protected set; }
        public Dictionary<string, Vector3> BoundingVertices { get; protected set; } = new Dictionary<string, Vector3>()
        { { "minVertex", new Vector3()},
          { "maxVertex", new Vector3()}
        };
        protected string datasetType;

        private string filePath;

        public void ImportFile(string aFilePath)
        {
            filePath = aFilePath;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.ASCII))
            {
                ImportData(streamReader);
            }
        }

        private void ImportData(StreamReader streamReader)
        {
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                if (line.IndexOf("BOUNDS") >= 0)
                {
                    float[] boundingCoordinates = ImportBoundingVertices(streamReader.ReadLine());
                    BoundingVertices["minVertex"] = new Vector3(boundingCoordinates[0], boundingCoordinates[1], boundingCoordinates[2]);
                    BoundingVertices["maxVertex"] = new Vector3(boundingCoordinates[3], boundingCoordinates[4], boundingCoordinates[5]);
                }
                if (line.IndexOf("NUMBER OF FACET EDGES") >= 0)
                {
                    VerticesInFacet = int.Parse(line.Split(' ').Last());
                }
                if (line.IndexOf("VERTICES") >= 0)
                {
                    Vertices = ImportVector3Array(streamReader.ReadLine(), LoadNumberOfVectors(line), "Vertices");
                }
                if (line.IndexOf("INDICES") >= 0)
                {
                    ImportIndices(streamReader.ReadLine());
                }
                if (line.IndexOf("VECTORS") >= 0)
                {
                    Normals = ImportVector3Array(streamReader.ReadLine(), LoadNumberOfVectors(line), "Vectors");
                }
                if (line.IndexOf("SCALARS") >= 0)
                {
                    DeltaTangents = ImportVector3Array(streamReader.ReadLine(), LoadNumberOfVectors(line), "Scalars");
                }
            }
            if (Normals == null & VerticesInFacet == 3)
            {
                CalculateMeshNormals();
            }
        }

        private float[] ImportBoundingVertices(string boundsStr)
        {
            return Array.ConvertAll(boundsStr.Split(' '), s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
            
        }
        private Vector3[] ImportVector3Array(string vectorsStr, int NumberOfVectors, string arrayName)
        {
            try
            {
                Vector3[] vectorArray = new Vector3[NumberOfVectors];
                float[] coordinates = Array.ConvertAll(vectorsStr.Split(' '), s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
                int currentVector = 0;
                for (int i = 0; i < coordinates.Length; i += 3)
                {
                    vectorArray[currentVector].Set(coordinates[i], coordinates[i + 1], coordinates[i + 2]);
                    currentVector++;
                }
                return vectorArray;
            }
            catch (FormatException ex)
            {
                throw Log.ThrowError("Incorrect " + arrayName + " data in: " + filePath, ex);
            }
        }

        private void ImportIndices(string indicesStr)
        {
            try
            {
                Indices = Array.ConvertAll(indicesStr.Split(' '), int.Parse);
            }
            catch (FormatException ex)
            {
                throw Log.ThrowError("Incorrect indices data in: " + filePath, ex);
            }
        }

        private int LoadNumberOfVectors(string line)
        {
            string[] cellsData = line.Split(' ');
            return int.Parse(cellsData[1]);

        }

        private void CalculateMeshNormals()
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