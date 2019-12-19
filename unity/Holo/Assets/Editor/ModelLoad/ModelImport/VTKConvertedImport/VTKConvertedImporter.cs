using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


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
            /***
            TODO: Import 
            1. Boundaries 
            2.Vertices 
            3. Indices
            If simulation:
            4. Vectors
            5. Colours/Scalars
            ***/
        }
    }
}
