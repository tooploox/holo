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
            var singleModel = new SingleModel(inputRootDir);

            CreateOutputRoot(inputRootDir, outputFolder, singleModel);
            ConvertSingleModel(singleModel);
        }

        private void CreateOutputRoot(string inputRootDir, string outputFolder, SingleModel singleModel)
        {
            outputRootDir = outputFolder + @"\" + Path.GetFileName(inputRootDir);
            Directory.CreateDirectory(outputRootDir);
            File.Copy(inputRootDir + @"\ModelInfo.json", outputRootDir + @"\ModelInfo.json", true);
            if (singleModel.Info.IconFileName != null)
            {
                File.Copy(inputRootDir + @"\" + singleModel.Info.IconFileName, 
                    outputRootDir + @"\" + singleModel.Info.IconFileName, true);
            }
        }

        private void ConvertSingleModel(SingleModel singleModel)
        {
            Log.Info(singleModel.Info.Caption + " conversion started!");
            foreach (ModelLayerInfo layerInfo in singleModel.Info.Layers)
            {
                ConvertLayer(layerInfo);
            }
        }

        private void ConvertLayer(ModelLayerInfo layerInfo)
        {
            string outputLayerDir = outputRootDir + @"\" + Path.GetFileName(layerInfo.Directory);
            Directory.CreateDirectory(outputLayerDir);
            var fileConverter = new VTKImporter();
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
                throw Log.ThrowError("No files found in: " + rootDirectory, new FileNotFoundException());
            }
            return filePaths;
        }
    }
}
