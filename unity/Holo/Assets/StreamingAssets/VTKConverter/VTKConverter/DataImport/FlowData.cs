using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class FlowData : ModelData
    {
        private int numberOfVertices;

        public FlowData(vtkDataSet vtkModel)
        {
            BoundingBox = vtkModel.GetBounds();
            numberOfVertices = vtkModel.GetNumberOfPoints() / 2;

            GetLineVerticesAndVectors(vtkModel);
            SetPointIndices(numberOfVertices);
            GetFlowColors(vtkModel);
        }

        private void GetLineVerticesAndVectors(vtkDataSet vtkModel)
        {
            Vertices = new double[numberOfVertices][];
            Vectors = new double[numberOfVertices][];
            int currentVertexNumber = 0;
            for (int i = 0; i < numberOfVertices * 2; i+=2)
            {
                Vertices[currentVertexNumber] = vtkModel.GetPoint(i);
                Vectors[currentVertexNumber] = vtkModel.GetPoint(i+1);
                currentVertexNumber += 1;
            }
        }

        private void GetFlowColors(vtkDataSet vtkModel)
        {
            // Kitware.VTK.dll automatically scales colours to 0-255 range.
            Scalars = new double[numberOfVertices][];
            vtkDataArray colors = vtkModel.GetCellData().GetScalars("Colors");
            for(int i = 0; i < numberOfVertices; i++)
            {
                Scalars[i] = colors.GetTuple3(i);
            }
        }
    }
}
