using System;
using System.Linq;
using Kitware.VTK;

namespace VTKConverter
{
    class ModelData
    {
        public float[] BoundingBox { get; private set; }
        public float[] Vertices {get; private set;}
        public int[] Indices { get; private set; }
        public float[] Vectors { get; private set; }
        public float[] Scalars { get; private set; }

        public ModelData(vtkUnstructuredGrid vtkModel, bool simulationFlag)
        {
            //TODO: Implement reading vertices cells if simulation - vectors and colours
        }

        private vtkPolyData Triangulate(vtkPolyData input)
        {
            vtkTriangleFilter triangleFilter = new vtkTriangleFilter();
            triangleFilter.SetInput(input);

            vtkPolyData output = triangleFilter.GetOutput();

            return output;
        }
    }
}
