using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using System;
using System.Linq;
using z.Data;

namespace InSys.ITI.OrbitChangeOfSchedule
{
    public class OrbitChangeOfSchedule : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OrbitChangeOfSchedule(_Session, _Parameter);
        }
        public OrbitChangeOfSchedule(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public OrbitChangeOfSchedule() { }
        public override ReturnSet LoadList()
        {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new OrbitChangeOfScheduleDb(Session))
                {
                    var data = db.QueryTable<vOrbiChangeOfScheduleList>(@"(SELECT * FROM dbo.fOrbiChangeOfScheduleList({0}))a", options, Session.ID_Company);
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
