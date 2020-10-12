using System.IO;

using ModelConversion.LayerConversion.FrameImport.VTK;

namespace ModelConversion.LayerConversion.FrameImport
{
    class FrameFactory
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string extension;
        private string dataType;

        public IFrame Import(string inputPath, string dataType)
        {
            extension = Path.GetExtension(inputPath);
            switch (extension)
            {
                case ".vtk":
                case ".vtu":
                case ".vtp":
                    return ImportVTKFrame(inputPath, dataType);
                default:
                    throw Log.ThrowError("Wrong file extension! Only supporting VTK files", new IOException());       
            }
        }

        private IFrame ImportVTKFrame(string inputPath, string dataType)
        {
            switch (dataType)
            {
                case "anatomy":
                    return new AnatomyFrame(inputPath);
                case "fibre":
                    return new FibreFrame(inputPath);
                case "flow":
                    return new FlowFrame(inputPath);
                default:
                    throw Log.ThrowError("Wrong model datatype in ModelInfo.json! \n Currently supporting: \"anatomy\" \"fibre\" and \"flow\" ", new IOException());
            }
        }
    }
}
