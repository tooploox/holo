using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class UnstructuredGridImporter
{
    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    private List<Vector3> vertices = new List<Vector3>();
    public Vector3[] Vertices { get; private set; }
    public Vector3[] Normals { get; private set; }

    public Dictionary<string, Vector3> BoundingVertices { get; private set; } = new Dictionary<string, Vector3>()
    {
        { "minVertex", new Vector3()},
        { "maxVertex", new Vector3()}
    };

    public void LoadFile(StreamReader streamReader)
    {
        vertices.Clear();
        indices.Clear();

        bool verticesFlag = false;
        bool normalsFlag = false;
        while (!streamReader.EndOfStream)
        {
            if (verticesFlag & normalsFlag) break;
            var line = streamReader.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                string[] pointsData = line.Split(' ');
                int numberOfVertices = int.Parse(pointsData[1]);
                GetVertices(streamReader, numberOfVertices);
                verticesFlag = true;
            }

            if (line.IndexOf("CELLS", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                string[] cellsData = line.Split(' ');
                int numberOfLines = int.Parse(cellsData[1]);
                GetIndicesAndNormals(streamReader, numberOfLines);
                normalsFlag = true;
            }
        }
    }

    private void GetVertices(StreamReader streamReader, int numberOfVertices)
    {
        List<Vector3> vertices = new List<Vector3>();
        Normals = new Vector3[numberOfVertices];

        for (int i = 0; i < numberOfVertices; i++)
            vertices.AddRange(streamReader.GetLineVertices());
        Vertices = vertices.ToArray();
    }

    private void GetIndicesAndNormals(StreamReader streamReader, int numberOfLines)
    {
        //TODO: Change indices list to array
        List<int> facetIndices = new List<int>();

        bool firstVertex = true;

        for (int i = 0; i < numberOfLines; i++)
        {
            facetIndices = streamReader.GetLineIndices();
            foreach (int index in facetIndices)
            {
                BoundingVertices.UpdateBoundingVertices(firstVertex, Vertices[index]);
                firstVertex = false;
            }
            if(facetIndices.Count > 2)
                UpdateFacetNormals(facetIndices);
            indices.AddRange(facetIndices);
        }
        if (facetIndices.Count > 2)
            foreach (Vector3 normal in Normals)
                normal.Normalize();
    }

    //Updates facet normals
    private void UpdateFacetNormals(List<int> facetIndices)
    {
        Vector3 currentNormal = new Vector3();
        currentNormal = CalculateFacetNormal(facetIndices);
        foreach (int index in facetIndices)
            Normals[index] += currentNormal;
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
