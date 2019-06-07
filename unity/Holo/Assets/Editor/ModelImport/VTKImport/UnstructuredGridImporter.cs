using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class UnstructuredGridImporter
{
    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    public Vector3[] Vertices { get; private set; }
    public Vector3[] Normals { get; private set; }
    public int IndicesInFacet { get; private set; }
    private bool dataflow = false;
    public Dictionary<string, Vector3> BoundingVertices { get; private set; } = new Dictionary<string, Vector3>()
    {
        { "minVertex", new Vector3()},
        { "maxVertex", new Vector3()}
    };

    public void ImportFile(StreamReader streamReader, bool IsDataflow)
    {
        vertices.Clear();
        indices.Clear();
        dataflow = IsDataflow;
        if (dataflow)
        {
            ImportDataFlow(streamReader);
        }
        else
        {
            ImportModelBody(streamReader);
        }

    }

    private void ImportDataFlow(StreamReader streamReader)
    {
        bool verticesFlag = false;
        bool vectorsFlag = false;
        var line = "";
        while (!(line.IndexOf("CELL_DATA", StringComparison.CurrentCultureIgnoreCase) >= 0) || !streamReader.EndOfStream)
        {
            line = streamReader.ReadLine();
            if (verticesFlag & vectorsFlag)
            {
                break;
            }

            if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                string[] pointsData = line.Split(' ');
                int numberOfVertices = int.Parse(pointsData[1]);
                GetVertices(streamReader, numberOfVertices);
                verticesFlag = true;
            }

        }
    }
    private void ImportModelBody(StreamReader streamReader)
    {
        bool verticesFlag = false;
        bool indicesFlag = false;
        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();
            if (verticesFlag & indicesFlag)
            {
                break;
            }
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                string[] pointsData = line.Split(' ');
                int numberOfVertices = int.Parse(pointsData[1]);
                GetVertices(streamReader, numberOfVertices);
                verticesFlag = true;
            }

            if (line.IndexOf("CELLS", StringComparison.CurrentCultureIgnoreCase) >= 0 & verticesFlag == true)
            {
                string[] cellsData = line.Split(' ');
                int numberOfLines = int.Parse(cellsData[1]);
                GetIndicesAndNormals(streamReader, numberOfLines);
                indicesFlag = true;
            }
        }
    }

    private void GetVertices(StreamReader streamReader, int numberOfVertices)
    {
        if (dataflow)
        {
            numberOfVertices *= 2;
        }
        Vertices = new Vector3[numberOfVertices];
        Normals = new Vector3[numberOfVertices];
        bool firstVertex = true;
        for (int i = 0; i < numberOfVertices; i++)
        {
            Vector3 currentVertex = streamReader.GetLineVertex();
            Vertices[i] = currentVertex;
            BoundingVertices.UpdateBoundingVertices(firstVertex, currentVertex);
            firstVertex = false;
            if(dataflow)
            {
                i++;
                Vertices[i] = currentVertex;
                i++;
            }
        }
    }

    private void GetDataflowVectors(StreamReader streamReader)
    {
        bool firstVertex = false;
        for (int i = 1; i < Vertices.Length - 1; i += 2)
        {
            Vector3 currentVertex = streamReader.GetLineVertex();
            Vertices[i] += currentVertex;
            BoundingVertices.UpdateBoundingVertices(firstVertex, currentVertex);
            Normals[i - 1] = currentVertex;
            Normals[i] = currentVertex;
            
        }
    }

    private void GetIndicesAndNormals(StreamReader streamReader, int numberOfLines)
    {
        List<int> facetIndices = new List<int>();

        bool firstVertex = true;

        for (int i = 0; i < numberOfLines; i++)
        {
            facetIndices = streamReader.GetLineIndices();
            if (firstVertex)
            {
                IndicesInFacet = facetIndices.Count;
            }
            if (facetIndices.Count > 2)
            {
                UpdateFacetNormals(facetIndices);
            }
            indices.AddRange(facetIndices);
        }
        if (facetIndices.Count > 2)
        {
            foreach (Vector3 normal in Normals)
            {
                normal.Normalize();
            }
        }
    }
    //Updates facet normals
    private void UpdateFacetNormals(List<int> facetIndices)
    {
        Vector3 currentNormal = new Vector3();
        currentNormal = CalculateFacetNormal(facetIndices);
        foreach (int index in facetIndices)
        { 
            Normals[index] += currentNormal;
        }
    }

    //Calculates a normal of a facet.
    private Vector3 CalculateFacetNormal(List<int> facetIndices)
    {
        Vector3 normal = new Vector3();
        List<Vector3> facetVertices = new List<Vector3>();
        foreach (int index in facetIndices)
        {
            facetVertices.Add(Vertices[index]);
        }
        normal = Vector3.Cross(facetVertices[0] - facetVertices[2], facetVertices[1] - facetVertices[0]);
        normal.Normalize();
        return normal;
    }
}
