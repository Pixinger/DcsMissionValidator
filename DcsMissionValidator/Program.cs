using System;
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

                    // -------------------------
                    // Run on demand
                    // Get a list with FileInfo's from the argument parser. 
                    var fileInfos = argumentParser.GetFileInfos();
                    if (fileInfos != null)
                    {
                        // Validate all requested files.
                        foreach (var fileInfo in fileInfos)
                        {
                            MissionValidator.Validate(configuration, fileInfo, argumentParser.IsSimulate, argumentParser.IsTextfile);
                        }
                    }

                    // -------------------------
                    // Watch specific directory
                    if (!string.IsNullOrWhiteSpace(argumentParser.MonitorFolder))
                    {
                        if (Directory.Exists(argumentParser.MonitorFolder))
                        {
                            Console.WriteLine($"Watching folder: {argumentParser.MonitorFolder}");
                            Console.WriteLine($"Press Ctrl+C to exit.");

                            using (var validationPool = new ValidationPool(configuration, argumentParser.IsSimulate, argumentParser.IsTextfile))
                            {
                                using (DirectoryWatcher directoryWatcher = new DirectoryWatcher(
                                    argumentParser.MonitorFolder, 
                                    (s, e) =>
                                    {
                                        if ((e != null) && (e.FileInfo != null))
                                        {
                                            validationPool.Add(e.FileInfo);
                                        }
                                    }))
                                {
                                    ConsoleTermination.WaitForControlC(); // Wait until Ctrl-C is pressed.
                                }
                                Logger.Debug("DirectoryWatcher scope left.");
                            }
                            Logger.Debug("ValidationPool scope left.");
                        }
                        else
                        {
                            Logger.Error($"Directory {argumentParser.MonitorFolder} not found.");
                        }
                    }
                    else
                    {
                        Logger.Debug($"No (-w)atch directory specified.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: Main-Exception-Handler: " + ex.Message);
            }
        }

        private static void ConsoleTermination_Terminate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
