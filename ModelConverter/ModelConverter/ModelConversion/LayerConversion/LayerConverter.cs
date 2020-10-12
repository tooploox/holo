using System;
using System.IO;
using System.Linq;

using Kitware.VTK;

using ModelConversion.LayerConversion.FrameImport;
using ModelConversion.LayerConversion.FrameExport;

namespace ModelConversion.LayerConversion
{
    class LayerConverter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string outputRootDir;
        private double scalingFactor = -1;

        public LayerConverter(string outputRootDir)
        {
            this.outputRootDir = outputRootDir;
        }

        public void Convert(ModelLayerInfo layerInfo)
        {
            var frameFactory = new FrameFactory();
            var frameExporter = new FrameExporter();

            string outputLayerDir = outputRootDir + @"\" + Path.GetFileName(layerInfo.Directory);
            Directory.CreateDirectory(outputLayerDir);


            string[] inputPaths = GetFilepaths(layerInfo.Directory);
            CheckIfVTKFormat(inputPaths[0]);
            foreach (string inputPath in inputPaths)
            {
                string filename = Path.GetFileNameWithoutExtension(inputPath);

                IFrame frame = frameFactory.Import(inputPath, layerInfo.DataType);
                if (scalingFactor == -1)
                {
                    scalingFactor = GetScalingFactor(frame);
                }
                frame.NormalizeVectors(scalingFactor);
                frameExporter.ExportFrameToTxt(frame, filename, outputLayerDir);
                
                Log.Info(filename + " converted sucessfully.");
            }
        }

        private void CheckIfVTKFormat(string inputPath)
        {
            string[] vtkExtensions = { ".vtk", ".vtu", ".vtp" };
            string extension = Path.GetExtension(inputPath);
            if (Array.Exists(vtkExtensions, ex => ex == extension))
            {
                SetUpVTKDebugger();
            }
        }

        private double GetScalingFactor(IFrame frame)
        {
            double minAbsBounding = Math.Abs(frame.BoundingBox[0]);
            foreach (double bounding in frame.BoundingBox)
            {
                minAbsBounding = Math.Min(Math.Abs(minAbsBounding), Math.Abs(bounding));
            }

            double scalingPower = 0 - Math.Floor(Math.Log10(minAbsBounding));
            return Math.Pow(10, scalingPower);

        }

        private void SetUpVTKDebugger()
        {
            var vtkOutput = vtkWin32OutputWindow.New();
            vtkOutput.SendToStdErrOn();
            vtkOutputWindow.SetInstance(vtkOutput);
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