using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Linq;
using z.Data;

namespace InSys.ITI.OrbitMissedLog
{
    public class OrbitMissedLog : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OrbitMissedLog(_Session, _Parameter);
        }
        public OrbitMissedLog(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public OrbitMissedLog() { }

        public override ReturnSet LoadList()
        {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new OrbitMissedLogDb(Session))
                {
                    var data = db.QueryTable<vOrgMasterList>(@"(SELECT * FROM dbo.fOrbitMissedLogList({0}))a", options, Session.ID_Company);
                    return new ReturnSet
                    {
                        Data = new { Total = data.Count, Rows = data.Data.ToList() },
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
