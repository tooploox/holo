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
           FileConverter fileConverter = new FileConverter();
            fileConverter.Convert(@"C:\Users\mit-kuchnowski\repos\holo\VTKConverter\path_lines-2843705301408_024.vtk");
        }
    }
}
