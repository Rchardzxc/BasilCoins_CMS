using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.NatureOfRequest
{
    public class NatureOfRequest: BaseModule
    {

        public int? ID_User { get; set; }
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new NatureOfRequest(_Session, _Parameter);
        }

        public NatureOfRequest(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
            this.ID_User = Session.ID_User;
        }

        public NatureOfRequest() {
            
        }

        public override ReturnSet LoadList()
        {

            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                using (var db = new NatureOfRequestDb(Session))
                {
                    var NatureOfRequestsData = db.QueryTable<tNatureOfRequests>("(select * from dbo.tNatureOfRequests)a", to);
                    return new ReturnSet() { Data = new { Total = NatureOfRequestsData.Count, Rows = NatureOfRequestsData.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadNatureOfRequests", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }

        }

        public override ReturnSet Save()
        {
            int RecordId = 0;
            var NatureOfRequestData = Parameter["Data"].ToObject<tNatureOfRequests>();
            int NatureOfRequestId = NatureOfRequestData.ID;

            try
            {
                using (var db = new NatureOfRequestDb(Session))
                {
                    //New
                    if (NatureOfRequestId == 0)
                    {
                        NatureOfRequestData.DateTimeCreated = DateTime.Now;
                        NatureOfRequestData.ID_CreatedBy = ID_User.ToInt32();
                        db.Add(NatureOfRequestData);
                        db.SaveChanges();
                        RecordId = NatureOfRequestData.ID;
                    }
                    //Edit
                    else if (NatureOfRequestId > 0)
                    {
                        var NatureOfRequestToUpdate = db.Single<tNatureOfRequests>("select * from dbo.tNatureOfRequests where ID = {0}", NatureOfRequestId);

                        if (NatureOfRequestToUpdate != null)
                        {
                            NatureOfRequestData.DateTimeModified = DateTime.Now;
                            NatureOfRequestData.ID_ModifiedBy = ID_User.ToInt32();
                            db.Update(NatureOfRequestData);
                            db.SaveChanges();
                            RecordId = NatureOfRequestData.ID;
                        }
                        else
                        {
                            throw new Exception("Data does not exists");

                        }
                    }
                }
                return new ReturnSet() { Data = RecordId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveNatureOfRequests", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new NatureOfRequestDb(Session))
                {
                    var data = db.Single<tNatureOfRequests>("select * from dbo.tNatureOfRequests where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tNatureOfRequests()),
                            Schema = Helpers.GetSchema("tNatureOfRequests")
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

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new NatureOfRequestDb(Session))
                {

                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var NatureOfRequestToDelete = db.Single<tNatureOfRequests>("select * from dbo.tNatureOfRequests where ID = {0}", id);

                        //Do not allow system generated value
                        if (NatureOfRequestToDelete.IsSystemGenerated)
                            throw new Exception(NatureOfRequestToDelete.Name + " is system default value.");
                        else db.Remove(NatureOfRequestToDelete);
                    }
                    db.SaveChanges();
                }

                return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteNatureOfRequests", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }
    }
}
