using System;
using System.Globalization;
using System.Linq;
using System.Text;

using Kitware.VTK;

namespace ModelConversion.LayerConversion.FrameImport.VTK
{
    abstract class VTKFrame : IFrame
    {
        public double[] BoundingBox { get; protected set; }
        public double[][] Vertices { get; protected set; }
        public int NumberOfFacetEdges { get; protected set; }
        public int[] Indices { get; protected set; }
        public double[][] Vectors { get; protected set; } = null;
        public double[][] Scalars { get; protected set; } = null;

        protected vtkDataSet vtkModel;

        public VTKFrame(string inputPath)
        {
            vtkModel = ReadVTKData(inputPath);
            ImportFrameBounds();
        }

        public void NormalizeVectors(double scalingFactor)
        {
            BoundingBox = ScaleArray(BoundingBox, scalingFactor);
            Vertices = ScaleArray(Vertices, scalingFactor);
            if (Vectors != null)
            {
                Vectors = ScaleArray(Vectors, scalingFactor);
            }
        }

        private vtkDataSet ReadVTKData(string path)
        {
            using (vtkDataSetReader reader = new vtkDataSetReader())
            {
                reader.ReadAllScalarsOn();
                reader.GetReadAllScalars();
                reader.ReadAllVectorsOn();
                reader.GetReadAllVectors();
                reader.ReadAllColorScalarsOn();
                reader.GetReadAllColorScalars();
                reader.SetFileName(path);
                reader.Update();
                return reader.GetOutput();
            }
        }

        private void ImportFrameBounds()
        {
            double[] boundingCoordinates = vtkModel.GetBounds();
            BoundingBox = new double[6] { boundingCoordinates[0], boundingCoordinates[2], -boundingCoordinates[5],
            boundingCoordinates[1], boundingCoordinates[3], -boundingCoordinates[4] };
        }

        protected void ImportVertices()
        {
            int numberOfPoints = vtkModel.GetNumberOfPoints();
            Vertices = new double[numberOfPoints][];
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vertices[i] = vtkModel.GetPoint(i);
                Vertices[i][2] = -Vertices[i][2];
            }
        }

        protected virtual void ImportIndices()
        {
            int numberOfCells = vtkModel.GetNumberOfCells();
            NumberOfFacetEdges = vtkModel.GetMaxCellSize();
            Indices = new int[NumberOfFacetEdges * numberOfCells];
            int currentIndexNumber = 0;
            for (int i = 0; i < numberOfCells; i++)
            {
                currentIndexNumber = ImportCellIndices(currentIndexNumber, vtkModel.GetCell(i).GetPointIds());
            }
        }

        protected void ComputePointIndices(int numberOfPoints)
        {
            NumberOfFacetEdges = 3;
            Indices = new int[numberOfPoints * 3];
            int currentVertexNumber = 0;
            for (int i = 0; i < Indices.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    Indices[i + j] = currentVertexNumber;
                }
                currentVertexNumber++;
            }
        }

        private int ImportCellIndices(int currentIndexNumber, vtkIdList cellIndices)
        {
            int numberOfIndices = cellIndices.GetNumberOfIds();
            for (int j = 0; j < numberOfIndices; j++)
            {
                Indices[currentIndexNumber] = cellIndices.GetId(j);
                currentIndexNumber += 1;
            }
            return currentIndexNumber;
        }

        private double[][] ScaleArray(double[][] jaggedArray, double scalingFactor)
        {
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                jaggedArray[i] = jaggedArray[i].Select(x => x * scalingFactor).ToArray();
            }
            return jaggedArray;
        }

        private double[] ScaleArray(double[] array, double scalingFactor)
        {
            return array.Select(x => x * scalingFactor).ToArray();
        }
    }
}
