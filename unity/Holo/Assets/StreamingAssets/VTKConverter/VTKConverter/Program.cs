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
            if (args.Length != 2)
            {
                throw new ArgumentException("Wrong number of parameters at the input!");
            }
            string path = args[0];
            string dataType = args[1];
            FileConverter fileConverter = new FileConverter();
            fileConverter.Convert(path, dataType);
        }
    }
}
