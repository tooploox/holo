using System.Linq;
using Kitware.VTK;

namespace ModelConversion.LayerConversion.FrameImport.VTK
{
    class FlowFrame : VTKFrame
    {
        private int numberOfVertices;

        public FlowFrame(string inputPath) : base(inputPath)
        {
            numberOfVertices = vtkModel.GetNumberOfCells();
            ImportVerticesAndVectors();
            ComputePointIndices(numberOfVertices);
            ImportFlowColors();
        }

        private void ImportVerticesAndVectors()
        {
            Vertices = new double[numberOfVertices][];
            Vectors = new double[numberOfVertices][];
            int currentVertexNumber = 0;
            for (int i = 0; i < numberOfVertices; i++)
            {
                int[] cellIds = new int[2] {
                    vtkModel.GetCell(i).GetPointIds().GetId(0),
                    vtkModel.GetCell(i).GetPointIds().GetId(1)
                };
                Vertices[currentVertexNumber] = vtkModel.GetPoint(cellIds[0]);
                Vectors[currentVertexNumber] = vtkModel.GetPoint(cellIds[1]).Zip(vtkModel.GetPoint(cellIds[0]), (vector, vertex) => vector - vertex).ToArray();
                Vertices[currentVertexNumber][2] = -Vertices[currentVertexNumber][2];
                Vectors[currentVertexNumber][2] = -Vectors[currentVertexNumber][2];
                currentVertexNumber += 1;
            }
        }

        private void ImportFlowColors()
        {
            // Kitware.VTK.dll automatically scales colours to 0-255 range.
            Scalars = new double[numberOfVertices][];
            vtkDataArray colors = vtkModel.GetCellData().GetScalars("Colors");
            for(int i = 0; i < numberOfVertices; i++)
            {
                Scalars[i] = TranslateColorAsFraction(colors.GetTuple3(i));
            }
        }

        private double[] TranslateColorAsFraction(double[] scalars)
        {
            for (int i = 0; i < 3; i++)
            {
                scalars[i] = scalars[i] / 255;
            }
            return scalars;
        }
    }
}
