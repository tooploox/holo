/*
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


//Loads a single STL file and turns it into a list of vertices (x,y,z) & if firstMesh: a list of indexes
public class STLImporter
{
    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    public Vector3[] BaseVertices { get; private set; }

    private List<Vector3> vertices = new List<Vector3>();
    public Vector3[] Vertices { get => vertices.ToArray(); }

    private List<Vector3> normals = new List<Vector3>();
    public Vector3[] Normals { get => normals.ToArray(); }



    private uint facetCount;

    public void LoadFile(string file_path)
    {
        vertices.Clear();
        indices.Clear();
        normals.Clear();
        using (FileStream filestream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader binaryReader = new BinaryReader(filestream, new ASCIIEncoding()))
            {
                // read header
                byte[] header = binaryReader.ReadBytes(80);
                facetCount = binaryReader.ReadUInt32();

                for (uint i = 0; i < facetCount; i++)
                {
                    AdaptFacet(binaryReader);
                }
            }
        }

        //Normalising summed-up facets' normals
        foreach (Vector3 normal in normals) normal.Normalize();
    }

    private void AdaptFacet(BinaryReader binaryReader)
    {
        Vector3 currentNormal = binaryReader.GetVector3();

        List<Vector3> verticesTriad = new List<Vector3>();
        for (int i = 0; i < 3; i++)
        {
            verticesTriad.Add(binaryReader.GetVector3());
        }

        // Maintaining counter-clockwise orientation 
        verticesTriad.Reverse();
        foreach (Vector3 vertex in verticesTriad)
            AddVertex(vertex, currentNormal);

        binaryReader.ReadUInt16(); // non-sense attribute byte
    }

    private void AddVertex(Vector3 currentVertex, Vector3 currentNormal)
    {
        for (int index = vertices.Count - 1; index >= 0; index--)
        {
            if (vertices[index].Equals(currentVertex))
            {
                indices.Add(index);
                normals[index] += currentNormal; //Summing up facets' normals to which vertex belongs to
                return;
            }
        }
        int newIndex = vertices.Count;
        indices.Add(newIndex);
        vertices.Add(currentVertex);
        normals.Add(currentNormal);
    }
}
*/