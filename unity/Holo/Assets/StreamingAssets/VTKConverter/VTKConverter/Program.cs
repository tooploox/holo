using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTKConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Wrong number of parameters at the input!");
            }
            string inputRootDir = args[0];
            string outputRootDir = args[1];
            string dataType = args[2];
            FileConverter fileConverter = new FileConverter();
            string[] inputPaths = GetFilepaths(inputRootDir);
            foreach (string inputPath in inputPaths)
            {
                fileConverter.Convert(inputPath, outputRootDir, dataType);
            }
            
        }

        static string[] GetFilepaths(string rootDirectory)
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
