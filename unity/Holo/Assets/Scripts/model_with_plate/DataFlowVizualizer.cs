using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class DataFlowVizualizer : MonoBehaviour
{

    private List<Vector3> points = new List<Vector3>();
    private List<Vector3> vectors = new List<Vector3>();
    public string fileVTK;
    public GameObject line;
    public float scale = 1;
    public float lengthScale = 3;
    private int readingStep = 0;

    void Start()
    {
        if(fileVTK != null)
            ReadVTKData();

        for (int i=0; i<points.Count; i++)
        {
            GameObject o = Instantiate(line, transform, false);
            LineRenderer r = o.GetComponent<LineRenderer>();

            r.SetPosition(0, points[i] * scale);
            r.SetPosition(1, (points[i] * scale) + (vectors[i] * scale * lengthScale));
            r.SetWidth(.3f * scale, .3f * scale);
        }
    }
    
    void ReadVTKData()
    {
        StreamReader f = new StreamReader(fileVTK);
        while (!f.EndOfStream)
        {
            string l = f.ReadLine();

            /* reading steps:
             * 0 on first lines with string data
             * 1 when reading points
             * 2 in lines between points and vectors
             * 3 reading vectors
            */

            string[] split = l.Trim().Split(" ".ToCharArray());
            
            double x, y, z;
            if (split.Length == 3
                && double.TryParse(split[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out x)
                && double.TryParse(split[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out y)
                && double.TryParse(split[2], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out z))
            {
                if (readingStep == 0) readingStep = 1;
                if (readingStep == 1) points.Add(new Vector3((float)x, (float)y, (float)z));
                if (readingStep == 2) readingStep = 3;
                if (readingStep == 3) vectors.Add(new Vector3((float)x, (float)y, (float)z));
            }
            else
            {
                if (readingStep == 1) readingStep = 2;
            }
        }
        f.Close();

        if (readingStep != 3)
            Debug.LogError("DataFlow read error: unsupported data structure in the vtk file");
        else if (points.Count != vectors.Count)
            Debug.LogError("DataFlow read error: points number not equal to vectors number");
        else
            Debug.Log(" " + points.Count + " points in Data Flow");
    }
    
}
