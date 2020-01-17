using System;

namespace VTKConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Wrong number of parameters at the input!\n" +
                    "VTKconverter.exe <path/to/model/root/folder> <path/to/the/root/folder>");
            }
            string inputRootDir = args[0];
            string outputFolder = args[1];

            ModelConverter modelConverter = new ModelConverter();
            modelConverter.Convert(inputRootDir, outputFolder);
        }
    }
}