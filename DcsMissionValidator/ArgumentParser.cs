using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DcsMissionValidator
{
    internal class ArgumentParser
    {
        private readonly bool _IsValid = false;
        private readonly bool _IsSimulate = false;
        private readonly bool _IsTextfile = false;
        private readonly bool _IsHelpRequested = false;
        private readonly List<string> _Filenames = new List<string>();
        private readonly List<string> _Directories = new List<string>();
        private readonly List<string> _Recursives = new List<string>();
        private readonly string _MonitorFolder = null;
        private readonly string _ConfigurationFilename = "DcsMissionValidator.xml";

        public ArgumentParser(string[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                foreach (var argument in args)
                {
                    if (argument.StartsWith("-file:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/file:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._Filenames.Add(argument.Substring(6, argument.Length - 6));
                    }
                    else if (argument.StartsWith("-dir:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/dir:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._Directories.Add(argument.Substring(5, argument.Length - 5));
                    }
                    else if (argument.StartsWith("-rec:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/rec:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._Recursives.Add(argument.Substring(5, argument.Length - 5));
                    }
                    else if (argument.StartsWith("-mon:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/mon:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._MonitorFolder = argument.Substring(5, argument.Length - 5);
                    }
                    else if (argument.StartsWith("-?", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/?", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsHelpRequested = true;
                    }
                    else if (argument.StartsWith("-sim", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/sim", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsSimulate = true;
                    }
                    else if (argument.StartsWith("-txt", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsTextfile = true;
                    }
                    else if (argument.StartsWith("-cfg:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/cfg:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._ConfigurationFilename = argument.Substring(5, argument.Length - 5);
                    }
                }
            }
        }

        public bool IsValid => this._IsValid;
        public bool IsSimulate => this._IsSimulate;
        public bool IsTextfile => this._IsTextfile;
        public bool IsHelpRequested => this._IsHelpRequested;

        public string[] Filenames
        {
            get
            {
                return this._Filenames.ToArray();
            }
        }
        public string[] Directories
        {
            get
            {
                return this._Directories.ToArray();
            }
        }
        public string[] Recursives
        {
            get
            {
                return this._Recursives.ToArray();
            }
        }
        public string ConfigurationFilename => this._ConfigurationFilename;
        public string MonitorFolder => this._MonitorFolder;

        private void AddFileInfosFromFilenames(List<FileInfo> fileInfoResults)
        {
            foreach (var filename in this._Filenames)
            {
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    fileInfoResults.Add(new FileInfo(filename));
                }
            }
        }
        private void AddFileInfosFromDirectories(List<FileInfo> fileInfoResults)
        {
            foreach (var directory in this._Directories)
            {
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    var directoryInfo = new DirectoryInfo(directory);
                    if (directoryInfo.Exists)
                    {
                        var fileInfos = directoryInfo.GetFiles("*.miz");
                        if (fileInfos != null)
                        {
                            fileInfoResults.AddRange(fileInfos);
                        }
                    }
                }
            }
        }
        private void AddFileInfosFromRecursives(List<FileInfo> fileInfoResults)
        {
            foreach (var recursive in this._Recursives)
            {
                if (!string.IsNullOrWhiteSpace(recursive))
                {
                    var directoryInfo = new DirectoryInfo(recursive);
                    if (directoryInfo.Exists)
                    {
                        var fileInfos = directoryInfo.GetFiles("*.miz", SearchOption.AllDirectories);
                        if (fileInfos != null)
                        {
                            fileInfoResults.AddRange(fileInfos);
                        }
                    }
                }
            }
        }

        public FileInfo[] GetFileInfos()
        {
            var list = new List<FileInfo>();
            this.AddFileInfosFromFilenames(list);
            this.AddFileInfosFromDirectories(list);
            this.AddFileInfosFromRecursives(list);
            return list.Distinct().ToArray();
        }

        public void PrintHelp()
        {
            Console.WriteLine("Command line information 'DcsMissionValidator.exe':");
            Console.WriteLine(" -?                      This text.");
            Console.WriteLine(" -file:{filename}        Validates the specified file.");
            Console.WriteLine(" -dir:{directory}        Validates all *.miz file of the specified directory.");
            Console.WriteLine(" -rec:{directory}        Recursivly validates all *.miz file of the specified directory and sub-directories.");
            Console.WriteLine(" -mon:{directory}        Permanently monitors a directory (incl sub-directories) and validates all *.miz files, when added or changed.");
            Console.WriteLine(" -sim                    SIMULATE validation. The file will not be deleted.");
            Console.WriteLine(" -txt                    Create a textfile for each FAILED validation.");
            Console.WriteLine("");
            Console.WriteLine("Samples:");
            Console.WriteLine("1. DcsMissionValidator.exe -file:test.miz -s");
            Console.WriteLine("2. DcsMissionValidator.exe -file:d:\\test.miz");
            Console.WriteLine("3. DcsMissionValidator.exe -file:\"d:\\some folder\\test.miz\"");
            Console.WriteLine("4. DcsMissionValidator.exe -dir:d:\\test");
            Console.WriteLine("4. DcsMissionValidator.exe -mon:d:\\test -txt");
            Console.WriteLine("5. DcsMissionValidator.exe -dir:\"d:\\some folder\"");
            Console.WriteLine("6. DcsMissionValidator.exe -file:test.miz -dir:D:\\AnotherOne -sim");
            Console.WriteLine("7. DcsMissionValidator.exe -file:test.miz -file:d:\\test2.miz -dir:\"D:\\Another One\" -s");
            Console.WriteLine("");
            Console.WriteLine("Have fun...");
        }

    }
}
