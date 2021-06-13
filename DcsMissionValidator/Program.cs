using System;
using System.Collections.Generic;
using System.IO;

namespace DcsMissionValidator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var argumentParser = new ArgumentParser(args);
                if (argumentParser.IsHelpRequested)
                {
                    argumentParser.PrintHelp();
                    return;
                }

                if (!argumentParser.IsValid)
                {
                    Console.WriteLine("ERROR: Invalid arguments. Use command line '-?' for more information.");
                    return;
                }

                var configuration = Configuration.LoadFromFile(argumentParser.ConfigurationFilename);
                if (configuration != null)
                {
                    Logger.SetDebug(configuration.Debug);
                    Logger.SetFilename(configuration.LogFilename);
                    Logger.SetFilename(configuration.LogFilename);
                    Logger.Debug("Debug enabled");
                    Logger.Debug($"Configfile: {configuration.LogFilename}");

                    // Get a list with FileInfo's from the argument parser. 
                    var fileInfos = argumentParser.GetFileInfos();
                    if (fileInfos != null)
                    {
                        // Validate all requested files.
                        foreach (var fileInfo in fileInfos)
                        {
                            MissionValidator.Validate(configuration, fileInfo, argumentParser.IsSimulate);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Mis2t");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: Main-Exception-Handler: " + ex.Message);
            }
        }
    }
}
