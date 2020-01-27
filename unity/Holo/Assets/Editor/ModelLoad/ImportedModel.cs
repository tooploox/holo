using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelLoad.ModelImport;

namespace ModelLoad
{
    public class ImportedModel : SingleModel
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private FileSeriesImporter seriesImporter = new FileSeriesImporter();
        private string pathToTmp;
        public ImportedModel(bool vtkConversion) : base()
        {
            pathToTmp = Application.dataPath.Replace(@"/", @"\") + "\\tmp\\";
            if (vtkConversion)
            {
                ConvertVTKToTemp();
            }
        }

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            seriesImporter.ImportData(layerInfo, objectName);
            AddLayerComponent(seriesImporter.ModelGameObject, layerInfo);
            SaveFilesForExport(layerInfo, objectName, seriesImporter.ModelMesh.Get(), seriesImporter.ModelGameObject);
        }

        private void ConvertVTKToTemp()
        {
            string pathToExe = Application.dataPath.Replace(@"/", @"\") + "\\VTKConverter\\";
            string command = pathToExe + "VTKConverter.exe " + RootDirectory + " " + pathToTmp;
            string rootFolderName = Path.GetFileName(RootDirectory);

            var p = new ProcessStartInfo("powershell.exe", command)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(p);
            
            string error = process.StandardError.ReadToEnd();
            var message = process.StandardOutput.ReadToEnd();
            int exitcode = process.ExitCode;
            process.WaitForExit();
            if (!error.Equals(""))
            {
                Log.Error(error);
                throw new FileNotFoundException();
            }
            RootDirectory = pathToTmp + @"\" + rootFolderName;
        }

        // Saves imported model to a Unity-friendly files, to be put in AssetBundles.
        private void SaveFilesForExport(ModelLayerInfo layerInfo, string objectName, Mesh modelMesh, GameObject modelGameObject)
        {
            string rootAssetsDir = @"Assets/tmp/" + Info.Caption;
            
            if (!AssetDatabase.IsValidFolder(rootAssetsDir))
            {
                AssetDatabase.CreateFolder("Assets/tmp", Info.Caption);
            }
            AssetsPath.Add(objectName + "_mesh", rootAssetsDir + @"/" + objectName + ".asset");
            AssetDatabase.CreateAsset(modelMesh, AssetsPath[objectName + "_mesh"]);

            AssetsPath.Add(objectName + "_GameObject", rootAssetsDir + @"/" + objectName + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(modelGameObject, AssetsPath[objectName + "_GameObject"]);
        }

        public void DeleteTmpData()
        {
            Directory.Delete(pathToTmp, true);
        }
    }
}