using System;
using UnityEngine;

public class FileImporter
{
    private STLImporter stlFileImporter;
    private string fileExtension;

    public Vector3[] BaseVertices { get; private set; }
    public Vector3[] Vertices { get; private set; }
    public Vector3[] Normals { get; private set; }
    public int[] Indices { get; private set; }

    //Getting format-specific FileImporter (only STL for now)
    public FileImporter(string extension)
    {
        fileExtension = extension;
        switch (fileExtension)
        {
            case ".stl":
                stlFileImporter = new STLImporter();
                break;
            default:
                throw new Exception("Type not supported!");
        }
    }

    public void LoadFile(string filePath, bool firstMesh)
    {
        switch (fileExtension)
        {
            case ".stl":
                LoadStlFile(filePath);
                break;
        }
        if (firstMesh)
            BaseVertices = new Vector3[Vertices.Length];
    }

    private void LoadStlFile(string filePath)
    {
        stlFileImporter.LoadFile(filePath);
        Vertices = stlFileImporter.Vertices;
        Indices = stlFileImporter.Indices;
        Normals = stlFileImporter.Normals;
    }
}
