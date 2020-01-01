using System.Collections.Generic;

/* Classes that contain information about a model, usually deserialized from ModelInfo.json. */

namespace VTKConverter
{
    // Information about a whole model, usually deserialized from ModelInfo.json.
    public class ModelInfo
    {
        public string Caption;
        public List<ModelLayerInfo> Layers = new List<ModelLayerInfo>();
    }

    // Information about a single layer.
    public class ModelLayerInfo
    {
        public string Caption;
        public string Simulation;
        // Directory with VTK or GameObject models inside.
        // Initially (when deserialized) this is relative to ModelInfo.json,
        // but will be converted to an absolute path in the process.
        public string Directory;
        // In case of using the importer to load GameObject from Unity Assets,
        // this indicates the GameObject filename (appended to the Directory).
        public string AssetFileName;
    }
}