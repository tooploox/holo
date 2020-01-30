using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Globalization;

class ModelConverter
{
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public string OutputRootDir { get; private set; }
    public string TmpPath { get; private set; }
    private string errormsg = null;

    public ModelConverter()
    {
        TmpPath = FileUtil.GetUniqueTempPathInProject();
    }

    public void Convert(string inputRootDir)
    {
        OutputRootDir = Path.GetFullPath(TmpPath + "/" + Path.GetFileName(inputRootDir));
        var process = ConfigureProcess(inputRootDir);

        process.Start();
        process.BeginOutputReadLine();
        errormsg = process.StandardError.ReadToEnd();
        int exitcode = process.ExitCode;
        process.WaitForExit();
        if (!string.IsNullOrEmpty(errormsg))
        {
            Log.Error(errormsg);
            throw new Exception();
        }
    }

    private Process ConfigureProcess(string inputRootDir)
    {
        string pathToExe = Path.GetFullPath(Application.dataPath + "/VTKConverter/");
        string command = pathToExe + "VTKConverter.exe " + "'" + Path.GetFullPath(inputRootDir) + "' '" + Path.GetFullPath(TmpPath) + "'";
        var startInfo = new ProcessStartInfo("powershell.exe", command)
        {
            CreateNoWindow = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        var process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);

        return process;
    }

    private static void OutputDataHandler(object sendingProcess,
        DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            Log.Debug(outLine.Data);
        }
    }
}