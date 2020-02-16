using System;
using System.IO;
using log4net.Config;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Filter;
using UnityEngine;


public static class LoggingConfiguration
{
    public static void Configure(string logFileDir)
    {
        string logFilepath = Path.GetFullPath(logFileDir + @"\" + DateTime.Now.ToString(@"dd.MM.yyyy\/HH-mm-ss") + ".log");

        var infoFileLogger = InitializeInfoLogger(logFilepath);
        var errorFileLogger = InitializeErrorLogger(logFilepath);
        var unityLogger = InitializeUnityLogger();

        BasicConfigurator.Configure(unityLogger, infoFileLogger, errorFileLogger);
    }

    private static RollingFileAppender InitializeInfoLogger(string logFilePath)
    {
        var infoPatternLayout = new PatternLayout
        {
            ConversionPattern = "%date %logger %level - %message%newline",
        };
        infoPatternLayout.ActivateOptions();

        var filter = new LevelRangeFilter
        {
            LevelMin = Level.Debug,
            LevelMax = Level.Info,
        };
        filter.ActivateOptions();
        var infofileAppender = new RollingFileAppender
        {
            AppendToFile = true,
            File = logFilePath,
            Layout = infoPatternLayout,
            LockingModel = new FileAppender.MinimalLock(),
            MaximumFileSize = "10MB",
            RollingStyle = RollingFileAppender.RollingMode.Size,
            StaticLogFileName = false
        };
        infofileAppender.AddFilter(filter);
        infofileAppender.ActivateOptions();

        return infofileAppender;
    }

    private static RollingFileAppender InitializeErrorLogger(string logFilePath)
    {
        var errorPatternLayout = new PatternLayout
        {
            ConversionPattern = "%date %logger %level - %message%newline%exception%stacktracedetail",
            IgnoresException = false,
        };
        errorPatternLayout.ActivateOptions();
        var filter = new LevelRangeFilter
        {
            LevelMin = Level.Warn,
            LevelMax = Level.Fatal
        };
        filter.ActivateOptions();
        var errorFileAppender = new RollingFileAppender
        {
            AppendToFile = true,
            File = logFilePath,
            LockingModel = new FileAppender.MinimalLock(),
            Layout = errorPatternLayout,
            MaximumFileSize = "10MB",
            RollingStyle = RollingFileAppender.RollingMode.Size,
            StaticLogFileName = false
        };
        errorFileAppender.AddFilter(filter);
        errorFileAppender.ActivateOptions();
        return errorFileAppender;
    }

    private static UnityAppender InitializeUnityLogger()
    {
        var unityLogger = new UnityAppender
        {
            Layout = new PatternLayout()
        };
        unityLogger.ActivateOptions();
        return unityLogger;
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

    public static Exception ThrowError(this log4net.ILog log, string message, Exception ex)
    {
        log.Error(message, ex);
        return ex;
    }
}