using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            using (StreamReader r = new StreamReader(rootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = r.ReadToEnd();
                Info = JsonConvert.DeserializeObject<ModelInfo>(json);
            }
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = rootDirectory + @"\" + layerInfo.Directory;
            }

            // simple validation of the structure
            if (Info.Layers.Count == 0)
            {
                throw new Exception("No layers found in ModelInfo.{json,txt} file");
            }
        }
    }
}
