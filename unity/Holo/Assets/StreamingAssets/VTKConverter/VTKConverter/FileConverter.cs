using System;
using System.IO;
using System.Text;
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
                reader.GetReadAllScalars();
                reader.ReadAllVectorsOn();
                reader.GetReadAllVectors();
                reader.ReadAllColorScalarsOn();
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
                    break;
                case "fibre":
                    modelData = new FibreData(vtkModel);
                    break;
                case "flow":
                    modelData = new FlowData(vtkModel);
                    break;
                default:
                    throw new Exception("Wrong model type!");
            }
            return modelData;
        }

        private void WriteModelToFile(ModelData modelData, string fileName, string outputRootDir)
        {
            string modelString = modelData.GetModelAsString();
            string outputPath = outputRootDir + @"\" + fileName + ".txt";
            using (StreamWriter file = new StreamWriter(outputPath, false, Encoding.ASCII, 65536))
            {
                file.Write(modelString);
            }
        }
    }
}

