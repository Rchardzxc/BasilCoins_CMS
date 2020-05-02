using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Examination
{
    public class Examination: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Examination(_Session, _Parameter);
        }

        public Examination(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public Examination() { }

        public override ReturnSet LoadList()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ExaminationDb(Session))
                {
                    var ExaminationData = db.QueryTable<tExaminations>("(SELECT * FROM tExaminations)a", to).Data.ToList();

                    return new ReturnSet() { Data = new { Total = ExaminationData.Count, Rows = ExaminationData }, Type = ReturnType.Result };
                }

            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadExaminations", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            int RecordId = 0;
            var ExaminationData = Parameter["Data"].ToObject<tExaminations>();
            int ExaminationID = ExaminationData.ID;

            try
            {
                using (var db = new ExaminationDb(Session))
                {
                    //New
                    if (ExaminationID == 0)
                    {
                        db.Add(ExaminationData);
                        db.SaveChanges(true);
                        RecordId = ExaminationData.ID;
                    }
                    //Edit
                    else if (ExaminationID > 0)
                    {
                        var ExaminationToUpdate = db.Single<tExaminations>("SELECT * FROM tExaminations WHERE ID = {0}", ExaminationID);

                        if (ExaminationToUpdate != null)
                        {
                            db.Update(ExaminationData);
                            db.SaveChanges(true);
                            RecordId = ExaminationData.ID;
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
                Logger.LogError(ref message, "SaveExaminations", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }

        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ExaminationDb(Session))
                {
                    var data = db.Single<tExaminations>("SELECT * FROM tExaminations WHERE ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tExaminations()),
                            Schema = Helpers.GetSchema("tExaminations")
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
                using (var db = new ExaminationDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var ExaminationToDelete = db.Single<tExaminations>("SELECT * FROM tExaminations WHERE ID = {0}", id);
                        db.Remove(ExaminationToDelete);
                    }
                    db.SaveChanges();
                }
                return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteExamination", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }
    }
}
