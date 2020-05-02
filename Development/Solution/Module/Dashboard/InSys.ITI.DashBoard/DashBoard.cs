using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Linq;
using z.Data;

namespace InSys.ITI.DashBoard
{
    public class DashBoard : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new DashBoard(_Session, _Parameter);
        }
        public DashBoard() { }
        public DashBoard(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public override ReturnSet LoadForm() {
            try {
                using (var db = new DashBoardDb(Session)) {
                    int CurrentYear = DateTime.Now.Year;
                    string model = "InSys.ITI.Models";
                    //Announcement
                    var Announcement = db.ExecQuery<vAnnouncements>(@"SELECT a.ID, a.Subject, a.Description, a.Attachment, u.EmployeeName AS CreatedBy, 'Software Engineer' AS Designation
                                                                    FROM tAnnouncements a
	                                                                    INNER JOIN vUserEmployee u ON u.ID = a.ID_CreatedBy
                                                                    WHERE CONVERT(DATE, GETDATE(), 101) BETWEEN ShowOn AND ISNULL(ShowUntil, CONVERT(DATE, GETDATE(), 101))").ToList();
                    //Upcomming Events
                    var UpCommingEvent = db.ExecQuery<vUpcommingEvents>(@"SELECT ue.ID, ue.Title, ue.Description, ue.EffectiveDate, ue.TimeStart, ue.TimeEnd, ue.Attachment,
	                                                                        m.Code AS [Month], DAY(EffectiveDate) AS [Day]
                                                                        FROM tUpcommingEvents ue
	                                                                        INNER JOIN tMonth m ON m.ID = MONTH(EffectiveDate)
                                                                        WHERE CONVERT(DATE, GETDATE(), 101) BETWEEN ShowOn AND ISNULL(ShowUntil, CONVERT(DATE, GETDATE(), 101))").ToList();
                    //Show Leave Details
                    var LeaveDetails = db.ExecQuery<vEmployeeLeaveDetails>(Helpers.GetSqlQuery("vEmployeeLeaveDetails", model), Session.ID_Company, Session.ID_Employee, CurrentYear).ToList();
                    
                    //Show TimeSummary Data not yet properly defined
                    var TimeSummary = db.ExecQuery<vEmployeeTimeKeepingSummary>(Helpers.GetSqlQuery("vEmployeeTimeKeepingSummary", model), Session.ID_Employee).ToList();

                    return new ReturnSet() { Data = new { 
                                                AnnouncementData = Announcement
                                                , UpcommingEventData = UpCommingEvent
                                                , LeaveDetailsData = LeaveDetails
                                                , TimeSummaryData = TimeSummary }
                    };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }


        public override ReturnSet LoadList() {
            try {
                var FilingType = Parameter["FilingType"].ToString();
                var resultData = new ReturnSet();
                switch (FilingType) {
                    case ("LEAVE"):
                        Leave LeaveData = new Leave(Session, Parameter);
                        resultData = LeaveData.LoadList();
                        break;
                    case ("OT"):
                        resultData = new ReturnSet() { Message = "No Overtime Yet", Type = ReturnType.Result };
                        break;
                    case ("OB"):
                        OB OBData = new OB(Session, Parameter);
                        resultData = OBData.LoadList();
                        break;
                    case ("ML"):
                        MissedLog MissedLogData = new MissedLog(Session, Parameter);
                        resultData = MissedLogData.LoadList();
                        break;
                    case ("COS"):
                        resultData = new ReturnSet() { Message = "No Change Of Schedule Yet", Type = ReturnType.Result };
                        break;
                }

                return resultData;
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        //Load Filing Forms
        public override ReturnSet LoadFilingForm() {
            try {
                var FilingType = Parameter["FilingType"].ToString().ToLower();
                ReturnSet data = new ReturnSet();
                using (var db = new LeaveDb(Session)) {
                    switch (FilingType)
                    {
                        case ("leave"):
                            Leave LeaveData = new Leave(Session, Parameter);
                            data = LeaveData.LoadForm();
                            break;
                        case ("ot"):
                            break;
                        case ("ob"):
                            OB OBData = new OB(Session, Parameter);
                            data = OBData.LoadForm();
                            break;
                        case ("ml"):
                            MissedLog MLData = new MissedLog(Session, Parameter);
                            data = MLData.LoadForm();
                            break;
                        case ("cos"):
                            break;
                    }
                }
                return data;
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);            
            }
        
        
        } 
    }
}
