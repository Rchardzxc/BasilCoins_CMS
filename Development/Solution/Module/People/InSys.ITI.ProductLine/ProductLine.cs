using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.ProductLine
{
    public class ProductLine : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ProductLine(_Session, _Parameter);
        }
        public ProductLine() {}
        public ProductLine(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList() {
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                using (var db = new ProductLineDb(Session))
                {
                    var data = db.QueryTable<tProductLine>("(select * from dbo.tProductLine)a", to);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet LoadForm() {
            try
            {
                int paramId = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new ProductLineDb(Session))
                {
                    var data = db.tProductLine.Where(x => x.Id == paramId).ToList();
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tProductLine()),
                            Schema = Helpers.GetSchema("tProductLine")
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
        public override ReturnSet Save() {

            var r = new ReturnSet();
            try
            {
                using (var db = new ProductLineDb(Session))
                {
                    tProductLine tfm = Parameter["Data"].ToObject<tProductLine>();
                    if (db.tProductLine.Where(x => (x.Id == tfm.Id)).Single() != null)
                        db.Update(tfm);
                    else db.Add(tfm);
                    db.SaveChanges(true);

                    return new ReturnSet() { Data = tfm.Id, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }

        }
        public override ReturnSet DeleteRecord() {
            try
            {
                using (var db = new ProductLineDb(Session)) {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();

                    foreach (var id in ids)
                    {
                        var ProductLine = db.tProductLine.Where(x => x.Id == id);
                        if (ProductLine != null)
                        {
                            db.Remove(ProductLine);
                        }
                    }
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Record Deleted", Type = ReturnType.Result };
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        
        }


    }
}
