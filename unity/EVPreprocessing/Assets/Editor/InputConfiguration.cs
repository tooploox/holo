using System;
using System.IO;
using UnityEngine;
using UnityEditor;

using NDesk.Options;

class InputConfiguration
{
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public string RootDirectory { get; private set; } = null;
    public string OutputDir { get; private set; } = null;
    public string LogFileDir { get; private set; } = null;
    private OptionSet options = new OptionSet();

    public InputConfiguration()
    {
        if (Application.isBatchMode)
        {
            GetBatchModeArgs();
        }
        else
        {
            RootDirectory = EditorUtility.OpenFolderPanel("Select model root folder with ModelInfo.json", Application.dataPath, "");
            OutputDir = EditorUtility.OpenFolderPanel("Select model output directory", Application.dataPath, "");
        }
        if (string.IsNullOrEmpty(OutputDir))
        {
            OutputDir = Application.dataPath + "/StreamingAssets/";
            Log.Info("OutputDir is empty, setting it to: " + OutputDir);
        }
        if (string.IsNullOrEmpty(LogFileDir))
        {
            LogFileDir = Application.persistentDataPath + "/PreprocessingLogs/";
            Log.Info("LogFile is empty, setting it to: " + LogFileDir);
        }

        LoggingConfiguration.Configure(LogFileDir);

        if (string.IsNullOrEmpty(RootDirectory))
        {
            if (Application.isBatchMode)
            {
                DisplayHelp();
            }
            throw Log.ThrowError("Path cannot be null!", new IOException()); 
        }
    }

    private void GetBatchModeArgs()
    {
        string[] args = Environment.GetCommandLineArgs();

        options.Add("RootDirectory=", "Path to the root directory of the model.", rootdir => RootDirectory = rootdir)
            .Add("OutputDir=", "Directory where finished Asset Bundle will be stored.", outputdir => OutputDir = outputdir)
            .Add("LogDir=", "Directory where log file will be stored.", logdir => LogFileDir = logdir);

        options.Parse(args);
    }

    private void DisplayHelp()
    {
        Console.WriteLine("Options:");
        options.WriteOptionDescriptions(Console.Out);
    }
}