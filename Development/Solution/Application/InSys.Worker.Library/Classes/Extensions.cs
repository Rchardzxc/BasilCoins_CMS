using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.Worker.Library.Classes
{
    public class Extensions
    {
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
