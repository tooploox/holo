using System;
using System.IO;
using Kitware.VTK;
using VTKConverter.DataImport;

namespace VTKConverter
{
    class FileConverter
    {
        public void Convert(string inputPath, string outputRootDir, string dataType)
        {
            vtkDataSet vtkModel = ReadVTKData(inputPath);
            ModelData modelData = ImportModelData(vtkModel, dataType);
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            WriteModelToFile(modelData, fileName, outputRootDir);
            Console.WriteLine(fileName + " converted sucessfully.");
        }

        private vtkDataSet ReadVTKData(string path)
        {
            using (vtkDataSetReader reader = new vtkDataSetReader())
            {
                reader.ReadAllScalarsOn();
                reader.GetReadAllColorScalars();
                reader.SetFileName(path);
                reader.Update();
                vtkDataSet vtkModel = reader.GetOutput();
                return vtkModel;
            }

        }

        private ModelData ImportModelData(vtkDataSet vtkModel, string dataType)
        {
            ModelData modelData;
            switch (dataType)
            {
                case "anatomy":
                    modelData = new AnatomyData(vtkModel);
                    return modelData;
                case "fibre":
                    modelData = new FibreData(vtkModel);
                    return modelData;
                case "flow":
                    modelData = new FlowData(vtkModel);
                    return modelData;
                default:
                    throw new Exception("Wrong model type!");
            }
        }

        private void WriteModelToFile(ModelData modelData, string fileName, string outputRootDir)
        {
            string modelString = modelData.GetModelAsString();
            string outputPath = outputRootDir + @"\" + fileName + ".txt";
            using (StreamWriter file = new StreamWriter(outputPath))
            {
                file.Write(modelString);
            }
        }
    }
}

