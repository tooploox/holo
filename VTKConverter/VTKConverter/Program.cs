﻿using System;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

namespace VTKConverter
{
    class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                var exception =  new IOException();
                Log.Error("Wrong number of parameters at the input!\n" +
                    "VTKconverter.exe <path/to/model/root/folder> <path/to/the/root/folder>", exception);
                throw exception;
            }
            string inputRootDir = args[0];
            string outputFolder = args[1];

            ModelConverter modelConverter = new ModelConverter();
            modelConverter.Convert(inputRootDir, outputFolder);
        }
    }
}