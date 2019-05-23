using System;
using System.IO;
using UnityEngine;

public class Logging : MonoBehaviour
{
    string LogFilePath;

    private void Start()
    {
        string LogFileName = DateTime.Now.ToString("ddMMyyyy-HHmmss") + "_log.txt";

        string LogFileDir = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(LogFileDir);

        LogFilePath = Path.Combine(LogFileDir, LogFileName);

        Debug.Log("Logging to file: " + LogFilePath);

        using (var logFile = File.CreateText(LogFilePath))
        {
            logFile.WriteLine(" ==== " + DateTime.Now.ToString() + " logging started ==== ");
        }

        Application.logMessageReceived += HandleLog;
    }

    public void LogToFile(string Text)
    {
        string LogTime = DateTime.Now.ToString("HH:mm:ss:fff");
        string Msg = LogTime + "    " + Text;
        using (var logger = File.AppendText(LogFilePath))
        {
            logger.WriteLine(Msg);
        }
    }

    void HandleLog(string LogString, string StackTrace, LogType Type)
    {
        LogToFile(LogString);
        if (!string.IsNullOrEmpty(StackTrace))
        {
            LogToFile(StackTrace);
        }
    }
}
