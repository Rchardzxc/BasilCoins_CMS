using InSys.Helper;
using InSys.ITI.Common;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.MovementType
{
    public class MovementType: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new MovementType(_Session, _Parameter);
        }

        public MovementType(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public MovementType() { }

        public override ReturnSet LoadList() {
            var r = new ReturnSet();
            try {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new MovementTypeDb(Session)) {
                    var data = db.QueryTable<vMovementTypeList>("(SELECT * FROM dbo.vMovementTypeList)a", to);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try {
                var Id = Parameter["ID"].ToObject<int>();
                using (var db = new MovementTypeDb(Session)) {
                    var data = db.tMovementType.Where(x => x.ID == Id).FirstOrDefault();

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tMovementType() {
                                ID_CreatedBy = Session.ID_User
                            }),
                            Schema = Helpers.GetSchema("tMovementType")
                        },
                        Type = ReturnType.Result
                    };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadDetail()
        {
            try
            {
                var Id = Parameter["ID"].ToObject<int>();
                using (var db = new MovementTypeDb(Session)) {
                    var MovementTypeDetails = db.tMovementTypeFields.Where(x => x.Id_MovementType == Id).ToList();

                    return new ReturnSet()
                    {
                        Data = MovementTypeDetails,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet Save()
        {
            try {
                var data = Parameter["Data"].ToObject<tMovementType>();
                var detail = Parameter["Detail"].ToObject<List<tMovementTypeFields>>();
                var deletedRecord = Parameter["DeletedRecord"].ToObject<List<int>>();
                using (var db = new MovementTypeDb(Session)) {
                    //var record = db.tMovementType.Where(x => x.ID == data.ID);

                    //if (record != null)

                    if (data.ID > 0) {
                        data.ID_ModifiedBy = Session.ID_User;
                        data.ModifiedAt = DateTime.Now;
                        db.Update(data);
                    } else {
                        data.ID_CreatedBy = Session.ID_User;
                        data.CreatedAt = DateTime.Now;
                        db.Add(data);
                    }

                    db.SaveChanges(true);

                    foreach (var rec in detail) {
                        rec.Id_MovementType = data.ID; 
                        db.Update(rec);
                    }

                    //remove deleted Detail records
                    foreach (var del in deletedRecord) {
                        var delRecord = db.tMovementTypeFields.Where(x => x.Id == del).FirstOrDefault();
                        db.tMovementTypeFields.Remove(delRecord);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Data = data.ID, Message = "Successfully Saved.", Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }

        }
        
        public override ReturnSet DeleteRecord()
        {
            try {
                var Id = Parameter["ID"].ToObject<int>();
                using (var db = new MovementTypeDb(Session)) {

                    var MovementType = db.tMovementType.Where(x => x.ID == Id);
                    var MovementTypeFields = db.tMovementTypeFields.Where(x => x.Id_MovementType == Id).FirstOrDefault();
                    db.Remove(MovementType);
                    db.Remove(MovementTypeFields);
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Successfully Deleted.", Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadLookup()
        {
            string LookupName = Parameter["LookupName"].ToString().ToLower();
            TableOptions to = Parameter["data"].ToObject<TableOptions>();
            var data = new List<vEmployeeFieldLookup>();

            try
            {
                using (var db = new MovementTypeDb(Session))
                {
                    switch (LookupName)
                    {
                        case "employeemovementfields":
                            data = db.ExecQuery<vEmployeeFieldLookup>("SELECT FieldName, DataType, DisplayName FROM tEmployeeMovementFields ")
                                .OrderBy(c => c.DisplayName)
                                .ToList();
                            break;
                        default:
                            throw new Exception("Method not found.");
                    }

                    return new ReturnSet()
                    {
                        Data = new { Rows = data },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return new ReturnSet()
                {
                    Message = ex.Message,
                    Type = ReturnType.Error
                };
            }
        }


    }
}
