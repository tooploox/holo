using System.IO;

namespace ModelConversion
{
    class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            LoggingConfiguration.Configure();
            Log.Info("VTKConverter started!");
            if (args.Length != 2)
            {
                throw Log.ThrowError("Wrong number of parameters at the input!\n VTKconverter.exe <path/to/model/root/folder> <path/to/the/root/folder>", 
                    new IOException());
            }
            string inputRootDir = args[0];
            string outputFolder = args[1];

            ModelConverter modelConverter = new ModelConverter();
            modelConverter.Convert(inputRootDir, outputFolder);
        }
    }
}