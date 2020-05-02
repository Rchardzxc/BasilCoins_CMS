using InSys.Context;
using InSys.Helper;
using InSys.HRMS.Models;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Index
{
    public class Index
    {
        private BrowserSession Session { get; set; }
        private Pair param { get; set; }
        public Index(Pair _param, BrowserSession _ctx)
        {
            this.param = _param;
            this.Session = _ctx;
        }
        
        public virtual ReturnSet LoadCompany()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new IndexDb(Session))
                {
                    int ID_Roles = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", Session.ID_User).ID_Roles;
                    var companies = new List<tCompany>();
                    if (ID_Roles == 1)
                    {
                        companies = db.ExecQuery<tCompany>("select * from dbo.tCompany").ToList();
                    }
                    else
                    {
                        companies = db.ExecQuery<tCompany>("select tc.* from dbo.tCompany tc" +
                            " inner join dbo.tCompanyAccessRights tcar on tc.ID = tcar.ID_Company" +
                            " where tcar.ID_Roles = {0}", ID_Roles).ToList();
                    }
                    return r = new ReturnSet() { Data = companies, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadCompany", Helpers.CurrentUser(Session), "InSys.ITI.Index");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadProfile()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new IndexDb(Session))
                {
                    var employee = db.Single<vEmployee>($"{Helpers.GetSqlQuery("vEmployee")} where e.ID = {{0}}", param["ID"].ToInt32());
                    var persona = db.Single<vPersona>($"{Helpers.GetSqlQuery("vPersona")} where tp.ID = {{0}}", employee.ID_Persona);
                    var address = db.ExecQuery<vPersonaAddress>($"{Helpers.GetSqlQuery("vPersonaAddress")} WHERE tpa.ID_Persona = {{0}}", persona.ID).ToList();
                    var education = db.ExecQuery<vPersonaEducationalBackGround>(@"SELECT tpeb.*, tea.Name EducationAttainmentStatus FROM dbo.tPersonaEducationalBackGround tpeb
                                                                                LEFT JOIN dbo.tEducationalAttainment tea ON tpeb.ID_EducationAttainmentStatus = tea.ID where tpeb.ID_Persona = {0}", persona.ID).ToList();
                    var empHistory = db.ExecQuery<tPersonaEmployment>(@"SELECT tpeh.* FROM dbo.tPersonaEmployment tpeh WHERE tpeh.ID_Persona = {0}", persona.ID).ToList();
                    var licenses = db.ExecQuery<tPersonaLicensesAndCertificates>(@"SELECT tplc.* FROM dbo.tPersonaLicensesAndCertificates tplc WHERE tplc.ID_Persona = {0}", persona.ID).ToList();
                    return r = new ReturnSet() {
                        Data = new {
                            Employee = employee,
                            Persona = persona,
                            Address = address,
                            Education = education,
                            EmpHist = empHistory,
                            Licenses = licenses
                        }, Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadProfile", Helpers.CurrentUser(Session), "InSys.ITI.Index");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadNotification()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new IndexDb(Session))
                {
                    var data = db.ExecQuery<vWebNotification>("SELECT TOP 20 twn.*, tp.Name Receiver, tp2.Name Sender FROM dbo.tWebNotification twn" +
                        " LEFT JOIN dbo.tUsers tu ON twn.ID_Receiver = tu.ID" +
                        " LEFT JOIN dbo.tEmployee te ON tu.ID_Employee = te.ID" +
                        " LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID" +
                        " LEFT JOIN dbo.tUsers tu2 ON twn.ID_Sender = tu2.ID" +
                        " LEFT JOIN dbo.tEmployee te2 ON tu.ID_Employee = te2.ID" +
                        " LEFT JOIN dbo.tPersona tp2 ON te.ID_Persona = tp2.ID" +
                        " WHERE twn.ID_Receiver = {0} AND twn.IsSent = 1 ORDER BY twn.IsSeen ASC, twn.DateTimeCreated DESC", Session.ID_User);
                    return r = new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadNotification", Helpers.CurrentUser(Session), "InSys.ITI.Index");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet SeeNotification()
        {
            var r = new ReturnSet();
            try
            {
                int rID = param["ID"].ToInt32();
                using (var db = new IndexDb(Session))
                {
                    var notification = db.Single<tWebNotification>("select * from tWebNotification where ID = {0}", rID);
                    if (notification != null)
                    {
                        notification.IsSeen = true;
                        db.Update(notification);
                        db.SaveChanges();
                    }
                }
                r = new ReturnSet() { Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                var message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SeeNotification", Helpers.CurrentUser(Session), "InSys.ITI.Index");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
            return r;
        }

    }
    public class IndexDb: InSysContext
    {
        public IndexDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tWebNotification> tWebNotifications { get; set; }
        public DbSet<tCompany> tCompanies { get; set; }
        public DbQuery<BrowserSession> browserSessions { get; set; }
        public DbQuery<Profile> profiles { get; set; }
        public DbSet<tUsers> tUsers { get; set;}
        public DbQuery<vEmployee> vEmployees { get; set; }
        public DbQuery<vPersonaAddress> vPersonaAddresses { get; set; }
        public DbQuery<vPersonaEducationalBackGround> vPersonaEducationalBackGrounds { get; set; }
        public DbSet<tPersonaEmployment> tPersonaEmployments { get; set; }
        public DbSet<tPersonaLicensesAndCertificates> tPersonaLicensesAndCertificates { get; set; }
        public DbQuery<vPersona> vPersonas { get; set; }
        public DbQuery<vWebNotification> vWebNotifications { get; set; }
    }
}
