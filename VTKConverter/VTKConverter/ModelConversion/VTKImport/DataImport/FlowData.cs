using System.Linq;
using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class FlowData : ModelData
    {
        private int numberOfVertices;

        public FlowData(vtkDataSet vtkModel) : base(vtkModel)
        {
            numberOfVertices = vtkModel.GetNumberOfPoints() / 2;
            ImportVerticesAndVectors(vtkModel);
            ComputePointIndices(numberOfVertices);
            ImportFlowColors(vtkModel);
        }

        private void ImportVerticesAndVectors(vtkDataSet vtkModel)
        {
            Vertices = new double[numberOfVertices][];
            Vectors = new double[numberOfVertices][];
            int currentVertexNumber = 0;
            for (int i = 0; i < numberOfVertices * 2; i+=2)
            {
                Vertices[currentVertexNumber] = vtkModel.GetPoint(i);
                Vectors[currentVertexNumber] = vtkModel.GetPoint(i + 1).Zip(vtkModel.GetPoint(i), (vector, vertex) => vector - vertex).ToArray();
                Vertices[currentVertexNumber][2] = -Vertices[currentVertexNumber][2];
                Vectors[currentVertexNumber][2] = -Vectors[currentVertexNumber][2];
                currentVertexNumber += 1;
            }
        }

        private void ImportFlowColors(vtkDataSet vtkModel)
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
