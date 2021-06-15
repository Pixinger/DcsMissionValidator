using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DcsMissionValidator
{
    internal class MissionValidator
    {
        #region private class Mission
        private class Mission
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
                            var split = sub.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
        #endregion

        #region private class TextfileLogger
        private class TextfileLogger
        {
            private string _Filename;

            public TextfileLogger(string filename)
            {
                this._Filename = filename;
            }

            public void Write(string text)
            {
                // This method should be bullet proof!
                try
                {
                    using (StreamWriter file = new StreamWriter(this._Filename, true, Encoding.UTF8))
                    {
                        file.WriteLine(text);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Exception while writing to TextfileLogger.");
                }
            }
        }
        #endregion


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
        private static bool Analyze(Configuration configuration, FileInfo fileInfo, bool createTextfile)
        {
            Debug.Assert(fileInfo != null);
            Debug.Assert(fileInfo.Exists);

            // Create a textfile-logger if we need one.
            TextfileLogger textfileLogger = null;
            if (createTextfile)
            {
                textfileLogger = new TextfileLogger(fileInfo.FullName + ".txt");
            }

            try
            {
                bool result = true;

                Logger.Debug($"Open miz-File: {fileInfo.FullName}");
                using (ZipArchive zip = ZipFile.Open(fileInfo.FullName, ZipArchiveMode.Read))
                {
                    // Folder: "track"
                    Logger.Debug($"Check folders");
                    if (HasFolder(zip.Entries, "track") || HasFolder(zip.Entries, "track_data"))
                    {
                        result = false;
                        Logger.Error($"File '{fileInfo.FullName}' contained 'track' or 'track_data' folder.");
                        textfileLogger?.Write("The mission contained invalid folders. Maybe it was copied from a trk-file?");
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
                                    textfileLogger?.Write($"The mission contained an invalid mod: '{mod}'");
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
                        textfileLogger?.Write($"The mission contained no 'mission' file.");
                    }
                }

                return result;
            }
            catch (Exception ex) // Be ready for the unexpected...
            {
                Logger.Error(ex, $"Exception while analyzing file: {fileInfo.FullName}");
                textfileLogger?.Write($"Exception while analyzing file. EXCEPTION: {ex.Message}");
                return false;
            }
        }
        public static bool Validate(Configuration configuration, FileInfo fileInfo, bool simulate, bool createTextfile)
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
                if (Analyze(configuration, fileInfo, createTextfile))
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
