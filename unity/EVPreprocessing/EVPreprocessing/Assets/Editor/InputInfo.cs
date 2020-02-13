using System;
using System.IO;
using UnityEngine;
using UnityEditor;

using NDesk.Options;

class InputInfo
{
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public string RootDirectory { get; private set; }
    public string OutputDir { get; private set; }
    public string LogFileDir { get; private set; }
    private OptionSet options = new OptionSet();
    public LoggingConfiguration LogConfig { get; private set; }

    public InputInfo()
    {
        if (Application.isBatchMode)
        {
            GetBatchModeArgs();
        }
        else
        {
            RootDirectory = EditorUtility.OpenFolderPanel("Select model root folder with ModelInfo.json", Application.dataPath, "");
        }
        if(string.IsNullOrEmpty(OutputDir))
        {
            OutputDir = Application.dataPath + "/StreamingAssets/";
        }
        if (string.IsNullOrEmpty(LogFileDir))
        {
            LogFileDir = Application.persistentDataPath + "/PreprocessingLogs/";
        }

        LogConfig = new LoggingConfiguration(LogFileDir);

        if (string.IsNullOrEmpty(RootDirectory))
        {
            var ex = new IOException();
            Log.Error("Path cannot be null!", ex);
            if (Application.isBatchMode)
            {
                DisplayHelp();
            }
            throw ex;
        }
    }
   
    private void GetBatchModeArgs()
    {
        string[] args = Environment.GetCommandLineArgs();

        options.Add("RootDirectory=", "Path to the root directory of the model.", rootdir => RootDirectory = rootdir)
            .Add("OutputPath:", "Directory where finished Asset Bundle will be stored.", outputdir => OutputDir = outputdir)
            .Add("LogDir:", "Directory where log file will be stored.", logdir => LogFileDir = logdir);

        options.Parse(args);
    }

    private void DisplayHelp()
    {
        Console.WriteLine("Options:");
        options.WriteOptionDescriptions(Console.Out);
    }
}

