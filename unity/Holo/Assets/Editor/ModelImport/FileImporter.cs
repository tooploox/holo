﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport.VTKImport;

namespace ModelImport
{
    //A class for loading a specific file. Currently STL and specific VTK formats are supported.
    public class FileImporter
    {
        private STLImporter stlImporter;
        private VTKImporter vtkImporter;
        private string fileExtension;

        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector3[] DeltaTangents { get; private set; }
        public int[] Indices { get; private set; }
        public int VerticesInFacet { get; private set; }
        public Dictionary<string, Vector3> BoundingVertices { get; private set; } = new Dictionary<string, Vector3>()
        { { "minVertex", new Vector3()},
          { "maxVertex", new Vector3()}
        };

        //A constructor ensuring FileImporter is format-specific (only STL and VTK for now)
        public FileImporter(string extension, bool simulationData)
        {
            fileExtension = extension;
            if (extension == ".stl")
            {
                stlImporter = new STLImporter();
            }
            else if (extension == ".vtk")
            {
                vtkImporter = new VTKImporter(simulationData);
            }
            else
            {
                EditorUtility.ClearProgressBar();
                throw new Exception("Type not supported!");
            }
        }

        //Loads a mesh from the given filepath.
        public void ImportFile(string filePath, bool firstMesh)
        {
            CheckExtension(filePath);
            switch (fileExtension)
            {
                case ".stl":
                    LoadStlFile(filePath);
                    break;
                case ".vtk":
                    LoadVtkFile(filePath);
                    break;
            }
        }

        //Checks if file extensions are consistent throughout the folder.
        private void CheckExtension(string filePath)
        {
            string currentExtension = Path.GetExtension(filePath);
            if (!fileExtension.Equals(currentExtension))
            {
                EditorUtility.ClearProgressBar();
                throw new Exception("File extensions are not consistent!");
            }
        }
        //Loads a mesh from the STL file located in the given filepath.
        private void LoadStlFile(string filePath)
        {
            stlImporter.LoadFile(filePath);
            Vertices = stlImporter.Vertices;
            Indices = stlImporter.Indices;
            Normals = stlImporter.Normals;
        }

        //Loads a mesh from the VTK file located in the given filepath.
        private void LoadVtkFile(string filePath)
        {
            vtkImporter.ImportFile(filePath);
            Vertices = vtkImporter.Vertices;
            Indices = vtkImporter.Indices;
            VerticesInFacet = vtkImporter.VerticesInFacet;
            Normals = vtkImporter.Normals;
            BoundingVertices = vtkImporter.BoundingVertices;
            DeltaTangents = vtkImporter.DeltaTangents;
        }
    }
}
