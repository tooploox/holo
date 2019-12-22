using Kitware.VTK;
using VTKConverter.DataImport;

namespace VTKConverter
{
    class FileConverter
    {
        public void Convert(string path, bool simulationFlag)
        {
            vtkDataSet vtkModel = ReadVTKData(path);
            ModelData modelData = ImportModelData(vtkModel, simulationFlag);
            WriteModelToFile(modelData);
        }

        private vtkDataSet ReadVTKData(string path)
        {
            using (vtkDataSetReader reader = new vtkDataSetReader())
            {
                //TODO: Can I use vtkDataSet and leave it at that or Do I need to use PolyData/UnstructuredGridReader
                reader.ReadAllScalarsOn();
                reader.SetFileName(path);
                reader.Update();
                vtkDataSet vtkModel = reader.GetOutput();
                return vtkModel;
            }
            
        }

        private ModelData ImportModelData(vtkDataSet vtkModel, bool simulationFlag)
        {
            ModelData modelData;
            switch (simulationFlag)
            {
                case true:
                    modelData = new FibreData(vtkModel);
                    return modelData;
                case false:
                    modelData = new AnatomyData(vtkModel);
                    return modelData;
                default:
                    throw new System.Exception("Wrong model type!");
            }
        }

        private void WriteModelToFile(ModelData modelData)
        { 
            //TODO: Write model properties into a temp txt file.
        }
    }
}

