using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.ReportGenerator.Classes
{
    public static class Config
    {
        public static string HRMSConnection = ConfigurationManager.AppSettings.Get("HRMSConnection");
        public static string KioskConnection = ConfigurationManager.AppSettings.Get("KioskConnection");
        public static DataTable GetTable(string query, string ConnectionString, params object[] parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                var matchCollection = Regex.Matches(query, @"(@[a-z|A-Z|0-9^]+)");
                using (var con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(query, con))
                    {
                        var idx = 0;
                        foreach (object value in parameters)
                        {
                            cmd.Parameters.AddWithValue(matchCollection[idx].Value, value);
                            idx += 1;
                        }
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception((ex.InnerException ?? ex).Message);
            }
        }
        public static DataRow Single(this DataTable source)
        {
            if (source.Rows.Count > 0)
                return source.Rows[0];
            else
                return null;
        }
        public static string GetConfigValue(this string source, string Name)
        {
            return source.Split(new char[] { ';' }).Where(x => x.Split(new char[] { '=' })[0] == Name).Select(x => x.Split(new char[] { '=' })[1]).FirstOrDefault();
        }
    }
}
