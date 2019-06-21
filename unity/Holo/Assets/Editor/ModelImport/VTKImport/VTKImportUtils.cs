using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;


namespace ModelImport.VTKImport
{
    public static class VTKImportUtils
    {
        public static Vector3 GetLineVertex(this StreamReader streamReader)
        {
            Vector3 vertex = new Vector3();

            string LineVertices = streamReader.ReadLine();
            string[] coordinatesStringArray = LineVertices.Split(' ');

            vertex = GetVector3(coordinatesStringArray[0], coordinatesStringArray[1], coordinatesStringArray[2]);

            return vertex;
        }


        public static List<Vector3> GetLineVertices(this StreamReader streamReader)
        {
            List<Vector3> vertices = new List<Vector3>();

            string lineVertices = streamReader.ReadLine();
            string[] coordinatesStringArray = lineVertices.Split(' ');

            foreach (IList<string> coordinate in coordinatesStringArray.ChunksOf(3))
            {
                vertices.Add(GetVector3(coordinate[0], coordinate[1], coordinate[2]));
            }
            return vertices;
        }

        public static float GetLineFloat(this StreamReader streamReader)
        {
            float currentFloat = 0.0f;

            string lineFloat = streamReader.ReadLine();
                currentFloat = float.Parse(lineFloat, CultureInfo.InvariantCulture.NumberFormat);
            return currentFloat;
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
            for (int i = 1; i < matches.Count; i++) //omitting number of indices in a facet
            {
                int index = int.Parse(matches[i].Value);
                indices.Add(index);
            }
            return indices;
        }

        public static Dictionary<string, Vector3> UpdateBoundingVertices(this Dictionary<string, Vector3> currentBoundingVertices,
                                                                         bool IsFirstVertex, Vector3 currentVertex)
        {
            Vector3 minVertex = currentBoundingVertices["minVertex"];
            Vector3 maxVertex = currentBoundingVertices["maxVertex"];
            if (IsFirstVertex)
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
}

