using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ModelLoad.ModelImport.VTKConvertedImport
{
    public class VTKConvertedImporter : IFileImporter
    {
        public Vector3[] Vertices { get; protected set; }
        public Vector3[] Normals { get; protected set; }
        public Vector3[] DeltaTangents { get; protected set; }
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
                    Vertices = ImportVector3Array(streamReader.ReadLine(), GetNumberOfVectors(line));
                }
                if (line.IndexOf("INDICES") >= 0)
                {
                    ImportIndices(streamReader.ReadLine());
                }
                if (line.IndexOf("VECTORS") >= 0)
                {
                    Normals = ImportVector3Array(streamReader.ReadLine(), GetNumberOfVectors(line));
                }
                if (line.IndexOf("SCALARS") >= 0)
                {
                    DeltaTangents = ImportVector3Array(streamReader.ReadLine(), GetNumberOfVectors(line));
                }
            }
        }

        private float[] ImportBoundingVertices(string boundsStr)
        {
            return Array.ConvertAll(boundsStr.Split(' '), s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
            
        }
        private Vector3[] ImportVector3Array(string vectorsStr, int NumberOfVectors)
        {
            Vector3[] vectorArray = new Vector3[NumberOfVectors];
            float[] coordinates = Array.ConvertAll(vectorsStr.Split(' '), s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
            int currentVector = 0;
            for(int i = 0; i < coordinates.Length; i+=3)
            {
                vectorArray[currentVector].Set(coordinates[i], coordinates[i + 1], coordinates[i + 2]);
                currentVector++;
            }
            return vectorArray;
        }

        private void ImportIndices(string indicesStr)
        {
            Indices = Array.ConvertAll(indicesStr.Split(' '), int.Parse);
        }

        private int GetNumberOfVectors(string line)
        {
            string[] cellsData = line.Split(' ');
            return int.Parse(cellsData[1]);
        }
    }
}
