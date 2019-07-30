using System.Collections.Generic;
using UnityEngine;

namespace ModelLoad.ModelImport
{
    interface IFileImporter
    {
        Vector3[] Vertices { get; }
        Vector3[] Normals { get; }
        Vector3[] DeltaTangents { get; }
        int[] Indices { get; }
        int VerticesInFacet { get; }
        Dictionary<string, Vector3> BoundingVertices { get; }

        void ImportFile(string filePath);
    }
}
