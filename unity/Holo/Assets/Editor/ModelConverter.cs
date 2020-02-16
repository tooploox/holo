using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

class ModelConverter
{
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public string OutputRootDir { get; private set; }
    public string TmpPath { get; private set; }
    private string errormsg = null;
    private static bool errorWritten = false;

    public ModelConverter()
    {
        TmpPath = AssetDirs.TempAssetsDir;
    }

    public void Convert(string inputRootDir)
    {
        OutputRootDir = Path.GetFullPath(TmpPath + "/" + Path.GetFileName(inputRootDir));
        var process = ConfigureProcess(inputRootDir);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        if (errorWritten)
        {
            throw new ConverterException();
        }
    }

    private Process ConfigureProcess(string inputRootDir)
    {
        string pathToExe = Path.GetFullPath(Application.dataPath + "/VTKConverter/");
        string command = pathToExe + "VTKConverter.exe " + "'" + Path.GetFullPath(inputRootDir) + "' '" + Path.GetFullPath(TmpPath) + "'";
        var startInfo = new ProcessStartInfo("powershell.exe", command)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        var process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
        process.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);
        return process;
    }

    private static void OutputDataHandler(object sendingProcess,
        DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            Log.Debug(outLine.Data);
        }
    }

    private static void ErrorDataHandler(object sendingProcess,
    DataReceivedEventArgs outLine)
    {
        var process = (Process) sendingProcess;
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            Log.Error(outLine.Data, new ConverterException());
            errorWritten = true;
        }
    }

    class ConverterException : Exception
    {
        public ConverterException() : base() { }

        public ConverterException(string msg) : base(msg) { }
    }
}