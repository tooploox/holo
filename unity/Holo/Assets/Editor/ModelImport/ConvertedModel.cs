using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport.LayerImport;

namespace ModelImport
{
    public class ConvertedModel : ModelImporter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private LayerImporter layerImporter = new LayerImporter();
        private string pathToTmp;
        public ConvertedModel(bool vtkConversion) : base()
        {
            pathToTmp = Path.GetFullPath(Application.dataPath + "/tmp/");
            if (vtkConversion)
            {
                ConvertVTKToTemp();
            }
        }

        protected override void ImportLayer(ModelLayerInfo layerInfo)
        {
            string objectName = Info.Caption + "_" + Path.GetFileName(layerInfo.Directory);
            layerImporter.ImportData(layerInfo, objectName);
            AddLayerComponent(layerImporter.ModelGameObject, layerInfo);
            SaveFilesForExport(layerInfo, objectName, layerImporter.ModelMesh.Get(), layerImporter.ModelGameObject);
        }

        private void ConvertVTKToTemp()
        {
            string pathToExe = Application.dataPath.Replace(@"/", @"\") + "\\VTKConverter\\";
            string command = pathToExe + "VTKConverter.exe " + RootDirectory + " " + pathToTmp;
            string rootFolderName = Path.GetFileName(RootDirectory);

            var p = new ProcessStartInfo("powershell.exe", command)
            {
                CreateNoWindow = false,
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
                throw new Exception();
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
            AssetDatabase.Refresh();
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