using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using z.Data;

namespace Service.WebNotification.Classes
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
    public static class ConfigExtension
    {
        public static string GetValue(this object source, string name)
        {
            return source.GetType().GetProperty(name).GetValue(source).IsNull("").ToString();
        }
        public static void SetValue(this object source, string name, string value)
        {
            source.GetType().GetProperty(name).SetValue(source, value, null);
        }
        public static void LogEntry(this EventLog source, string message, EventLogEntryType entryType)
        {
            if (source == null) Console.WriteLine(message);
            else source.WriteEntry(message, entryType);
        }
    }
}
