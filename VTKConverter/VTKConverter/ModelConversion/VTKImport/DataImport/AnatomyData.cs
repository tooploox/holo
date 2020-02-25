﻿using Kitware.VTK;

namespace VTKConverter.DataImport
{
    class AnatomyData : ModelData
    {
        public AnatomyData(vtkDataSet vtkModel) : base(vtkModel)
        {
            ImportVertices(vtkModel);
            ImportIndices(vtkModel);
        }
    }
}
