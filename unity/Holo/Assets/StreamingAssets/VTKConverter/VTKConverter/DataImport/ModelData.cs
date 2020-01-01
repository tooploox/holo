using System;
using System.Linq;
using System.Text;
using Kitware.VTK;

namespace VTKConverter.DataImport
{
    abstract class ModelData
    {
        public double[] BoundingBox { get; protected set; }
        public double[][] Vertices { get; protected set; }
        public int[] Indices { get; protected set; }
        public double[][] Vectors { get; protected set; }
        public double[][] Scalars { get; protected set; }

        public string GetModelAsString()
        {
            string modelString = "";
            modelString += "BOUNDS\n" + ConvertArrayToString(BoundingBox) + "\n";
            modelString += "VERTICES " + Vertices.Length.ToString() + "\n" + ConvertArrayToString(Vertices) + "\n";
            modelString += "INDICES\n" + ConvertArrayToString(Indices) + "\n";
            if (Vectors != null)
            {
                modelString += "VECTORS " + Vectors.Length.ToString() + "\n" + ConvertArrayToString(Vectors) + "\n";
            }
            if (Scalars != null)
            {
                modelString += "SCALARS " + Scalars.Length.ToString() + "\n" + ConvertArrayToString(Scalars) + "\n";
            }
            return modelString;
        }

        protected void GetVertices(vtkDataSet vtkModel)
        {
            int numberOfPoints = vtkModel.GetNumberOfPoints();
            Vertices = new double[numberOfPoints][];
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vertices[i] = vtkModel.GetPoint(i);
            }
        }

        protected virtual void GetIndices(vtkDataSet vtkModel)
        {
            int numberOfCells = vtkModel.GetNumberOfCells();
            int cellSize = vtkModel.GetMaxCellSize();
            Indices = new int[cellSize * numberOfCells];
            int currentIndexNumber = 0;
            for (int i = 0; i < numberOfCells; i++)
            {
                currentIndexNumber = GetCellIndices(currentIndexNumber, vtkModel.GetCell(i).GetPointIds());
            }
        }

        protected void SetPointIndices(int numberOfPoints)
        {
            Indices = Enumerable.Range(0, numberOfPoints).ToArray();
        }

        private int GetCellIndices(int currentIndexNumber, vtkIdList cellIndices)
        {
            int numberOfIndices = cellIndices.GetNumberOfIds();
            for (int j = 0; j < numberOfIndices; j++)
            {
                Indices[currentIndexNumber] = cellIndices.GetId(j);
                currentIndexNumber += 1;
            }
            return currentIndexNumber;
        }

        private string ConvertArrayToString(double[][] jaggedArray)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                string vertexStr = string.Join(" ", jaggedArray[i].Select(p => Math.Round(p, 5).ToString()).ToArray());
                stringBuilder.Append(vertexStr + " ");
                
            }
            return stringBuilder.ToString();
        }

        private string ConvertArrayToString(int[] indicesArray)
        {
            string txtArray = String.Join(" ", indicesArray.Select(p => p.ToString()).ToArray());
            return txtArray;
        }
        private string ConvertArrayToString(double[] indicesArray)
        {
            string txtArray = String.Join(" ", indicesArray.Select(p => Math.Round(p, 5).ToString()).ToArray());
            return txtArray;
        }
    }
}
