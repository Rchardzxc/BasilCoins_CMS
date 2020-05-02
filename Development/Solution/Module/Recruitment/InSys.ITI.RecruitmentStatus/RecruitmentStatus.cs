using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.RecruitmentStatus
{
    public class RecruitmentStatus: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new RecruitmentStatus(_Session, _Parameter);
        }

        public RecruitmentStatus(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public RecruitmentStatus() { }

        public override ReturnSet LoadList()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new RecruitmentStatusDb(Session))
                {
                    var data = db.QueryTable<tRecruitmentStatus>("(select * from dbo.tRecruitmentStatus)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRecruitmentStatus", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }


        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new RecruitmentStatusDb(Session))
                {
                    var data = db.Single<tRecruitmentStatus>("select * from dbo.tRecruitmentStatus where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tRecruitmentStatus()),
                            Schema = Helpers.GetSchema("tRecruitmentStatus")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            int ReturnId = 0;
            var RecruitmentStatusData = Parameter["Data"].ToObject<tRecruitmentStatus>();
            int RecruitmentStatusId = RecruitmentStatusData.ID;
            try
            {
                using (var db = new RecruitmentStatusDb(Session))
                {
                    //New Data
                    if (RecruitmentStatusId == 0)
                    {
                        db.Add(RecruitmentStatusData);
                        db.SaveChanges(true);
                        //PostKioskRecruitmentStatus(RecruitmentStatusData);
                        ReturnId = RecruitmentStatusData.ID;
                    }
                    //Edit
                    else if (RecruitmentStatusId > 0)
                    {
                        var RecruitmentStatusToUpdate = db.Single<tRecruitmentStatus>("select * from dbo.tRecruitmentStatus where ID = {0}", RecruitmentStatusId);

                        if (RecruitmentStatusToUpdate != null)
                        {

                            db.Update(RecruitmentStatusData);
                            db.SaveChanges(true);
                            //PostKioskRecruitmentStatus(RecruitmentStatusData);
                            ReturnId = RecruitmentStatusData.ID;
                        }
                        else { throw new Exception("Data does not exists"); }
                    }
                }

                return new ReturnSet() { Data = ReturnId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveRecruitmentStatus", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new RecruitmentStatusDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var RecruitmentStatusToDelete = db.Single<tRecruitmentStatus>("select * from dbo.tRecruitmentStatus where ID = {0}", id);
                        if (RecruitmentStatusToDelete != null && RecruitmentStatusToDelete.IsSystemGenerated == false)
                        {
                            db.Remove(RecruitmentStatusToDelete);
                        }
                        else
                        {
                            if (RecruitmentStatusToDelete == null)
                            {
                                throw new Exception("Data does not exist");
                            }
                            else if (RecruitmentStatusToDelete.IsSystemGenerated == true)
                            {
                                throw new Exception("System generated data cannot be deleted");
                            }
                        }
                    }
                    //DeletePostedKioskRecruitmentStatus(ids);
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteRecruitmentStatus", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadLookup()
        {
            string ParamName = Parameter["Name"].ToString();
            ReturnSet Data = new ReturnSet();
            try {
                using (var db = new RecruitmentStatusDb(Session)) {
                    switch (ParamName)
                    {
                        case "RecruitmentStatusTypes":
                            Data = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, CAST(1 AS BIT) IsActive FROM dbo.fRecruitmentStatusTypes()");
                            break;
                    }
                    return new ReturnSet() { Data = Data.Data, Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual void PostKioskRecruitmentStatus(tRecruitmentStatus RecruitmentKioskData)
        {
            //try
            //{
            //    using (var db = new KioskContext())
            //    {
            //        var RecruitmentStatusToUpdate = db.Single<tKioskRecruitmentStatus>("select * from dbo.tKioskRecruitmentStatus where ID = {0}", RecruitmentKioskData.ID);

            //        if (RecruitmentStatusToUpdate == null)
            //        {
            //            var NewRecruitmentKioskData = new tKioskRecruitmentStatus();
            //            NewRecruitmentKioskData.ID = RecruitmentKioskData.ID;
            //            NewRecruitmentKioskData.Name = RecruitmentKioskData.Name;
            //            NewRecruitmentKioskData.Type = RecruitmentKioskData.Type;
            //            NewRecruitmentKioskData.IsSystemGenerated = RecruitmentKioskData.IsSystemGenerated;
            //            NewRecruitmentKioskData.Description = RecruitmentKioskData.Description;
            //            NewRecruitmentKioskData.IsActive = RecruitmentKioskData.IsActive;
            //            db.Add(NewRecruitmentKioskData);
            //            db.SaveChanges();
            //        }
            //        else if (RecruitmentStatusToUpdate != null)
            //        {
            //            RecruitmentStatusToUpdate.ID = RecruitmentKioskData.ID;
            //            RecruitmentStatusToUpdate.Name = RecruitmentKioskData.Name;
            //            RecruitmentStatusToUpdate.Type = RecruitmentKioskData.Type;
            //            RecruitmentStatusToUpdate.IsSystemGenerated = RecruitmentKioskData.IsSystemGenerated;
            //            RecruitmentStatusToUpdate.Description = RecruitmentKioskData.Description;
            //            RecruitmentStatusToUpdate.IsActive = RecruitmentKioskData.IsActive;
            //            db.Update(RecruitmentStatusToUpdate);
            //            db.SaveChanges(true);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string message = (ex.InnerException ?? ex).Message;
            //    Logger.LogError(ref message, "PostKioskRecruitmentStatus", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment", new KioskContext());
            //    throw new Exception(message);
            //}
        }

        public virtual void DeletePostedKioskRecruitmentStatus(List<int> ids)
        {
            //try
            //{
            //    using (var db = new KioskContext())
            //    {
            //        foreach (int id in ids)
            //        {
            //            var RecruitmentStatusToDelete = db.Single<tKioskRecruitmentStatus>("select * from dbo.tKioskRecruitmentStatus where ID = {0}", id);
            //            if (RecruitmentStatusToDelete != null) db.Remove(RecruitmentStatusToDelete);
            //        }
            //        db.SaveChanges();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string message = (ex.InnerException ?? ex).Message;
            //    Logger.LogError(ref message, "DeletePostedKioskRecruitmentStatus", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment", new KioskContext());
            //    throw new Exception(message);
            //}
        }
    }
}
