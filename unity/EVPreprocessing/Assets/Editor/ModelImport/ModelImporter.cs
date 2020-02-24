using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ModelImport
{
    public abstract class ModelImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ModelInfo Info { get; protected set; }
        public List<string> AssetPaths { get; protected set; } = new List<string>();
        public string RootDirectory { get; protected set;} 

        //Loads a single model, with its body and/or simulationData.
        public ModelImporter(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public void GetModelData()
        {
            ReadInfoFile();
            Log.Debug("Converting model: \"" + Info.Caption + "\"");
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                Log.Debug("Converting layer: \"" + layerInfo.Caption + "\" from: " + layerInfo.Directory);
                ImportLayer(layerInfo);
            }
            ImportIcon();
        }

        protected void ReadInfoFile()
        {
            if (!File.Exists(RootDirectory + @"\" + "ModelInfo.json"))
            {
                throw Log.ThrowError("No ModelInfo.json found in root folder!", new FileNotFoundException());
            }

            using (StreamReader r = new StreamReader(RootDirectory + @"\" + "ModelInfo.json"))
            {
                try
                {
                    string json = r.ReadToEnd();
                    Info = JsonConvert.DeserializeObject<ModelInfo>(json);
                }
                catch (JsonReaderException ex)
                {
                    throw Log.ThrowError("Corrupted ModelInfo.json file!", ex);
                }
            }

            // convert some paths to be absolute filenames
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = RootDirectory + @"\" + layerInfo.Directory;
            }
            if (!string.IsNullOrEmpty(Info.IconFileName))
            {
                Info.IconFileName = RootDirectory + @"\" + Info.IconFileName;
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0)
            {
                throw Log.ThrowError("No layers found in ModelInfo.json file at:" + RootDirectory, new InvalidDataException());
            }
        }

        private Texture2D layerAutomaticIcon;

        protected void LayerAutomaticIconGenerate(UnityEngine.Object obj)
        { 
            layerAutomaticIcon = IconGenerator.GetIcon(obj);
        }

        // Imports layer (with body or simulation blendshapes).
        // May also set layerAutomaticIcon.
        abstract protected void ImportLayer(ModelLayerInfo layerInfo);

        // Descendants must use this on all GameObjects representing layers
        protected void AddLayerComponent(GameObject go, ModelLayerInfo layerInfo)
        {
            ModelLayer layer = go.AddComponent<ModelLayer>();
            layer.Caption = layerInfo.Caption;
            layer.Simulation = CheckIfSimulation(layerInfo.DataType);
            layer.DataType = GetDataType(layerInfo.DataType);
        }

        private DataType GetDataType(string layerDataType)
        {
            return layerDataType == "volumetric" ? DataType.Volumetric : DataType.Mesh;
        }

        private bool CheckIfSimulation(string simulationFlag)
        {
            string[] simulationVariants = { "true", "fibre", "flow"};
            return simulationVariants.Contains(simulationFlag);
        }

        // Load icon from Info.IconFileName, add it to AssetPaths
        private void ImportIcon()
        {
            bool hasIconFileName = !string.IsNullOrEmpty(Info.IconFileName);
            if (hasIconFileName || layerAutomaticIcon != null)
            {
                /* Note: At one point I tried to optimize it, by detecting when
                 * icon is already in the assets
                 * ( Info.IconFileName.StartsWith(Application.dataPath) )
                 * and then just adding the existing asset-relative path to AssetPaths.
                 * 
                 * But it doesn't work: we need the file to be called "icon.asset"
                 * ("icon.png" is ignored by Unity bundle building, as it has unrecognized
                 * extension). So we need to read + write the file anyway.
                 */

                Texture2D texture;
                if (hasIconFileName) {
                    byte[] data = File.ReadAllBytes(Info.IconFileName);
                    texture = new Texture2D(1, 1);
                    texture.LoadImage(data);
                } else
                {
                    texture = layerAutomaticIcon;
                }

                string iconAssetPath = AssetDirs.TempAssetsDir + "/icon.asset";
                AssetDatabase.CreateAsset(texture, iconAssetPath);
                AssetPaths.Add(iconAssetPath);
            }
        }
    }
}
