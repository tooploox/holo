using System;
using System.Collections.Generic;
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
            string inputPath = args[0];
            string outputPath = args[1];
            string dataType = args[2];
            FileConverter fileConverter = new FileConverter();
            fileConverter.Convert(inputPath, outputPath, dataType);
        }
    }
}
