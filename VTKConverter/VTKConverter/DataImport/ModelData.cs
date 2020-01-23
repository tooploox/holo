using System;
using System.Globalization;
using System.Linq;
using System.Text;

using Kitware.VTK;

namespace VTKConverter.DataImport
{
    abstract class ModelData
    {
        public double[] BoundingBox { get; protected set; }
        public double[][] Vertices { get; protected set; }
        public int NumberOfFacetEdges { get; protected set; }
        public int[] Indices { get; protected set; }
        public double[][] Vectors { get; protected set; } = null;
        public double[][] Scalars { get; protected set; } = null;

        public ModelData(vtkDataSet vtkModel)
        {
            LoadBounds(vtkModel);
        }

        private void LoadBounds(vtkDataSet vtkModel)
        {
            double[] boundingCoordinates = vtkModel.GetBounds();
            BoundingBox = new double[6] {boundingCoordinates[0], boundingCoordinates[2], -boundingCoordinates[4],
            boundingCoordinates[1], boundingCoordinates[3], -boundingCoordinates[5]};
        }

        protected void LoadVertices(vtkDataSet vtkModel)
        {
            int numberOfPoints = vtkModel.GetNumberOfPoints();
            Vertices = new double[numberOfPoints][];
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vertices[i] = vtkModel.GetPoint(i);
                Vertices[i][2] = -Vertices[i][2];
            }
        }

        protected virtual void LoadIndices(vtkDataSet vtkModel)
        {
            int numberOfCells = vtkModel.GetNumberOfCells();
            NumberOfFacetEdges = vtkModel.GetMaxCellSize();
            Indices = new int[NumberOfFacetEdges * numberOfCells];
            int currentIndexNumber = 0;
            for (int i = 0; i < numberOfCells; i++)
            {
                currentIndexNumber = LoadCellIndices(currentIndexNumber, vtkModel.GetCell(i).GetPointIds());
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

        private int LoadCellIndices(int currentIndexNumber, vtkIdList cellIndices)
        {
            int numberOfIndices = cellIndices.GetNumberOfIds();
            for (int j = 0; j < numberOfIndices; j++)
            {
                Indices[currentIndexNumber] = cellIndices.GetId(j);
                currentIndexNumber += 1;
            }
            return currentIndexNumber;
        }

        public string GetModelAsString()
        {
            StringBuilder modelString = new StringBuilder();
            modelString.Append("BOUNDS\n" + ConvertArrayToString(BoundingBox) + "\n");
            modelString.Append("NUMBER OF FACET EDGES " + NumberOfFacetEdges.ToString() + "\n");
            modelString.Append("VERTICES " + Vertices.Length.ToString() + "\n" + ConvertArrayToString(Vertices) + "\n");
            modelString.Append("INDICES\n" + ConvertArrayToString(Indices) + "\n");
            if (Vectors != null)
            {
                modelString.Append("VECTORS " + Vectors.Length.ToString() + "\n" + ConvertArrayToString(Vectors) + "\n");
            }
            if (Scalars != null)
            {
                modelString.Append("SCALARS " + Scalars.Length.ToString() + "\n" + ConvertArrayToString(Scalars) + "\n");
            }
            return modelString.ToString();
        }

        private string ConvertArrayToString(double[][] jaggedArray)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                string vertexStr = string.Join(" ", jaggedArray[i].Select(p => Math.Round(p, 5).ToString(CultureInfo.InvariantCulture)).ToArray());
                stringBuilder.Append(vertexStr + ' ');  
            }
            string finalString = stringBuilder.ToString();
            finalString = finalString.TrimEnd();
            return finalString;
        }

        private string ConvertArrayToString(int[] indicesArray)
        {
            string txtArray = String.Join(" ", indicesArray.Select(p => p.ToString()).ToArray());
            return txtArray;
        }
        private string ConvertArrayToString(double[] indicesArray)
        {
            string txtArray = String.Join(" ", indicesArray.Select(p => Math.Round(p, 5).ToString(CultureInfo.InvariantCulture)).ToArray());
            return txtArray;
        }
    }
}
