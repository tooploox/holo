using System;
using System.IO;

using Newtonsoft.Json;


namespace VTKConverter
{
    public class SingleModel
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ModelInfo Info { get; private set; }

        //Loads a single model, with its body and/or simulationData.
        public SingleModel(string rootDirectory)
        {
            ReadInfoFile(rootDirectory);
        }

        protected void ReadInfoFile(string rootDirectory)
        {
            try
            {
                ReadModelInfoJson(rootDirectory);
            }
            catch (FileNotFoundException ex)
            {
                Log.Error("No ModelInfo.json found in root folder!", ex);
                throw;
            }
            foreach (ModelLayerInfo layerInfo in Info.Layers)
            {
                layerInfo.Directory = rootDirectory + @"\" + layerInfo.Directory;
            }

            if (Info.Layers.Count == 0)
            {
                var exception = new InvalidDataException();
                Log.Error("No layers found in ModelInfo.json file", exception);
                throw exception;
            }
        }

        private void ReadModelInfoJson(string rootDirectory)
        {
            using (StreamReader r = new StreamReader(rootDirectory + @"\" + "ModelInfo.json"))
            {
                string json = r.ReadToEnd();
                Info = JsonConvert.DeserializeObject<ModelInfo>(json);
            }
        }
    }
}
