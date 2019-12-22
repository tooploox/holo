using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class AnatomyData : ModelData
    {
        public AnatomyData(vtkDataSet vtkModel)
        {
            BoundingBox = vtkModel.GetBounds();
            GetVertices(vtkModel);
            GetIndices(vtkModel);
        }
    }
}
