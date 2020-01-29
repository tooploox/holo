using System;
using System.IO;

using Newtonsoft.Json;


namespace VTKConverter
{
    public class SingleModel
    {
        public ModelInfo Info { get; private set; }

        //Loads a single model, with its body and/or simulationData.
        public SingleModel(string rootDirectory)
        {
            ReadInfoFile(rootDirectory);
        }

        protected void ReadInfoFile(string rootDirectory)
        {
            if (!File.Exists(rootDirectory + @"\" + "ModelInfo.json"))
            {
                throw new Exception("No ModelInfo.json found in root folder!");
            }

            using (StreamReader streamReader = new StreamReader(rootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = streamReader.ReadToEnd();
                Info = JsonConvert.DeserializeObject<ModelInfo>(json);
            }
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = rootDirectory + @"\" + layerInfo.Directory;
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0)
            {
                throw new Exception("No layers found in ModelInfo.json file");
            }
        }
    }
}
