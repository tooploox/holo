using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class FibreData : ModelData
    {
        private int numberOfPoints;

        public FibreData(vtkDataSet vtkModel) : base(vtkModel)
        {
            numberOfPoints = vtkModel.GetNumberOfPoints();
            ComputePointIndices(numberOfPoints);
            ImportVertices(vtkModel);
            ImportVectors(vtkModel);
            ImportAngles(vtkModel);
        }

        private void ImportVectors(vtkDataSet vtkModel)
        {
            Vectors = new double[numberOfPoints][];

            vtkDataArray vtkVectors = vtkModel.GetPointData().GetVectors("fn");
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vectors[i] = vtkVectors.GetTuple3(i);
                Vectors[i][2] = -Vectors[i][2];
            }
        }

        private void ImportAngles(vtkDataSet vtkModel)
        {
            Scalars = new double[numberOfPoints][];
            vtkPointData pointData = vtkModel.GetPointData();
            int arrayNumbers = pointData.GetNumberOfArrays();
            vtkDataArray alphaAngles = pointData.GetScalars("alpha");
            vtkDataArray betaAngles = pointData.GetScalars("beta");
            for (int i = 0; i < numberOfPoints; i++)
            {
                double[] currentScalars = { alphaAngles.GetTuple1(i), betaAngles.GetTuple1(i), 0.0};
                Scalars[i] = currentScalars;
            }
        }
    }
}
