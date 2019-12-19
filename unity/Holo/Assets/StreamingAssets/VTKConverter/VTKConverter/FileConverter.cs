using Kitware.VTK;


namespace VTKConverter
{
    class FileConverter
    {
        public void Convert(string path, bool simulationFlag)
        {
            vtkUnstructuredGrid vtkModel = ReadVTKData(path);
            ModelData modelData = new ModelData(vtkModel, simulationFlag);
            WriteModelToFile(modelData);
        }

        private vtkUnstructuredGrid ReadVTKData(string path)
        {
            using (vtkDataSetReader reader = new vtkDataSetReader())
            {
                //TODO: Can I use vtkDataSet and leave it at that or Do I need to use PolyData/UnstructuredGridReader
                reader.SetFileName(path);
                vtkPolyData readerOutput = reader.GetPolyDataOutput();
                return Polydata2UnstructuredGrid(readerOutput);
            }
            
        }

        private vtkUnstructuredGrid Polydata2UnstructuredGrid(vtkPolyData input)
        {
            vtkAppendFilter appendFilter = new vtkAppendFilter();
            appendFilter.SetInput(input);
            appendFilter.Update();
            vtkUnstructuredGrid unstructuredGrid = appendFilter.GetOutput();

            return unstructuredGrid;
        }

        private void WriteModelToFile(ModelData modelData)
        { 
            //TODO: Write model properties into a temp txt file.
        }
    }
}

