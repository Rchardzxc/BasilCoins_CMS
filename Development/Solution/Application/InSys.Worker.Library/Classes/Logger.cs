using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InSys.Worker.Library.Classes;

namespace InSys.Worker.Library.Classes
{
    public class Logger
    {
        public static void LogError(string message, string process, EventLog myLog)
        {
            myLog.LogEntry($"{process + " - " + message}", EventLogEntryType.Error);
        }
        public static void LogError(Exception ex, string process, EventLog myLog)
        {
            string message = (ex.InnerException ?? ex).Message.ToString();
            myLog.LogEntry($"{process + " - " + message}", EventLogEntryType.Error);
        }
        public static void LogMessage(string message, string process, EventLog myLog, EventLogEntryType entry)
        {
            myLog.LogEntry($"{process + " - " + message}", entry);
        }
    }
    
}
