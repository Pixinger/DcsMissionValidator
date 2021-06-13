using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DcsMissionValidator
{
    internal static class Logger
    {
#if(DEBUG)
        private static bool _DebugEnabled = true;
#else
        private static bool _DebugEnabled = false;
#endif
        private static bool _WriteFileEnabled = true;
        private static string _Filename = "DcsMissionValidator.log";

        public static void SetFilename(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _Filename = filename;
            }
        }
        public static void SetEnabled(bool enabled)
        {
            _WriteFileEnabled = enabled;
        }
        public static void SetDebug(bool enabled)
        {
            _DebugEnabled = enabled;
        }

        private static void WriteFileTarget(string text)
        {
            if (_WriteFileEnabled)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(_Filename, true, Encoding.UTF8))
                    {
                        file.WriteLine(text);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception while writing to log file: ", ex.Message);
                }
            }
        }
        private static void WriteConsoleTarget(string text)
        {
            Console.WriteLine(text);
            System.Diagnostics.Debug.WriteLine(text);
        }


        public static void Debug(string text)
        {
            if (_DebugEnabled)
            {
                string textTmp = $"DEBUG: {text}";
                WriteConsoleTarget(textTmp);
                WriteFileTarget(textTmp);
            }
        }
        public static void Info(string text)
        {
            WriteConsoleTarget(text);
            WriteFileTarget(text);
        }
        public static void Warn(string text)
        {
            string textTmp = $"WARN: {text}";
            WriteConsoleTarget(textTmp);
            WriteFileTarget(textTmp);
        }
        public static void Error(string text)
        {
            string textTmp = $"ERROR: {text}";
            WriteConsoleTarget(textTmp);
            WriteFileTarget(textTmp);
        }
        public static void Error(Exception ex)
        {
            Error(ex, "Unexpected exception.");
        }
        public static void Error(Exception ex, string text)
        {
            Info("------------------------------------------------------------");
            Info("EXCEPTION");
            Info(text);
            Info(ex.Message);
            Info("------------------------------------------------------------");
            Info(ex.ToString());
            Info("------------------------------------------------------------");
        }
    }
}
