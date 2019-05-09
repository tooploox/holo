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
    { { "minVertex", new Vector3()},
      { "maxVertex", new Vector3()}
    };

    public void LoadFile(StreamReader streamReader)
    {
        streamReader.ReadLine(); // Blank line
        vertices.Clear();
        indices.Clear();
        GetVertices(streamReader);
        GetIndicesAndNormals(streamReader);

    }

    private void GetVertices(StreamReader streamReader)
    {
        List<Vector3> vertices = new List<Vector3>();
        string[] pointsData = streamReader.ReadLine().Split(' ');

        int numberOfVertices = int.Parse(pointsData[1]);
        Normals = new Vector3[numberOfVertices];

        for (int i = 0; i < numberOfVertices; i++)
            vertices.AddRange(streamReader.GetLineVertices());
        Vertices = vertices.ToArray();
    }

    private void GetIndicesAndNormals(StreamReader streamReader)
    {
        //TODO: Change indices list to array
        string[] indicesData = streamReader.ReadLine().Split(' ');
        int numberOfLines = int.Parse(indicesData[1]);
        List<int> facetIndices = new List<int>();
        Vector3 currentNormal = new Vector3();
        bool firstVertex = true;

        for (int i = 0; i < numberOfLines; i++)
        {
            facetIndices = streamReader.GetLineIndices();
            currentNormal = CalculateFacetNormal(facetIndices);

            foreach (int index in facetIndices)
            {
                Normals[index] += currentNormal;
                BoundingVertices.UpdateBoundingVertices(firstVertex, Vertices[index]);
                firstVertex = false;
            }
            indices.AddRange(facetIndices);
        }
        foreach (Vector3 normal in Normals)
            normal.Normalize();
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
