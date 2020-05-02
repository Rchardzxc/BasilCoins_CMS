using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.CostCenter
{
    public class CostCenter : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session,Pair _Parameter)
        {
            return new CostCenter(_Session,_Parameter);
        }
        public CostCenter()
        {

        }
        public CostCenter(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }
        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CostCenterDB(Session))
                {
                    return new ReturnSet { Data = new { Total = db.tCostCenter.Count(), Rows = db.tCostCenter.OrderByDescending(x => x.ID).ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet LoadForm()
        {
            try
            {
                var recordID = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new CostCenterDB(Session))
                {
                    var record = db.tCostCenter.Where(x => x.ID == recordID).SingleOrDefault();
                    return new ReturnSet
                    {
                        Data = new 
                        {
                            Form = record.IsNull(new tCostCenter()),
                            Schema = Helpers.GetSchema("tCostCenter")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet Save()
        { 
            try
            {
                var data = Parameter["Data"].ToObject<tCostCenter>();
                using (var db = new CostCenterDB(Session))
                {
                    if (db.tCostCenter.Where(x => x.ID == data.ID).Any())
                        db.Update(data);
                    else
                        db.Add(data);
                    db.SaveChanges(true);

                    return new ReturnSet { Data = data.ID, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet DeleteRecord()
        {
            try
            {
                var ids = Parameter["Data"].ToObject<List<int>>();
                var deletedRecord = 0;
                using (var db = new CostCenterDB(Session))
                {
                    foreach (var id in ids)
                    {
                        var record = db.tCostCenter.Where(x => x.ID == id).SingleOrDefault();
                        if (record != null) db.Remove(record);
                        deletedRecord++;
                    }
                    db.SaveChanges(true);
                    return new ReturnSet { Message = $"{deletedRecord} record(s) deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
    }
}
