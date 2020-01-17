using System;
using System.IO;

namespace VTKConverter
{
    class ModelConverter
    {
        private string outputRootDir;

        public void Convert(string inputRootDir, string outputFolder)
        {
            CreateOutputRoot(inputRootDir, outputFolder);
            SingleModel singleModel = new SingleModel(inputRootDir);
            ConvertSingleModel(singleModel);
        }

        private void CreateOutputRoot(string inputRootDir, string outputFolder)
        {
            outputRootDir = outputFolder + @"\" + Path.GetFileName(inputRootDir);
            Directory.CreateDirectory(outputRootDir);
            File.Copy(inputRootDir + @"\ModelInfo.json", outputRootDir + @"\ModelInfo.json", true);
        }

        private void ConvertSingleModel(SingleModel singleModel)
        {
            Console.WriteLine("Conversion started!");
            foreach (ModelLayerInfo layerInfo in singleModel.Info.Layers)
            {
                string outputLayerDir = outputRootDir + @"\" + Path.GetFileName(layerInfo.Directory);
                Directory.CreateDirectory(outputLayerDir);
                ConvertLayer(layerInfo.Directory, outputLayerDir, layerInfo.DataType);
            }
        }

        private void ConvertLayer(string inputFolder, string outputLayerDir, string dataType)
        {
            FileConverter fileConverter = new FileConverter();
            string[] inputPaths = GetFilepaths(inputFolder);
            foreach (string inputPath in inputPaths)
            {
                fileConverter.Convert(inputPath, outputLayerDir, dataType);
            }
        }

        private string[] GetFilepaths(string rootDirectory)
        {
            string[] filePaths = Directory.GetFiles(rootDirectory + @"\");
            if (filePaths == null)
            {
                throw new Exception("No files found in: " + rootDirectory);
            }
            return filePaths;
        }
    }
}
