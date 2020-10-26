using Kitware.VTK;

namespace ModelConversion.LayerConversion.FrameImport.VTK
{
    class AnatomyFrame : VTKFrame
    {
        public AnatomyFrame(string inputPath) : base(inputPath)
        {
            ImportVertices();
            ImportIndices();
        }
    }
}
