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
            if (args.Length != 2)
            {
                throw new ArgumentException("Wrong number of parameters at the input!");
            }
            string inputRootDir = args[0];
            string outputFolder = args[1];

            ModelConverter modelConverter = new ModelConverter();
            modelConverter.Convert(inputRootDir, outputFolder);
        }
    }
}
