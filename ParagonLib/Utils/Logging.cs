using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    public static class Logging
    {
        
        static List<string> OpenLogs = new List<string>();

        public static void Log(string Log, TraceEventType level, string Message, params object[] args)
        {
            lock (OpenLogs)
            {
                Message = String.Format(Message, args);
                string logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", Log + ".log");
                if (!OpenLogs.Contains(logfile))
                {
                    File.WriteAllText(logfile, "");
                    OpenLogs.Add(logfile);
                }
                File.AppendAllLines(logfile, new string[] { Message });

                if (Log == "Xml Validation")
                    return;

                Message = String.Format("{0}: \t{1}: {2}", Log, level.ToString(), Message);
                logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "debug" + ".log");
                if (!OpenLogs.Contains(logfile))
                {
                    File.WriteAllText(logfile, "");
                    OpenLogs.Add(logfile);
                }
                File.AppendAllLines(logfile, new string[] { Message });
                
            }

        }
        [Obsolete("Set a logging level",true)]
        internal static void Log(string Log, string Message, params object[] args)
        {
            Logging.Log(Log, TraceEventType.Information, Message, args);
        }

        internal static void LogIf(bool test, TraceEventType level,string Log, string Message, params object[] args)
        {
            if (test)
                Logging.Log(Log, level, Message, args);
            
        }
    }
}
