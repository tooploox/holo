using System;
using System.Linq;
using Kitware.VTK;
using VTKConverter.DataImport;

namespace VTKConverter
{
    class FileConverter
    {
        public void Convert(string inputPath, string outputPath, string dataType)
        {
            vtkDataSet vtkModel = ReadVTKData(inputPath);
            ModelData modelData = ImportModelData(vtkModel, dataType);
            WriteModelToFile(modelData, outputPath);
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
                    throw new System.Exception("Wrong model type!");
            }
        }

        private void WriteModelToFile(ModelData modelData, string outputPath)
        {
            string modelString = modelData.GetModelAsString();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputPath))
            {
                file.Write(modelString);
            }
        }
    }
}

