using System;
using System.Linq;

namespace VTKConverter
{
    class StringConverter
    {
        public string ConvertString(string vtkfile)
        {
            vtkfile = RemoveHeader(vtkfile);
            vtkfile = SeparateVectors(vtkfile);
            vtkfile = RemoveCellTypes(vtkfile);
            if (vtkfile.Contains("COLOR_SCALARS"))
            {
                vtkfile = SeparateColors(vtkfile);
            }
            return vtkfile;
        }

        private string RemoveHeader(string vtkfile)
        {
            int pointsIndex = vtkfile.IndexOf("POINTS");
            return vtkfile.Remove(0, pointsIndex);
        }

        private string SeparateVectors(string vtkfile)
        {
            int startIndex = vtkfile.IndexOf("\n", vtkfile.IndexOf("POINTS")) + 1;
            int lastIndex = vtkfile.IndexOf("CELLS") - (startIndex + 1);
            string vectorSubstring = vtkfile.Substring(startIndex, lastIndex - 1);
            vtkfile = SeparateVectorsInText(vtkfile, vectorSubstring, startIndex, lastIndex);

            return vtkfile;
        }

        private string SeparateColors(string vtkfile)
        {
            int startIndex = vtkfile.IndexOf("\n", vtkfile.IndexOf("COLOR_SCALARS")) + 1;
            int lastIndex = vtkfile.Length - startIndex;
            string vectorSubstring = vtkfile.Substring(startIndex, lastIndex - 1);
            vtkfile = SeparateVectorsInText(vtkfile, vectorSubstring, startIndex, lastIndex);
            return vtkfile;
        }

        private string SeparateVectorsInText(string vtkfile, string vectorSubstring, int startIndex, int lastIndex)
        {
            vectorSubstring = AddNewlinesAfterEachVector(vectorSubstring);
            vtkfile = vtkfile.Remove(startIndex, lastIndex - 1);
            vtkfile = vtkfile.Insert(startIndex, vectorSubstring);
            return vtkfile;
        }

        private string RemoveCellTypes(string vtkfile)
        {
            int startIndex = vtkfile.IndexOf("CELL_TYPES");
            int removalLength = (vtkfile.IndexOf("CELL_DATA") - 1) - vtkfile.IndexOf("CELL_TYPES");
            vtkfile = vtkfile.Remove(startIndex,removalLength);
            return vtkfile;
        }
        private string AddNewlinesAfterEachVector(string substring)
        {
            substring = substring.Replace("\n", "");
            substring = string.Join(Environment.NewLine, substring.Split()
                .Select((word, index) => new { word, index })
                .GroupBy(x => x.index / 3)
                .Select(grp => string.Join(" ", grp.Select(x => x.word))));

            return substring;
        }
    }
}
