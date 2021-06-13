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
        private readonly bool _IsHelpRequested = false;
        private readonly List<string> _Filenames = new List<string>();
        private readonly List<string> _Directories = new List<string>();
        private readonly List<string> _Recursives = new List<string>();
        private readonly string _ConfigurationFilename = "DcsMissionValidator.xml";

        public ArgumentParser(string[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                foreach (var argument in args)
                {
                    if (argument.StartsWith("-f:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/f:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._Filenames.Add(argument.Substring(3, argument.Length - 3));
                    }
                    else if (argument.StartsWith("-d:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/d:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._Directories.Add(argument.Substring(3, argument.Length - 3));
                    }
                    else if (argument.StartsWith("-r:", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/r:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsValid = true;
                        this._Recursives.Add(argument.Substring(3, argument.Length - 3));
                    }
                    else if (argument.StartsWith("-?", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/?", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsHelpRequested = true;
                    }
                    else if (argument.StartsWith("-s", StringComparison.InvariantCultureIgnoreCase) ||
                        argument.StartsWith("/s", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._IsSimulate = true;
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
            AddFileInfosFromFilenames(list);
            AddFileInfosFromDirectories(list);
            AddFileInfosFromRecursives(list);
            return list.Distinct().ToArray();
        }

        public void PrintHelp()
        {
            Console.WriteLine("Command line information 'DcsMissionValidator.exe':");
            Console.WriteLine(" -f:{filename}         Validates the specified file.");
            Console.WriteLine(" -d:{directory}        Validates all *.miz file of the specified directory.");
            Console.WriteLine(" -s                    Simulate validation. The file will not be deleted.");
            Console.WriteLine(" -?                    This text.");
            Console.WriteLine("");
            Console.WriteLine("Samples:");
            Console.WriteLine("1. DcsMissionValidator.exe -f:test.miz -s");
            Console.WriteLine("2. DcsMissionValidator.exe -f:d:\\test.miz");
            Console.WriteLine("3. DcsMissionValidator.exe -f:\"d:\\some folder\\test.miz\"");
            Console.WriteLine("4. DcsMissionValidator.exe -d:d:\\test");
            Console.WriteLine("5. DcsMissionValidator.exe -d:\"d:\\some folder\"");
            Console.WriteLine("6. DcsMissionValidator.exe -f:test.miz -d:D:\\AnotherOne -s");
            Console.WriteLine("7. DcsMissionValidator.exe -f:test.miz -f:d:\\test2.miz -d:\"D:\\Another One\" -s");
            Console.WriteLine("");
            Console.WriteLine("Have fun...");
        }

    }
}
