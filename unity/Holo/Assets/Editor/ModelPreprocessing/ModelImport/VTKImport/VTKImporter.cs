using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class VTKImporter
{
    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    public Vector3[] BaseVertices { get; private set; }

    private List<Vector3> vertices = new List<Vector3>();
    public Vector3[] Vertices { get => vertices.ToArray(); }

    public void LoadFile(string file_path)
    {
        vertices.Clear();
        indices.Clear();
        using (StreamReader streamReader = new StreamReader(file_path, Encoding.ASCII))
        {
            streamReader.ReadLine(); //DataFile version
            streamReader.ReadLine(); //vtk output

            string encoding = streamReader.ReadLine();
            if (!encoding.Equals("ASCII"))
                throw new Exception("Wrong file encoding!");

            string datatype = streamReader.ReadLine();
            //if (!datatype.Equals("DATASET POLYDATA"))
              //  throw new Exception("Wrong file datatype!");

            GetVertices(streamReader);

            SkipLines(streamReader);

            GetIndices(streamReader);
        }
    }

    private void GetVertices(StreamReader streamReader)
    {
        string[] pointsData = streamReader.ReadLine().Split(' ');
        int numberOfVertices = int.Parse(pointsData[1]);
        BaseVertices = new Vector3[numberOfVertices];

        int lineCount = (numberOfVertices - 1) / 3 + 1;
        for (int i = 0; i < lineCount; i++)
        {
            vertices.AddRange(streamReader.GetLineVertices());
        }

    }

    //skipping data about lines joining vertices
    private void SkipLines(StreamReader streamReader)
    {
        string[] linesData = streamReader.ReadLine().Split(' ');
        int numberOfLines = int.Parse(linesData[1]);
        for (int i = 0; i < numberOfLines; i++)
            streamReader.ReadLine();
        streamReader.ReadLine(); //skipping an empty Line
    }

    private void GetIndices(StreamReader streamReader)
    {
        string[] indicesData = streamReader.ReadLine().Split(' ');
        int numberOfLines = int.Parse(indicesData[1]);
        for (int i = 0; i < numberOfLines; i++)
            indices.AddRange(streamReader.GetLineIndices());
    }

}