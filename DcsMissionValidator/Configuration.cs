﻿using System;
using System.IO;
using System.Xml.Serialization;

namespace DcsMissionValidator
{
    public class Configuration
    {
#if (DEBUG)
        public bool Debug = true;
#else
        public bool Debug = false;
#endif
        public bool LogEnabled = true;
        public string LogFilename = "DcsMissionValidator.log";
        public string[] InvalidFolders;
        public string[] ValidMods;

        [XmlIgnore]
        private string Filename = "DcsMissionValidator.xml";

        private static Configuration Default()
        {
            var instance = new Configuration();

            instance.InvalidFolders = new string[]
            {
                "track",
                "track_data"
            };

            instance.ValidMods = new string[]
            {
                "476 vFG Range Targets by Noodle & Stuka",
                "Edge540 FM by Aero",
                "Military Aircraft Mod",
                "CivilAircraftMod",
                "A-4E-C"
            };

            return instance;
        }
        public static Configuration LoadFromFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                    using (TextReader reader = new StreamReader(filename, System.Text.Encoding.UTF8))
                    {
                        var instance = serializer.Deserialize(reader) as Configuration;
                        reader.Close();
                        instance.Filename = filename;
                        return instance;
                    }
                }
                else
                {
                    Console.Write("Configuration not found. Do you want to create one (y/n)? ");
                    var line = Console.ReadLine();
                    while (!line.StartsWith("y", StringComparison.InvariantCultureIgnoreCase) && (!line.StartsWith("n", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        line = Console.ReadLine();
                    }

                    if (line.StartsWith("y"))
                    {
                        var instance = Configuration.Default();
                        instance.Filename = filename;
                        instance.Save();
                        return instance;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while parsing configuration file: ", ex.Message);
                return null;
            }
        }
        public void Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                using (StreamWriter streamWriter = new StreamWriter(this.Filename, false, System.Text.Encoding.UTF8))
                {
                    serializer.Serialize(streamWriter, this, null);
                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while saving configuration file: ", ex.Message);
            }
        }
        public bool IsModValid(string modName)
        {
            if (this.ValidMods == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(modName))
            {
                return false;
            }

            foreach (var mod in this.ValidMods)
            {
                if (mod.Equals(modName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
