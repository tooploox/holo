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

        public VTKImporter(string datasetType)
        {
            this.datasetType = datasetType;
        }

        public void ImportFile(string filePath)
        {
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
                throw new Exception("Wrong dataset type!");
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
                Vector3 currentVertex = streamReader.GetLineVertex();
                Vertices[i] = currentVertex;
                BoundingVertices.UpdateBoundingVertices(firstVertex, currentVertex);
                firstVertex = false;
            }
        }
    }
}
