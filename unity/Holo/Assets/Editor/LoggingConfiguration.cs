using System.IO;
using log4net.Config;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public class LoggingConfiguration
{
    static LoggingConfiguration()
    {
        var patternLayout = new PatternLayout
        {
            ConversionPattern = "%date %logger %level - %message%newline%exception",
            IgnoresException = false
        };
        patternLayout.ActivateOptions();
        
        // setup the appender that writes to AppData/LocalLow/MicroscopeIT/PreprocessingLogs/
        var fileAppender = new RollingFileAppender
        {
            AppendToFile = true,
            DatePattern = @"dd.MM.yyyy/HH.mm.ss'.log'",
            File = Path.GetFullPath(Application.persistentDataPath + "/PreprocessingLogs/"),
            Layout = patternLayout,
            RollingStyle = RollingFileAppender.RollingMode.Date,
            StaticLogFileName = false
        };
        fileAppender.ActivateOptions();
        var unityLogger = new UnityAppender
        {
            Layout = new PatternLayout()
        };
        unityLogger.ActivateOptions();
        BasicConfigurator.Configure(unityLogger, fileAppender);
    }

    private class UnityAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            string message = RenderLoggingEvent(loggingEvent);
            if (Level.Compare(loggingEvent.Level, Level.Error) >= 0)
            {
                // everything above or equal to error is an error
                Debug.LogError(message);
            }
            else if (Level.Compare(loggingEvent.Level, Level.Warn) >= 0)
            {
                // everything that is a warning up to error is logged as warning
                Debug.LogWarning(message);
            }
            else if (Level.Compare(loggingEvent.Level, Level.Info) >= 0)
            {
                //If it's on the Dubug Level it will show in the Unity console, otherwise it's just in the logs.
                Debug.Log(message);
            }
        }
    }
    
}