using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace ModelLoad.ModelImport.VTKImport
{
    abstract class VTKImporter : IFileImporter
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

        public VTKImporter(string datasetType)
        {
            this.datasetType = datasetType;
        }

        public void ImportFile(string aFilePath)
        {
            filePath = aFilePath;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.ASCII))
            {
                streamReader.ReadLine(); //DataFile version
                streamReader.ReadLine(); //vtk output
                CheckType(streamReader);
                ImportData(streamReader);
            }
        }

        abstract protected void ImportData(StreamReader streamReader);

        protected void CheckType(StreamReader streamReader)
        {
            string encoding = streamReader.ReadLine();
            if (!encoding.Equals("ASCII"))
            {
                EditorUtility.ClearProgressBar();
                throw new Exception("Wrong file encoding!");
            }
            string currentDatasetType = streamReader.ReadLine().Split(' ')[1];
            if (!datasetType.Equals(currentDatasetType))
            {
                EditorUtility.ClearProgressBar();
                throw new Exception("Wrong dataset type. Expected " + datasetType + 
                    ", got " + currentDatasetType + ". VTK file: " + filePath);
            }
        }

        protected bool GetPoints(StreamReader streamReader, string line)
        {
            string[] pointsData = line.Split(' ');
            int numberOfVertices = int.Parse(pointsData[1]);
            GetVertices(streamReader, numberOfVertices);
            return true;
        }

        //Get meshes' vertices of the body. 
        protected void GetVertices(StreamReader streamReader, int numberOfVertices)
        {
            Vertices = new Vector3[numberOfVertices];
            bool firstVertex = true;
            for (int i = 0; i < numberOfVertices; i++)
            {
                List<Vector3> verticesList = streamReader.GetLineVertices();
                for (int j = 0; j < verticesList.Count; j++)
                {
                    Vertices[i+j] = verticesList[j];
                    BoundingVertices.UpdateBoundingVertices(firstVertex, verticesList[j]);
                    firstVertex = false;
                }
                i += verticesList.Count - 1;
            }
        }
    }
}
