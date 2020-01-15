using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class AnatomyData : ModelData
    {
        public AnatomyData(vtkDataSet vtkModel) : base(vtkModel)
        {
            GetVertices(vtkModel);
            GetIndices(vtkModel);
        }
    }
}
