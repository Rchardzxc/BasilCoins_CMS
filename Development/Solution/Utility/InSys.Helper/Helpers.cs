using InSys.Context;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using z.Data;

namespace InSys.Helper
{
    public class Helpers
    {
        public Helpers() { }
        public static object IIf(bool condition, object IfTrue, object IfNot)
        {
            if (condition) return IfTrue;
            else return IfNot;
        }
        public static bool HasCompanyRights(HttpContext Ctx)
        {
            int ID_Roles = Ctx.Session.Get<int>("ID_Roles");
            int ID_Company = Ctx.Session.Get<int>("ID_Company");
            using (var db = new HelperDb(null))
            {
                if (db.Any("select * from dbo.tCompanyAccessRights where ID_Roles = {0} and ID_Company = {1}", ID_Roles, ID_Company)) return true;
                else return false;
            }
        }
        public static bool HasCompanyRights(BrowserSession Ctx)
        {
            int ID_Roles = Ctx.ID_Roles;
            int ID_Company = Ctx.ID_Company.ToInt32();
            using (var db = new HelperDb(null))
            {
                if (db.Any("select * from dbo.tCompanyAccessRights where ID_Roles = {0} and ID_Company = {1}", ID_Roles, ID_Company)) return true;
                else return false;
            }
        }
        public static string GetSqlQuery(string FileName, string model= "InSys.ITI.Models", params object[] parameters)
        {
            string Query = "";
            try
            {
                string asmName = model;
                //string asmName = "InSys.ITI.Models";
                Assembly asm = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.IndexOf(asmName) > -1).FirstOrDefault();
                if (asm == null) throw new Exception("Model resources not found.");

                using (var strm = asm.GetManifestResourceStream(asmName + ".Sql." + FileName + ".sql"))
                {
                    if (strm == null) throw new Exception("File not found.");
                    using (var reader = new StreamReader(strm))
                    {
                        Query = reader.ReadToEnd();
                    }
                }
                return Query;
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "GetSqlQuery", "internal", "InSys.Helpers");
                throw new Exception(msg);
            }
        }
        public static string CurrentUser(BrowserSession Session)
        {
            if (Session == null) throw new Exception("Session Expired.");
            try
            {
                string user = null;
                using (var db = new HelperDb(null))
                {
                    user = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", Session.ID_User).IsNull("internal").ToString();
                    return user;
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "CurrentUser", "internal", "InSys.Helpers");
                return "internal";
            }
        }
        public static string CurrentUser(HttpContext Ctx)
        {
            if (Ctx.Session.Keys.Count() == 0) throw new Exception("Session Expired.");
            try
            {
                string user = null;
                using (var db = new HelperDb(null))
                {
                    user = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", Ctx.Session.Get<int>("ID_User")).IsNull("internal").ToString();
                    return user;
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "CurrentUser", "internal", "InSys.Helpers");
                return "internal";
            }
        }
        public static List<TableSchema> GetSchema(string table, DbContext db = null)
        {
            db = db == null ? db = new HelperDb(null) : db;
            var ret = db.ExecQuery<TableSchema>("select * from dbo.fTableSchema({0})", table).ToList();
            db = null;
            return ret;
        }
        public static string CompanyRights(int ID_Roles)
        {
            string ret = "";
            using (var db = new HelperDb(null))
            {
                if (ID_Roles == 1)
                {
                    return string.Join(",", db.ExecQuery<tCompany>("select * from dbo.tCompany").Select(x => x.ID).ToList());
                }
                ret = string.Join(",", db.ExecQuery<tCompanyAccessRights>("select * from dbo.tCompanyAccessRights where ID_Roles = {0}", ID_Roles).Select(x => x.ID_Company).ToList());
            }
            return ret;
        }

        public static string getReferenceNumber(string tblName, int ID_Company)
        {
            string refNum = "";
            string additonalPrefix = "";
            string prefix = "";
            int newRefNum = 0;
            int lastRefNumCurrentLength = 0;
            int maxLength = 0;

            try
            {
                using (var db = new HelperDb(null))
                {
                    var CompanyReference = db.Single<tCompanyRefNum>("select * from dbo.tCompanyRefNum where TableName = {0} and ID_Company = {1}", tblName, ID_Company);
                    if (CompanyReference != null)
                    {
                        newRefNum = CompanyReference.LastRefNum + 1;
                        maxLength = CompanyReference.MaxLength;
                        additonalPrefix = CompanyReference.AdditionalPrefix;
                        prefix = CompanyReference.Prefix;
                        lastRefNumCurrentLength = newRefNum.ToString().Length;
                        additonalPrefix = additonalPrefix == null || additonalPrefix == "" ? "" : additonalPrefix + '-';
                        maxLength = maxLength < lastRefNumCurrentLength ? (maxLength + 1) : maxLength;
                        refNum = newRefNum.ToString();
                        while (maxLength > lastRefNumCurrentLength)
                        {
                            refNum = '0' + refNum;
                            lastRefNumCurrentLength += 1;
                        }
                        refNum = prefix + '-' + additonalPrefix + refNum;
                    }
                    else
                    {
                        throw new Exception("Data on Company Reference does not exists, Please check your parameter value");
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "getReferenceNumber", "Internal", "InSys.Helpers");
                throw new Exception(message);
            }
            return refNum;
        }

        public static void updateReferenceNum(string tblName, int ID_Company, BrowserSession ss)
        {
            try
            {
                using (var db = new HelperDb(ss))
                {
                    var CompanyReferenceNum = db.Single<tCompanyRefNum>("select * from dbo.tCompanyRefNum where TableName = {0} and ID_Company = {1}", tblName, ID_Company);
                    if (CompanyReferenceNum != null)
                    {
                        CompanyReferenceNum.LastRefNum += 1;
                        db.Update(CompanyReferenceNum);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Data does not exists. Please check your parameter value.");
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "updateReferenceNum", "Internal", "InSys.Helpers");
                throw new Exception(message);
            }
        }

        public static void SendNotification(object data, BrowserSession Session)
        {
            using (var db = new WebNotificationDB(Session))
            {
                db.Add(data);
                //await db.SaveChangesAsync();
                db.SaveChanges();
            }
        }
        public static void SendMail()
        {

        }

    }

    public class WebNotificationDB : InSysContext
    {
        public WebNotificationDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbSet<tWebNotification> tWebNotifications { get; set; }
    }
}
