using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;


public static class VTKImportUtils
{
    public static List<Vector3> GetLineVertices(this StreamReader streamReader)
    {
        List<Vector3> vertices = new List<Vector3>();

        string LineVertices = streamReader.ReadLine();
        string[] CoordinatesStringArray = LineVertices.Split(' ');

        foreach (IList<string> coordinate in CoordinatesStringArray.ChunksOf(3))
        {
            vertices.Add(GetVector3(coordinate[0], coordinate[1], coordinate[2]));
        }
        return vertices;
    }

    private static Vector3 GetVector3(string x, string y, string z)
    { 
        Vector3 vector3 = new Vector3();

        vector3.x = float.Parse(x, CultureInfo.InvariantCulture.NumberFormat);
        vector3.y = float.Parse(y, CultureInfo.InvariantCulture.NumberFormat);
        
        //maintaining Unity counter-clockwise orientation
        vector3.z = -float.Parse(z, CultureInfo.InvariantCulture.NumberFormat);
        
        return vector3;
    }

    public static IEnumerable<IList<T>> ChunksOf<T>(this IEnumerable<T> sequence, int size)
    {
        List<T> chunk = new List<T>(size);

        foreach (T element in sequence)
        {
            chunk.Add(element);
            if (chunk.Count == size)
            {
                yield return chunk;
                chunk = new List<T>(size);
            }
        }
    }

    public static List<int> GetLineIndices(this StreamReader streamReader)
    {
        List<int> indices = new List<int>();

        string LineVertices = streamReader.ReadLine();
        var matches = Regex.Matches(LineVertices, @"\d+");

        for(int i = 1; i <matches.Count; i++) //omitting number of indices in a facet
        {
            int index = int.Parse(matches[i].Value);
            indices.Add(index);   
        }
        return indices;
    }

    public static Dictionary<string, Vector3> UpdateBoundingVertices(this Dictionary<string, Vector3> currentBoundingVertices, 
                                                                     bool firstVertex, Vector3 currentVertex)
    {
        Vector3 minVertex = currentBoundingVertices["minVertex"];
        Vector3 maxVertex = currentBoundingVertices["maxVertex"];
        if (firstVertex)
        {
            currentBoundingVertices["minVertex"] = currentVertex;
            currentBoundingVertices["maxVertex"] = currentVertex;
        }
        else
        { 
            currentBoundingVertices["minVertex"] = Vector3.Min(minVertex, currentVertex);
            currentBoundingVertices["maxVertex"] = Vector3.Max(maxVertex, currentVertex);
        }
        return currentBoundingVertices;
    }
}

