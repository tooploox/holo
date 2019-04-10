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
    private StreamReader streamReader;
    public void LoadFile(string file_path)
    {
        using (streamReader = new StreamReader(file_path, Encoding.ASCII))
        {
            streamReader.ReadLine(); //DataFile version
            streamReader.ReadLine(); //vtk output

            string encoding = streamReader.ReadLine();
            if (!encoding.Equals("ASCII"))
                throw new Exception("Wrong file encoding!");

            string[] datatype = streamReader.ReadLine().Split(' ');
            switch (datatype[1])
            {
                case "POLYDATA":
                    ImportFromPolydata();
                    break;
                case "UNSTRUCTURED_GRID":
                    streamReader.ReadLine(); // Blank line
                    ImportFromUnstructuredGrid();
                    break;
                default:
                    throw new Exception("Wrong file datatype!");
            }
        }
    }

    public void ImportFromPolydata()
    {
        PolyDataImporter polyDataImporter = new PolyDataImporter(streamReader);
        Indices = polyDataImporter.Indices;
        Vertices = polyDataImporter.Vertices;
        Normals = polyDataImporter.Normals;
    }

    public void ImportFromUnstructuredGrid()
    {
        UnstructuredGridImporter unstructuredGridImporter = new UnstructuredGridImporter(streamReader);
        Indices = unstructuredGridImporter.Indices;
        Vertices = unstructuredGridImporter.Vertices;
        Normals = unstructuredGridImporter.Normals;
    }
}