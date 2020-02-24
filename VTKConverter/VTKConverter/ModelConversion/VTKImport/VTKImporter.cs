using System;
using System.IO;
using System.Text;
using Kitware.VTK;
using VTKConverter.DataImport;

namespace VTKConverter
{
    class VTKImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public VTKImporter()
        {
            var vtkOutput = vtkWin32OutputWindow.New();
            vtkOutput.SendToStdErrOn();
            vtkOutputWindow.SetInstance(vtkOutput);
        }

        public void Convert(string inputPath, string outputRootDir, string dataType)
        {
            vtkDataSet vtkModel = ReadVTKData(inputPath);
            ModelData modelData = ImportModelData(vtkModel, dataType);
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            WriteModelToFile(modelData, fileName, outputRootDir);
            Log.Info(fileName + " converted sucessfully.");
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
                return reader.GetOutput();
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
                    throw Log.ThrowError("Wrong model datatype in ModelInfo.json! \n Currently supporting: \"anatomy\" \"fibre\" and \"flow\" ", new IOException());
            }
            return modelData;
        }

        private void WriteModelToFile(ModelData modelData, string fileName, string outputRootDir)
        {
            string modelString = modelData.GetModelAsString();
            string outputPath = outputRootDir + @"\" + fileName + ".txt";
            using (StreamWriter file = new StreamWriter(outputPath, false, Encoding.ASCII, ushort.MaxValue))
            {
                file.Write(modelString);
            }
        }
    }
}