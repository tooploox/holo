using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class VTKImporter
{
    public int[] Indices { get; private set; }
    public Vector3[] Vertices { get; private set; }
    public Vector3[] Normals { get; private set; }
    public int IndicesInFacet { get; private set; }
    public Dictionary<string, Vector3> BoundingVertices { get; private set; } = new Dictionary<string, Vector3>()
    {
        { "minVertex", new Vector3()},
        { "maxVertex", new Vector3()}
    };

    private StreamReader streamReader;
    private UnstructuredGridImporter unstructuredGridImporter = new UnstructuredGridImporter();
    private PolyDataImporter polyDataImporter = new PolyDataImporter();

    //Loads a mesh from the VTK file located in the given filepath.
    public void LoadFile(string filePath)
    {
        using (streamReader = new StreamReader(filePath, Encoding.ASCII))
        {
            streamReader.ReadLine(); //DataFile version
            streamReader.ReadLine(); //vtk output

            string encoding = streamReader.ReadLine();
            if (!encoding.Equals("ASCII"))
            {
                throw new Exception("Wrong file encoding!");
            }

            string[] datatype = streamReader.ReadLine().Split(' ');
            switch (datatype[1])
            {
                case "POLYDATA":
                    polyDataImporter.LoadFile(streamReader);
                    Indices = polyDataImporter.Indices;
                    Vertices = polyDataImporter.Vertices;
                    Normals = polyDataImporter.Normals;
                    break;
                case "UNSTRUCTURED_GRID":
                    unstructuredGridImporter.LoadFile(streamReader);
                    Indices = unstructuredGridImporter.Indices;
                    Vertices = unstructuredGridImporter.Vertices;
                    Normals = unstructuredGridImporter.Normals;
                    IndicesInFacet = unstructuredGridImporter.IndicesInFacet;
                    BoundingVertices = unstructuredGridImporter.BoundingVertices;

                    break;
                default:
                    throw new Exception("Wrong file datatype!");
            }
        }
    }
}