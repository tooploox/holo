using System.IO;
using Kitware.VTK;


namespace VTKConverter
{
    class FileConverter
    {
        public void Convert(string path, bool simulation)
        {
            using (vtkDataSetReader reader = new vtkDataSetReader())
            using (vtkDataSetWriter writer = new vtkDataSetWriter())
            {
                reader.SetFileName(path);
                vtkPolyData readerOutput = reader.GetPolyDataOutput();
                if (readerOutput.GetMaxCellSize() != 3)
                {
                    readerOutput = Triangulate(readerOutput);
                }
                vtkUnstructuredGrid unstructuredGrid = Polydata2UnstructuredGrid(readerOutput);
                writer.SetInput(unstructuredGrid);
                string outputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + "_gottem.vtu";
                writer.WriteToOutputStringOn();
                writer.SetHeader("");
                writer.Update();
                string vtkfile = writer.RegisterAndGetOutputString();
                StringConverter stringConverter = new StringConverter();
                vtkfile = stringConverter.ConvertString(vtkfile);
                File.WriteAllText(outputPath, vtkfile);
            }
        }

        private vtkPolyData Triangulate(vtkPolyData input)
        {
            vtkTriangleFilter triangleFilter = new vtkTriangleFilter();
            triangleFilter.SetInput(input);

            vtkPolyData output = triangleFilter.GetOutput();

            return output;
        }

        private vtkUnstructuredGrid Polydata2UnstructuredGrid(vtkPolyData input)
        {
            vtkAppendFilter appendFilter = new vtkAppendFilter();
            appendFilter.SetInput(input);
            appendFilter.Update();
            vtkUnstructuredGrid unstructuredGrid = appendFilter.GetOutput();

            return unstructuredGrid;
        }
    }
}

