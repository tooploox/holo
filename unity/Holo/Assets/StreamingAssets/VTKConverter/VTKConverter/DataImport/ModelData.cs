using Kitware.VTK;

namespace VTKConverter.DataImport
{
    abstract class ModelData
    {
        public double[] BoundingBox { get; protected set; }
        public double[][] Vertices { get; protected set; }
        public int[] Indices { get; protected set; }
        public float[] Vectors { get; protected set; }
        public float[] Scalars { get; protected set; }

        protected void SetVertices(vtkDataSet vtkModel)
        {
            int numberOfPoints = vtkModel.GetNumberOfPoints();
            Vertices = new double[numberOfPoints][];
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vertices[i] = vtkModel.GetPoint(i);
            }
        }

        protected void SetIndices(vtkDataSet vtkModel)
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
    }
}
