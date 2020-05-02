using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.OrgMasterList
{
    public class OrgMasterList : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OrgMasterList(_Session, _Parameter);
        }
        public OrgMasterList()
        {

        }
        public OrgMasterList(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }
        public override ReturnSet LoadList()
        {
            try
            {
                return new ReturnSet { };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }
        public ReturnSet LoadOrgType() 
        {
            try
            {
                using (var db = new OrgMasterListDB(Session)) {
                    var OrgTypes = db.ExecQuery<vOrgType>("SELECT * FROM tOrgType WHERE IsActive = 1").ToList();
                    return new ReturnSet 
                    { 
                        Data = OrgTypes,
                        Type = ReturnType .Result 
                    };
                }    
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }
        public override ReturnSet LoadForm()
        {
            try
            {

                return new ReturnSet { };
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
                var data = Parameter["Data"].ToObject<List<tOrgMasterList>>();
                using (var db = new OrgMasterListDB(Session))
                {
                    foreach (var row in data)
                    {
                        if (row.ID == 0)
                        {
                            db.Add(row);
                        }
                        else {
                            var record = db.tOrgMasterList.Where(x => x.ID == row.ID).AsNoTracking().SingleOrDefault();
                            if (record != null) db.Update(row);
                        }  
                    }
                    db.SaveChanges(true);
                }
                    return new ReturnSet { Type = ReturnType.Result };
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
                var deletedCount = 0;
                using (var db = new OrgMasterListDB(Session))
                {
                    foreach (var id in ids)
                    {
                        var record = db.tOrgMasterList.Where(x => x.ID == id).SingleOrDefault();
                        if (record != null) db.Remove(record);
                        deletedCount++;
                    }
                    db.SaveChanges(true);
                }
                    return new ReturnSet { Message = $"{deletedCount} record(s) deleted.",Type=ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet LoadOrgMasterList() {
            try
            {
                var ID_OrgType = Parameter["ID_OrgType"].IsNull(0).ToInt32();
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new OrgMasterListDB(Session))
                {
                    var record = db.QueryTable<vOrgMasterList>(@"(SELECT OML.*,OT.Name OrgType FROM dbo.tOrgMasterList AS OML 
                                                                 LEFT JOIN dbo.tOrgType AS OT ON OT.ID = OML.ID_OrgType
                                                                 WHERE OML.ID_OrgType = {0})a",to,ID_OrgType);

                    return new ReturnSet 
                    {
                        Data = new {Total = record.Count, Rows = record.Data.ToList() },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
    }
}
