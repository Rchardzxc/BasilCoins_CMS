using InSys.Context;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSys.Helper
{
    public class Logger
    {
        public Logger() { }
        public static string WebRootPath { get; set; }
        public static void LogError(ref string message, string action, string user, string dllName)
        {
            try
            {
                string errorMsg = "";
                string fileName = $"{user}-Error-{DateTime.Now.ToString("MMddyyyyHHmmssfff")}.txt";

                if (!Directory.Exists(Path.Combine(WebRootPath, "ErrorLogs")))
                    Directory.CreateDirectory(Path.Combine(WebRootPath, "ErrorLogs"));

                while (File.Exists(Path.Combine(Path.Combine(WebRootPath, "ErrorLogs"), fileName)))
                    fileName = $"{user}-Error-{DateTime.Now.ToString("MMddyyyyHHmmssfff")}.txt";

                errorMsg = dllName + Environment.NewLine + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + Environment.NewLine + $"{user}: {message}" + Environment.NewLine + $"action: {action}";
                byte[] msg = Encoding.ASCII.GetBytes(errorMsg.ToString());

                using (var fs = new FileStream(Path.Combine(Path.Combine(WebRootPath, "ErrorLogs"), fileName), FileMode.OpenOrCreate))
                    fs.Write(msg, 0, msg.Length - 1);

                using (var db = new LoggerDb(null))
                {
                    message = db.ExecScalarString("select dbo.fGetIndexError({0}) as Value", message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
    }

    public class LoggerDb : InSysContext {
        public LoggerDb(BrowserSession Session) : base(Session)
        {
        }

        public DbQuery<StringReturn> stringReturns { get; set; }
    }
}
