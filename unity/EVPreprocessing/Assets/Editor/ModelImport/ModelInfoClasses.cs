using System.Collections.Generic;

/* Classes that contain information about a model, usually deserialized from ModelInfo.json. */

namespace ModelImport
{
    // Information about a whole model, usually deserialized from ModelInfo.json.
    public class ModelInfo
    {
	    public string Caption;
        // Icon filename.
        // Initially (when deserialized from JSON) this is relative to ModelInfo.json,
        // but it is converted to an absolute path during SingleModel.ReadInfoFile,
        // so most of the code can safely assume it's an absolute filename.
        // Must be a PNG file now.
        public string IconFileName;
	    public List<ModelLayerInfo> Layers = new List<ModelLayerInfo>();
	}

    // Information about a single layer.
    public class ModelLayerInfo
    {
	    public string Caption;
		public string DataType;
		// Directory with VTK or GameObject models inside.
		// Initially (when deserialized) this is relative to ModelInfo.json,
        // but it is converted to an absolute path during SingleModel.ReadInfoFile,
        // so most of the code can safely assume it's an absolute filename.
		public string Directory;
        // In case of using the importer to load GameObject from Unity Assets,
        // this indicates the GameObject filename (appended to the Directory).
        public string AssetFileName;
        public bool UseAsIcon;
    }

    public class VolumetricMedata
    {
        public int width;
        public int height;
        public int depth;
        public int channels;
    }
}
