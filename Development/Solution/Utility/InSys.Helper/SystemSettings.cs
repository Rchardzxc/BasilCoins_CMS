using InSys.Context;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InSys.Helper
{
    public class SystemSettings
    {
        public SystemSettings() { }
        public static List<tSystemSettings> tSystemSettingsCollection { get; set; }
        public static void LoadSystemSettings()
        {
            using (var db = new SystemDb(null))
            {
                tSystemSettingsCollection = db.ExecQuery<tSystemSettings>("select * from dbo.tSystemSettings").ToList();
            }
        }

        public static object GetSettingValue(string Name)
        {
            return tSystemSettingsCollection.Where(x => x.Name == Name).Select(x => x.Value).FirstOrDefault();
        }
    }

    public class SystemDb : InSysContext {
        public SystemDb(BrowserSession Session) : base(Session)
        {
        }

        public DbSet<tSystemSettings> tSystemSettings { get; set; }
    }
}
