using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

class UnstructuredGridImporter
{
    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    private List<Vector3> vertices = new List<Vector3>();
    public Vector3[] Vertices { get => vertices.ToArray(); }

    public Vector3[] Normals { get; private set; }

    public UnstructuredGridImporter(StreamReader streamReader)
    {
        GetVertices(streamReader);
        GetIndicesAndNormals(streamReader);
    }


    private void GetVertices(StreamReader streamReader)
    {
        string[] pointsData = streamReader.ReadLine().Split(' ');
        int numberOfVertices = int.Parse(pointsData[1]);
        Normals = new Vector3[numberOfVertices];
        for (int i = 0; i < numberOfVertices; i++)
        {
            vertices.AddRange(streamReader.GetLineVertices());
        }

    }

    private void GetIndicesAndNormals(StreamReader streamReader)
    {
        string[] indicesData = streamReader.ReadLine().Split(' ');
        int numberOfLines = int.Parse(indicesData[1]);
        List<int> facetIndices = new List<int>();
        Vector3 currentNormal = new Vector3();

        for (int i = 0; i < numberOfLines; i++)
        {
            facetIndices = streamReader.GetLineIndices();
            indices.AddRange(facetIndices);

            currentNormal = CalculateFacetNormal(facetIndices);
            foreach (int index in facetIndices)
                Normals[index] += currentNormal;
            indices.AddRange(facetIndices);
        }
        foreach (Vector3 normal in Normals)
            normal.Normalize();
    }

    private Vector3 CalculateFacetNormal(List<int> facetIndices)
    {
        Vector3 normal = new Vector3();
        List<Vector3> facetVertices = new List<Vector3>();
        foreach (int index in facetIndices)
        {
            facetVertices.Add(vertices[index]);
        }
        normal = Vector3.Cross(facetVertices[1] - facetVertices[0], facetVertices[2] - facetVertices[0]);
        normal.Normalize();
        return normal;
    }
}
