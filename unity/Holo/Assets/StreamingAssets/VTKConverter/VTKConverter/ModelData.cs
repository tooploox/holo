using Kitware.VTK;

namespace VTKConverter
{
    class ModelData
    {
        public double[] BoundingBox { get; private set; }
        public float[] Vertices { get; private set; }
        public int[] Indices { get; private set; }
        public float[] Vectors { get; private set; }
        public float[] Scalars { get; private set; }

        public ModelData(vtkDataSet vtkModel, bool simulationFlag)
        {
            //TODO: Implement reading vertices cells if simulation - vectors and colours
            BoundingBox = vtkModel.GetBounds();
            SetIndices(vtkModel);
        }

        private void SetVertices()
        {

        }

        private void SetIndices(vtkDataSet vtkModel)
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
