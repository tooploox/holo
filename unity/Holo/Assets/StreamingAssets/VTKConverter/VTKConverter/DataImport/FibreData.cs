using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class FibreData : ModelData
    {
        private int numberOfPoints;

        public FibreData(vtkDataSet vtkModel)
        {
            BoundingBox = vtkModel.GetBounds();
            numberOfPoints = vtkModel.GetNumberOfPoints();
            SetPointIndices(numberOfPoints);
            GetVertices(vtkModel);
            GetVectors(vtkModel);
            GetAngles(vtkModel);
        }

        private void GetVectors(vtkDataSet vtkModel)
        {
            Vectors = new double[numberOfPoints][];

            vtkDataArray vtkVectors = vtkModel.GetPointData().GetVectors("fn");
            for (int i = 0; i < numberOfPoints; i++)
            {
               Vectors[i] = vtkVectors.GetTuple3(i);   
            }
        }

        private void GetAngles(vtkDataSet vtkModel)
        {
            Scalars = new double[numberOfPoints][];
            vtkPointData pointData = vtkModel.GetPointData();
            int arrayNumbers = pointData.GetNumberOfArrays();
            vtkDataArray alphaAngles = pointData.GetScalars("alpha");
            vtkDataArray betaAngles = pointData.GetScalars("beta");
            for (int i = 0; i < numberOfPoints; i++)
            {
                double[] currentScalars = { alphaAngles.GetTuple1(i), betaAngles.GetTuple1(i)};
                Scalars[i] = currentScalars;
            }
        }
    }
}
