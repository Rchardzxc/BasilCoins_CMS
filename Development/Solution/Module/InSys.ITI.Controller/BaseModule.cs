using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using z.Data;

namespace InSys.ITI.Controller
{
    public abstract class BaseModule
    {
        public BrowserSession Session { get; set; }
        public Pair Parameter { get; set; }
        public abstract BaseModule Initialize(BrowserSession _Session, Pair _Parameter);

        public static bool IsValidateError { get; set; }

        //Actions for basic CRUD
        public virtual ReturnSet LoadList() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadForm() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadFilingForm() { throw new NotImplementedException();  }

        public ReturnSet SaveForm()
        {
            var data = new ReturnSet() { Message = "Method not Implemented", Type = ReturnType.Error };
            try
            {
                BeforeSaveExecute();
                data = Save();
                if (data.Type == ReturnType.Error || data.Type == ReturnType.PageNotFound) return data;
                AfterSaveExecute();
            }
            catch(Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
            return data;
        }
        public virtual ReturnSet Save() { throw new NotImplementedException(); }
        public virtual void BeforeSaveExecute() { return; } //wag gagamit ng try catch or i throw lng ung message
        public virtual void AfterSaveExecute() { return; } //wag gagamit ng try catch or i throw lng ung message

        public virtual ReturnSet DeleteRecord() { throw new NotImplementedException(); }
        public virtual ReturnSet DeleteDetail() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadDetail() { throw new NotImplementedException(); }

        //Actions for setting status
        public virtual ReturnSet Approve() { throw new NotImplementedException(); }
        public virtual ReturnSet Disapprove() { throw new NotImplementedException(); }
        public virtual ReturnSet Cancel() { throw new NotImplementedException(); }
        public virtual ReturnSet Submit() { throw new NotImplementedException(); }
        public virtual ReturnSet Post() { throw new NotImplementedException(); }
        public virtual ReturnSet UnPost() { throw new NotImplementedException(); }
        public virtual ReturnSet Lock() { throw new NotImplementedException(); }
        public virtual ReturnSet UnLock() { throw new NotImplementedException(); }
        public virtual ReturnSet SetStatus() { throw new NotImplementedException(); }


        //Actions for custom loading record including LookUp
        public virtual ReturnSet LoadRecordDetails() { throw new NotImplementedException(); }
        
        //Use to show all approval history and other history related requests
        public virtual ReturnSet LoadHistory() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadLookup(){ throw new NotImplementedException(); }

        //Use to show all related approval list
        public virtual ReturnSet LoadApproval() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadApprovalDetail () { throw new NotImplementedException(); }

        //Actions for notifications
        public virtual ReturnSet SendNotification() { throw new NotImplementedException(); }

        //Actions for reports & external API
        public virtual ReturnSet LoadReport() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadExcel() { throw new NotImplementedException(); }

        public virtual ReturnSet LoadLookup<T>(DbContext ctx, String datasource) where T : class
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                var data = ctx.QueryTable<T>($"({datasource})a", to);

                return new ReturnSet
                {
                    Data = new
                    {
                        Total = data.Count,
                        Rows = data.Data.ToList()
                    },
                    Type = ReturnType.Result
                };

            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadApprovalHistory()
        {
            try
            {
                using (var db = new GlobalContext(Session))
                {
                    var menuCode = Parameter["MenuCode"].ToString();
                    var obj = Parameter["Data"].ToObject<JObject>();
                    var RefId = obj["ID"].IsNull(0).ToInt32();
                    var EmpId = Session.ID_Employee;
                    var MenuId = db.Menus.SingleOrDefault(x => x.Code == menuCode).Id.IsNull(0).ToInt32();

                    var approvalHistories = db.ExecQuery<vApprovalHistory>(@"SELECT ah.ID,ah.ID_Menus,ah.ID_Reference,ah.ApproverComment,ah.ApprovalAction,
		                                                                     ah.DateTimeCreated,ah.ApprovalLevel,ah.ID_TargetEmployee,ah.ID_Employee,p.Name Approver,p2.Name Employee
		                                                                     FROM dbo.tApprovalHistory ah
		                                                                     LEFT JOIN dbo.tEmployee e ON e.ID = ah.ID_Employee
		                                                                     LEFT JOIN dbo.tPersona p ON p.ID = e.ID_Persona
		                                                                     LEFT JOIN dbo.tEmployee e2 ON e2.ID = ah.ID_TargetEmployee
		                                                                     LEFT JOIN dbo.tPersona p2 ON p2.ID = e2.ID_Persona
                                                                             WHERE ID_Menus = {0} AND ID_Reference = {1} AND ID_TargetEmployee = {2}",MenuId,RefId,EmpId).OrderByDescending(x => x.ID).ToList();
                    return new ReturnSet { Data = approvalHistories,Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }

        public virtual ReturnSet ExceptionLogger(Exception ex, BrowserSession Session)
        {

            var st = new StackTrace();
            var sf = st.GetFrame(1);

            string message = (ex.InnerException ?? ex).Message;
            Logger.LogError(ref message, sf.GetMethod().Name, Helpers.CurrentUser(Session), sf.GetMethod().Module.Name);
            var r = new ReturnSet() { Message = message, Type = ReturnType.Error };
            return r;
        }

        //Generic Action
        public virtual ReturnSet ExecuteAction() { throw new NotImplementedException(); }
        public virtual ReturnSet LoadPDF() { throw new NotImplementedException(); }



    }
    public class GlobalContext : InSysContext
    {
        public GlobalContext(BrowserSession Session) : base(Session)
        {
        }
        public DbQuery<vApprovalHistory> ApprovalHistories { get; set; }
        public DbSet<tMenus> Menus { get; set; }
    }
}
