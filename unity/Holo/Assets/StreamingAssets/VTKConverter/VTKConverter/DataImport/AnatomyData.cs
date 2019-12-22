using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class AnatomyData : ModelData
    {
        public AnatomyData(vtkDataSet vtkModel, bool simulationFlag)
        {
            BoundingBox = vtkModel.GetBounds();
            SetVertices(vtkModel);
            SetIndices(vtkModel);
        }
    }
}
