using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;

class ModelConverter
{
    public string OutputRootDir { get; private set; }
    public string TmpPath { get; private set; }

    private string inputRootDir;
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public ModelConverter(string inputRootDir)
    {
        this.inputRootDir = inputRootDir;
        TmpPath = FileUtil.GetUniqueTempPathInProject();
        Convert();
    }

    private void Convert()
    {
        string pathToExe = Application.dataPath.Replace(@"/", @"\") + "\\VTKConverter\\";
        string command = pathToExe + "VTKConverter.exe " + inputRootDir + " " + TmpPath;
        string rootFolderName = Path.GetFileName(inputRootDir);

        var p = new ProcessStartInfo("powershell.exe", command)
        {
            CreateNoWindow = false,
            RedirectStandardError = true,
            RedirectStandardOutput = false,
            UseShellExecute = false
        };
        var process = Process.Start(p);

        string error = process.StandardError.ReadToEnd();
        int exitcode = process.ExitCode;
        process.WaitForExit();
        if (!error.Equals(""))
        {
            Log.Error(error);
            throw new Exception();
        }
        OutputRootDir = TmpPath + @"\" + rootFolderName;
    }
}
