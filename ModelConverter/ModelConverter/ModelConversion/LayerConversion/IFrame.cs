namespace ModelConversion.LayerConversion
{
    interface IFrame
    {
        double[] BoundingBox { get; }
        double[][] Vertices { get; }
        int NumberOfFacetEdges { get; }
        int[] Indices { get; }
        double[][] Vectors { get; }
        double[][] Scalars { get; }

        void NormalizeVectors(double scalingFactor);
    }
}
