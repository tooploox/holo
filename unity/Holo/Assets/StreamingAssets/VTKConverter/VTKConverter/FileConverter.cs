using Kitware.VTK;


namespace VTKConverter
{
    class FileConverter
    {
        public void Convert(string path, bool simulationFlag)
        {
            vtkDataSet vtkModel = ReadVTKData(path);
            ModelData modelData = new ModelData(vtkModel, simulationFlag);
            WriteModelToFile(modelData);
        }

        private vtkDataSet ReadVTKData(string path)
        {
            using (vtkDataSetReader reader = new vtkDataSetReader())
            {
                //TODO: Can I use vtkDataSet and leave it at that or Do I need to use PolyData/UnstructuredGridReader
                reader.SetFileName(path);
                reader.Update();
                vtkDataSet readerOutput = reader.GetOutput();
                return readerOutput;
            }
            
        }

        private void WriteModelToFile(ModelData modelData)
        { 
            //TODO: Write model properties into a temp txt file.
        }
    }
}

