using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    static class Logging
    {
        
        static List<string> OpenLogs = new List<string>();

        internal static void Log(string Log, TraceEventType level, string Message, params object[] args)
        {
            lock (OpenLogs)
            {
#if Multifile_logging
            Message = String.Format(Message, args);
            string logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", Log + ".log");
#else
                Message = String.Format("{0}: \t{1}: {2}",Log,level.ToString(), String.Format(Message, args));
                string logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "debug" + ".log");
#endif
                if (!OpenLogs.Contains(logfile))
                {
                    File.WriteAllText(logfile, "");
                    OpenLogs.Add(logfile);
                }
                File.AppendAllLines(logfile, new string[] { Message });
            }

        }
        [Obsolete("Set a logging level")]
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
