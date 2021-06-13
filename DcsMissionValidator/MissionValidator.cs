using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DcsMissionValidator
{
    internal class MissionValidator
    {
        public class Mission
        {
            private List<string> _Mods = new List<string>();
            private List<string> _InvalidMods = new List<string>();

            public Mission(Configuration configuration, string content)
            {
                Debug.Assert(configuration != null);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var start = content.IndexOf("[\"requiredModules\"] = ");
                    if ((start != -1) && (start < content.Length - 1))
                    {
                        var stop = content.IndexOf("[\"requiredModules\"]", start + 1);
                        if (stop != -1)
                        {
                            string sub = content.Substring(start, stop - start);
                            var split = sub.Split(new char[] { '\n'}, StringSplitOptions.RemoveEmptyEntries);
                            if (split != null &&
                                split.Length > 2 &&
                                split[0].Contains("[\"requiredModules\"] = ") &&
                                split[1].EndsWith("{") &&
                                split[split.Length - 1].Contains("}, -- end of"))
                            {
                                string expression = "(.*)(\\[\")(.*)(\"\\]) = (\")(.*)(\",)";
                                var regex = new Regex(expression);
                                for (int i = 2; i < split.Length - 1; i++)
                                {
                                    var match = regex.Match(split[i]);
                                    if ((match.Success) && (match.Groups.Count == 8))
                                    {
                                        string name = match.Groups[3].Value;
                                        if (name == match.Groups[6].Value)
                                        {
                                            Logger.Debug($"Found mod: {name}");
                                            this._Mods.Add(name);
                                            if (!configuration.IsModValid(name))
                                            {
                                                Logger.Debug($"Invalid mod found: {name}");
                                                this._InvalidMods.Add(name);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public List<string> Mods => this._Mods;
            public List<string> InvalidMods => this._InvalidMods;
        }


        private MissionValidator()
        {
        }

        private static bool HasFolder(ReadOnlyCollection<ZipArchiveEntry> entries, string folder)
        {
            if (!folder.EndsWith("/"))
            {
                folder += "/";
            }

            foreach (var entry in entries)
            {
                if (entry.FullName.StartsWith(folder))
                    return true;
            }

            return false;
        }
        private static bool Analyze(Configuration configuration, FileInfo fileInfo)
        {
            Debug.Assert(fileInfo != null);
            Debug.Assert(fileInfo.Exists);

            try
            {
                bool result = true;

                Logger.Debug($"Open MIZ-File");
                using (ZipArchive zip = ZipFile.Open(fileInfo.FullName, ZipArchiveMode.Read))
                {
                    // Folder: "track"
                    Logger.Debug($"Check folders");
                    if (HasFolder(zip.Entries, "track") || HasFolder(zip.Entries, "track_data"))
                    {
                        result = false;
                        Logger.Error($"File '{fileInfo.FullName}' contained 'track' or 'track_data' folder.");
                    }

                    // Mission file
                    Logger.Debug($"Check mission entry");
                    ZipArchiveEntry missionEntry = zip.GetEntry("mission");
                    if (missionEntry != null)
                    {
                        using (StreamReader streamWriter = new StreamReader(missionEntry.Open()))
                        {
                            var mission = new Mission(configuration, streamWriter.ReadToEnd());
                            var invalidMods = mission.InvalidMods;
                            if (invalidMods.Count > 0)
                            {
                                result = false;
                                Logger.Error($"File '{fileInfo.FullName}' contained invalid mods:");
                                foreach (var mod in invalidMods)
                                {
                                    Logger.Error($" - {mod}");
                                }
                            }
                            else
                            {
                                Logger.Debug($"No invalid mods found.");
                            }
                        }
                    }
                    else
                    {
                        result = false;
                        Logger.Error($"File '{fileInfo.FullName}' contains no 'mission' file.");
                    }

                }

                return result;
            }
            catch (Exception ex) // Be ready for the unexpected...
            {
                Logger.Error(ex);
                return false;
            }
        }
        public static bool Validate(Configuration configuration, FileInfo fileInfo, bool simulate)
        {
            if (configuration == null)
            {
                Logger.Error($"Validate(..): Parameter 'configuration' must not be <null>.");
                return false;
            }
            if (fileInfo == null)
            {
                Logger.Error($"Validate(..): Parameter 'fileInfo' must not be <null>.");
                return false;
            }

            if (!fileInfo.Exists)
            {
                Logger.Warn($"Skipping validation :-) ... File not found: {fileInfo.Name}.");
                return false;
            }

            try
            {
                // Analyze the miz-file.
                Logger.Debug($"Analyze: {fileInfo.FullName}");
                if (Analyze(configuration, fileInfo))
                {
                    return true;
                }
                else
                {
                    if (!simulate)
                    {
                        Logger.Info($"Deleting file '{fileInfo.FullName}' due to failed validation.");
                        fileInfo.Delete();
                    }
                    else
                    {
                        Logger.Info($"SIMULATE deleting file '{fileInfo.FullName}' due to failed validation.");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception while validate file: {fileInfo.FullName}");
                return false;
            }
        }
    }
}
