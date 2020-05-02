using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.RecruitmentReport
{
    public class RecruitmentReport: BaseModule
    {

        private int? System_ID_Company { get; set; }
        private int? ID_User { get; set; }
        private int? ID_Company { get; set; }
        private int? ID_Employee { get; set; }

        private int? ID_Role { get; set; }

        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new RecruitmentReport(_Session, _Parameter);
        }

        public RecruitmentReport(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
            this.ID_Company = Session.ID_Company;
            this.ID_Employee = Session.ID_Employee;
            this.ID_User = Session.ID_User;
            this.ID_Role = Session.ID_Roles;
            this.System_ID_Company = ID_Company;
        }

        public RecruitmentReport() {
             //Helpers.IIf(ID_Company == 0, OrganizationDb.CompanyRights(ID_Role), ID_Company);
        }

        public override ReturnSet LoadList()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new RecruitmentReportDb(Session))
                {
                    var fs = to.Filters.Where(x => x.Name == "ID_Company").FirstOrDefault();
                    if (fs == null)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Company",
                            Type = FilterType.IN,
                            Value = System_ID_Company
                        });
                    //else if (fs.Value == null)
                    //{
                    //    fs.Type = FilterType.IN;
                    //    fs.Value = Helpers.IIf(ID_Company == 0,  Organi (ID_Role, ID_Company);
                    //}

                    var MRFStatusData = db.QueryTable<vKioskMRFStatus>(Helpers.GetSqlQuery("vRecruitmentStatus"), to);
                    return new ReturnSet() { Data = new { Total = MRFStatusData.Count, Rows = MRFStatusData.Data.ToList() }, Type = ReturnType.Result };
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
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                int ID_MRF = Parameter["ID"].ToObject<int>();

                using (var db = new RecruitmentReportDb(Session))
                {
                    var MRFStatusDetails = db.QueryTable<vKioskMRFStatusDetails>("(SELECT taf.ID, taf.RefNum, taf.LastName"
                                        + "     , taf.FirstName, taf.MiddleName, taf.ID_RecruitmentStatus, tkrs.Name AS ApplicationStatus"
                                        + " , taf.ID_RecruitmentStatus AS c_ID_RecruitmentStatus FROM tApplicationForm taf"
                                        + "     INNER JOIN tRecruitmentStatus tkrs ON tkrs.ID = taf.ID_RecruitmentStatus"
                                        + " WHERE ISNULL(taf.ID_MRF, 0) = {0})a", to, ID_MRF);


                    var MRFStatusDetailCount = db.QueryTable<vKioskMRFStatusDetailCount>("(SELECT COUNT(taf.ID) AS TotalCount"
                                        + " , tkrs.Name AS ApplicationStatus"
                                        + " FROM tApplicationForm taf"
                                        + "     INNER JOIN tRecruitmentStatus tkrs ON tkrs.ID = taf.ID_RecruitmentStatus"
                                        + " WHERE ISNULL(taf.ID_MRF, 0) = {0} GROUP BY tkrs.Name)a", to, ID_MRF);

                    return new ReturnSet() { Data = new { Total = MRFStatusDetails.Count, Rows = MRFStatusDetails.Data.ToList(), RowCount = MRFStatusDetailCount.Data.ToList() }, Type = ReturnType.Result };
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
                var MRFStatusDetails = Parameter["Data"].ToObject<List<vKioskMRFStatusDetails>>();

                using (var db = new RecruitmentReportDb(Session))
                {
                    foreach (var data in MRFStatusDetails)
                    {
                        if (data.ID_RecruitmentStatus != data.c_ID_RecruitmentStatus)
                        {
                            var UpdateData = db.Single<tApplicationForm>("SELECT * FROM tApplicationForm WHERE ID = {0}", data.ID);
                            UpdateData.ID_RecruitmentStatus = data.ID_RecruitmentStatus;
                            db.Update(UpdateData);
                        }
                    }
                    db.SaveChanges();
                }

                return new ReturnSet() { Message = "Successfully Message", Type = ReturnType.Result };

            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet DeleteRecord()
        {
            throw new NotImplementedException();
        }
    }
}
