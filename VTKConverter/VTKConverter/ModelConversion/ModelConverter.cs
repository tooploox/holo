using System;
using System.IO;

namespace VTKConverter
{
    class ModelConverter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string outputRootDir;

        public void Convert(string inputRootDir, string outputFolder)
        {
            CreateOutputRoot(inputRootDir, outputFolder);
            var singleModel = new SingleModel(inputRootDir);
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
            Log.Info("Conversion started!");
            foreach (ModelLayerInfo layerInfo in singleModel.Info.Layers)
            {
                ConvertLayer(layerInfo);
            }
        }

        private void ConvertLayer(ModelLayerInfo layerInfo)
        {
            string outputLayerDir = outputRootDir + @"\" + Path.GetFileName(layerInfo.Directory);
            Directory.CreateDirectory(outputLayerDir);
            var fileConverter = new VTKImporter(outputRootDir);
            string[] inputPaths = GetFilepaths(layerInfo.Directory);
            foreach (string inputPath in inputPaths)
            {
                fileConverter.Convert(inputPath, outputLayerDir, layerInfo.DataType);
            }
        }

        private string[] GetFilepaths(string rootDirectory)
        {
            string[] filePaths = Directory.GetFiles(rootDirectory + @"\");
            if (filePaths == null)
            {
                var ex = new FileNotFoundException();
                Log.Error("No files found in: " + rootDirectory, ex);
                throw ex;
            }
            return filePaths;
        }
    }
}
