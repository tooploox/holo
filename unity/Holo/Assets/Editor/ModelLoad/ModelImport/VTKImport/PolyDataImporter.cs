using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModelLoad.ModelImport.VTKImport
{
    class PolydataImporter : VTKImporter
    {
        public PolydataImporter(string datasetType) : base(datasetType)
        {
            this.datasetType = datasetType;
        }

        //Imports a single simulation data mesh file.
        protected override void ImportData(StreamReader streamReader)
        {
            bool verticesFlag = false;
            bool vectorsFlag = false;
            bool tangentsAlpha = false;
            bool tangentsBeta = false;
            bool tangentsFlag = false;
            var line = "";
            while (!streamReader.EndOfStream)
            {
                if (verticesFlag && vectorsFlag && tangentsFlag)
                {
                    break;
                }
                line = streamReader.ReadLine();

                if (line.IndexOf("POINTS", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    verticesFlag = GetPoints(streamReader, line);
                }
                if (line.IndexOf("Vectors fn float", StringComparison.CurrentCultureIgnoreCase) >= 0)
                { 
                    vectorsFlag = GetSimulationVectors(streamReader);
                }
                if (line.IndexOf("LOOKUP_TABLE alpha", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    tangentsAlpha = GetSimulationTangents(streamReader, "alpha");
                }
                if (line.IndexOf("LOOKUP_TABLE beta", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    tangentsBeta = GetSimulationTangents(streamReader, "beta");
                }
                if (line.IndexOf("CELL_DATA", StringComparison.CurrentCultureIgnoreCase) >= 0)
                { 
                    tangentsFlag = GetSimulationColors(streamReader);
                }
                if (tangentsAlpha && tangentsBeta)
                {
                    tangentsFlag = true;
                }
            }

            if (!verticesFlag || !vectorsFlag || !tangentsFlag)
            {
                throw new Exception("Insufficient data in a file!");
            }
            SetSimulationIndices();
        }

        //Gets simulation vectors.
        private bool GetSimulationVectors(StreamReader streamReader)
        {
            Normals = new Vector3[Vertices.Length];
            for (int i = 0; i < Normals.Length; i++)
            {
                Vector3 currentVertex = streamReader.GetLineVertex();
                Normals[i] = currentVertex;
            }
            return true;
        }

        //Gets angles for simulation visualisation and stores them in deltaTangents for blendshapeanimation.
        private bool GetSimulationTangents(StreamReader streamReader, string angleName)
        {
            if (DeltaTangents == null)
            {
                DeltaTangents = new Vector3[Vertices.Length];
            }
            if (angleName.Equals("alpha"))
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    DeltaTangents[i].x = streamReader.GetLineFloat();
                }
            }

            if (angleName.Equals("beta"))
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    DeltaTangents[i].y = streamReader.GetLineFloat();
                }
            }
            return true;
        }

        // Sets simulation indices. We use single vertices for simulation so it's an array of incrementing numbers ranged from 0, Vertices.Length. 
        // Each point is a triangle so indices array is three times longer, each triangle pointing to only one vertex.
        private bool SetSimulationIndices()
        {
            VerticesInFacet = 3;
            Indices = new int[Vertices.Length * 3];
            int currentVertexNumber = 0;
            for (int i = 0; i < Indices.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    Indices[i + j] = currentVertexNumber;
                }
                currentVertexNumber++;
            }
            return true;
        }

        private bool GetSimulationColors(StreamReader streamReader)
        {
            DeltaTangents = new Vector3[Vertices.Length];
            streamReader.ReadLine();
            for (int i = 0; i < DeltaTangents.Length; i+=2)
            {
                Vector3 currentVertex = streamReader.GetLineVertex();
                DeltaTangents[i] = currentVertex;
                DeltaTangents[i + 1] = currentVertex;
            }
            return true;
        }
    }
}

