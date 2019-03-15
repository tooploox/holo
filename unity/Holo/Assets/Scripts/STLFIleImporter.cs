using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


//Loads a single STL file and turns it into a list of vertices (x,y,z) & if firstMesh: a list of indexes
public class STLFileImporter
{
    public Vector3[] BaseVertices { get; private set; }

    private List<Vector3> sTLMeshVertices = new List<Vector3>();
    public Vector3[] STLMeshVertices { get => sTLMeshVertices.ToArray(); }

    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    private uint facetCount = 1;

    public void LoadSTLFile(string file_path, bool firstMesh)
    {
        sTLMeshVertices.Clear();
        indices.Clear();
        using (FileStream filestream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader binaryReader = new BinaryReader(filestream, new ASCIIEncoding()))
            {
                // read header
                byte[] header = binaryReader.ReadBytes(80);
                facetCount = binaryReader.ReadUInt32();

                for (uint i = 0; i < facetCount; i++)
                    AdaptFacet(binaryReader, firstMesh);
            }
        }
        if (firstMesh)
            BaseVertices = new Vector3[sTLMeshVertices.Count];
    }

    private void AdaptFacet(BinaryReader binaryReader, bool firstMesh)
    {
        binaryReader.GetVector3(); // A normal we don't use

        for (int i = 0; i < 3; i++)
        {
            Vector3 vertex = binaryReader.GetVector3();
            AddVertex(vertex);
        }
        binaryReader.ReadUInt16(); // non-sense attribute byte
    }

    private void AddVertex(Vector3 vertex)
    {
        AddIndex(vertex);
        sTLMeshVertices.Add(vertex);
    }

    private void AddIndex(Vector3 currentVertex)
    {
        int index = 0;
        foreach (Vector3 listedVertex in sTLMeshVertices)
        {
            if (listedVertex.Equals(currentVertex))
            {
                indices.Add(index);
                return;
            }
            index++;
        }
        indices.Add(index);
    }
}
