using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Company
{
    public class Company : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Company(_Session, _Parameter);
        }
        public Company()
        {

        }
        public Company(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }
        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CompanyDB(Session))
                {
                    return new ReturnSet { Data = new { Total = db.tCompany.Count(), Rows = db.tCompany.OrderByDescending(x => x.ID).ToList() }, Type = ReturnType.Result };
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
                var dataID = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new CompanyDB(Session))
                {
                    var company = db.tCompany.Where(x => x.ID == dataID).SingleOrDefault();
                    
                    return new ReturnSet
                    {
                        Data = new
                        {
                            Company = company.IsNull(new tCompany()),
                            Schema = Helpers.GetSchema("tCompany")
                        },
                        Type= ReturnType.Result
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
                var data = Parameter["Data"].ToObject<tCompany>();
                var companyOrgTypes = Parameter["CompanyOrgType"].ToObject<List<tCompanyOrgType>>();
                var jobclassApproverCandidates = Parameter["JobClassApproverCandidates"].ToObject<List<tJobClassApproverCandidates>>();
                var removedJobClassApproverCandidates = Parameter["RemovedJobClassApproverCandidates"].ToObject<List<tJobClassApproverCandidates>>();
                using (var db = new CompanyDB(Session)) {
                    //INSERT/UPDATE Company
                    if (data.ID == 0)
                        db.Add(data);
                    else {
                        var record = db.Single<tCompany>("SELECT * FROM tCompany WHERE ID = {0}",data.ID);
                        if (record != null)
                            db.Update(data);
                    }
                    db.SaveChanges(true);

                    if (companyOrgTypes.Count > 0)
                    {
                        //INSERT COMPANY ORG TYPES
                        foreach (var orgType in companyOrgTypes)
                        {
                            orgType.ID_Company = data.ID;
                            if (orgType.ID == 0)
                                db.Add(orgType);
                            else {
                                var OrgTypeRec = db.Single<tCompanyOrgType>("SELECT * FROM tCompanyOrgType WHERE ID = {0}", orgType.ID);
                                if (OrgTypeRec != null)
                                    db.Update(orgType);
                            }
                        }
						db.SaveChanges(true);
                    }
                    if (jobclassApproverCandidates.Count > 0)
                    {
                        foreach (var x in jobclassApproverCandidates) {
                            x.ID_Org = x.ID;
                            x.ID = 0;
                            var rec = db.Single<tJobClassApproverCandidates>($"SELECT * FROM tJobClassApproverCandidates WHERE ID_Org = {x.ID_Org}");
                            if(rec == null)db.Add(x);
                        }
                        db.SaveChanges();
                    }
                    if (removedJobClassApproverCandidates.Count > 0)
                    {
                        foreach (var x in removedJobClassApproverCandidates)
                        {
                            var record = db.tJobClassApproverCandidates.Where(xx => xx.ID_Org == x.ID).SingleOrDefault();
                            if (record != null) db.Remove(record);
                        }
                        db.SaveChanges();
                    }
                    
                }
                    return new ReturnSet { Data = data.ID,Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet GetChildOrg()
        {
            try
            {
                var id = Parameter["ID"].IsNull(0).ToInt32();
                var IsParentCompany = Parameter["IsCompany"].IsNull(false).ToBool();
                List<vOrg> children = new List<vOrg>();
                using (var db = new CompanyDB(Session))
                {
                    if (IsParentCompany)
                    {
                        children = db.ExecQuery<vOrg>(@"SELECT
                                                        O.ID,
                                                        O.ID_Master,
                                                        o.ID_Parent,
                                                        OML.Name,
                                                        OML2.Name Parent,
                                                        o.ID_CompanyOrgType, 
                                                        COT.ID_OrgType,
                                                        OT.Name OrgType,
                                                        O.ID_Company,
                                                        C.Name Company,
                                                        null Children,
                                                        cast(0 as bit) IsOpen
                                                        FROM tOrg AS O
                                                        INNER JOIN dbo.tCompanyOrgType AS COT ON COT.ID = o.ID_CompanyOrgType
                                                        LEFT JOIN dbo.tOrgMasterList AS OML ON OML.ID = O.ID_Master
                                                        LEFT JOIN dbo.tOrgType AS OT ON OT.ID = COT.ID_OrgType
                                                        LEFT JOIN dbo.tOrg AS O2 ON O2.ID = O.ID_Parent
                                                        LEFT JOIN dbo.tOrgMasterList AS OML2 ON OML2.ID = O2.ID_Master
                                                        LEFT JOIN dbo.tCompany AS C ON c.ID = O.ID_Company
                                                        WHERE O.ID_Company = {0} AND O.ID_Parent IS NULL ORDER BY ID ASC",id).ToList();
                    }
                    else {
                        children = db.ExecQuery<vOrg>(@"SELECT
                                                        O.ID,
                                                        O.ID_Master,
                                                        o.ID_Parent,
                                                        OML.Name,
                                                        OML2.Name Parent,
                                                        o.ID_CompanyOrgType, 
                                                        COT.ID_OrgType,
                                                        OT.Name OrgType,
                                                        O.ID_Company,
                                                        C.Name Company,
                                                        null Children,
                                                        cast(0 as bit) IsOpen
                                                        FROM tOrg AS O
                                                        INNER JOIN dbo.tCompanyOrgType AS COT ON COT.ID = o.ID_CompanyOrgType
                                                        LEFT JOIN dbo.tOrgMasterList AS OML ON OML.ID = O.ID_Master
                                                        LEFT JOIN dbo.tOrgType AS OT ON OT.ID = COT.ID_OrgType
                                                        LEFT JOIN dbo.tOrg AS O2 ON O2.ID = O.ID_Parent
                                                        LEFT JOIN dbo.tOrgMasterList AS OML2 ON OML2.ID = O2.ID_Master
                                                        LEFT JOIN dbo.tCompany AS C ON c.ID = O.ID_Company
                                                        WHERE O.ID_Parent={0} ORDER BY ID ASC", id).ToList();
                    }
                    
                    return new ReturnSet
                    {
                        Data = new
                        {
                            CompanyChildren = children,
                            Schema = Helpers.GetSchema("tOrg")
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
        public ReturnSet LoadCompanyOrg() 
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                var id = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new CompanyDB(Session))
                {
                    var record = db.QueryTable<vCompanyOrgType>(@"(SELECT COT.* ,OT.Name OrgType, C.Name Company
                                                                FROM dbo.tCompanyOrgType AS COT
                                                                LEFT OUTER JOIN tOrgType OT ON OT.ID = COT.ID_OrgType
                                                                LEFT OUTER JOIN tCompany C ON C.ID = COT.ID_Company
                                                                WHERE COT.ID_Company = {0})a", to, id);
                    return new ReturnSet
                    {
                        Data = new { Total = record.Count, Rows = record.Data.ToList() },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet LoadCompanyNewDefaultOrg() {
            try
            {
                var data = Parameter["Data"].ToObject<vOrg>();
                var IsCompany = Parameter["IsCompany"].IsNull(false).ToBool();
                var org = new vOrg();
                using (var db = new CompanyDB(Session))
                {
                    if (IsCompany)
                    {
                        var companyOrgType = db.ExecQuery<vCompanyOrgType>(@"SELECT COT.*,OT.Name OrgType,c.Name Company FROM dbo.tCompanyOrgType AS COT
                                                                             LEFT JOIN dbo.tOrgType AS OT ON OT.ID = COT.ID_OrgType
                                                                             LEFT JOIN dbo.tCompany AS C ON c.ID = COT.ID_Company
                                                                             WHERE COT.ID_Company = {0} 
                                                                             AND COT.SeqNo = (SELECT MIN(COT2.SeqNo) FROM dbo.tCompanyOrgType AS COT2 WHERE COT2.ID_Company = {0})", data.ID).SingleOrDefault();
                        if (companyOrgType == null)
                            throw new Exception("Company doesn't have heirarchy.");

                        org.ID = 0;
                        org.ID_Master = 0;
                        org.Parent = String.Empty;
                        org.ID_Parent = null;
                        org.ID_CompanyOrgType = companyOrgType.ID;
                        org.ID_Company = companyOrgType.ID_Company;
                        org.Company = companyOrgType.Company;
                        org.ID_OrgType = companyOrgType.ID_OrgType;
                        org.Name = String.Empty;
                        org.OrgType = companyOrgType.OrgType;
                    }
                    else {
                        var companyOrgType = db.tCompanyOrgType.Where(x => x.ID == data.ID_CompanyOrgType).SingleOrDefault();

                        if (companyOrgType == null)
                            throw new Exception("Company doesn't have heirarchy.");

                        var companyOrgType2 = db.ExecQuery<vCompanyOrgType>(@"SELECT COT.*,OT.Name OrgType,c.Name Company FROM dbo.tCompanyOrgType AS COT
                                                                         LEFT JOIN dbo.tOrgType AS OT ON OT.ID = COT.ID_OrgType
                                                                         LEFT JOIN dbo.tCompany AS C ON c.ID = COT.ID_Company
                                                                         WHERE COT.ID_Company = {0}
                                                                         AND COT.SeqNo = (SELECT MIN(COT2.SeqNo) FROM dbo.tCompanyOrgType AS COT2 WHERE COT2.ID_Company = {0} AND COT2.SeqNo > {1})"
                                                                         , companyOrgType.ID_Company, companyOrgType.SeqNo).SingleOrDefault();

                        org.ID = 0;
                        org.ID_Master = 0;
                        org.Parent = data.Name;
                        org.ID_Parent = data.ID;
                        org.ID_CompanyOrgType = companyOrgType2.ID;
                        org.ID_Company = companyOrgType2.ID_Company;
                        org.Company = companyOrgType2.Company;
                        org.ID_OrgType = companyOrgType2.ID_OrgType;
                        org.Name = String.Empty;
                        org.OrgType = companyOrgType2.OrgType;
                    }
                        

                    return new ReturnSet
                    {
                        Data = new {
                            Schema = org,
                            TableSchema = Helpers.GetSchema("tOrg")
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
        public override ReturnSet LoadLookup()
        {
            using (var db = new CompanyDB(Session))
            {
                var name = Parameter["Name"].ToString();
                var r = new ReturnSet();
                switch (name) {
                    case "Master":
                        var filter = Parameter["OrgType"].IsNull(0).ToInt32();
                        r = base.LoadLookup<vOrgMasterList>(db, String.Format(@"SELECT OML.*,OT.Name OrgType FROM dbo.tOrgMasterList AS OML
                                                                          LEFT JOIN tOrgType as OT on OT.ID = OML.ID_OrgType
                                                                          WHERE OML.ID_OrgType ={0} and OML.IsActive = 1", filter));
                        break;
                    case "OrgType":
                        r = base.LoadLookup<tOrgType>(db, "SELECT ID,Name,IsActive,ID_UserCreatedBy,DateTimeModified,ID_UserModifiedBy FROM tOrgType WHERE IsActive = 1");
                        break;
                }
                return r;
            }     
        }
        public ReturnSet SaveHeirarchy() {
            try
            {
                var data = Parameter["Data"].ToObject<tOrg>();
                using (var db = new CompanyDB(Session))
                {
                    if (data.ID > 0)
                    {
                        var record = db.tOrg.Where(x => x.ID == data.ID).SingleOrDefault();

                        if (record != null)
                            db.Update(data);
                    }
                    else
                        db.Add(data);

                    db.SaveChanges(true);
                    

                }
                    return new ReturnSet {Data = data.ID,Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet GetLastHeirarchyLevel() {
            try
            {
                var company = Parameter["ID_Company"].IsNull(0).ToInt32();
                using (var db = new CompanyDB(Session))
                {
                    var companyOrgType = db.ExecQuery<tCompanyOrgType>(@"SELECT COT.* FROM dbo.tCompanyOrgType AS COT
                                                                         WHERE COT.ID_Company = {0} 
                                                                         AND COT.SeqNo = (SELECT MAX(COT2.SeqNo) FROM dbo.tCompanyOrgType AS COT2 WHERE COT2.ID_Company = {0})",company).SingleOrDefault();

                    return new ReturnSet
                    {
                        Data = companyOrgType,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet DeleteSelectedHeirarch() {
            try
            {
                var ids = Parameter["Data"].ToObject<List<int>>();
                var deleted = 0;
                using (var db = new CompanyDB(Session))
                {
                    foreach (var id in ids)
                    {
                        var record = db.tOrg.Where(x => x.ID == id).SingleOrDefault();
                        if (record != null)
                        {
                            db.Remove(record);
                            deleted++;
                        }
                    }
                    db.SaveChanges(true);
                    return new ReturnSet
                    {
                        Message = $"{deleted} record(s) has been deleted.",
                        Type = ReturnType.Result
                    };
                }
                    
                
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet SaveCompanyOrgType() {
            try
            {
                var data = Parameter["Data"].ToObject<vCompanyOrgType>();
                tCompanyOrgType cot = new tCompanyOrgType();
                cot.ID = data.ID;
                cot.ID_OrgType = data.ID_OrgType;
                cot.ID_Company = data.ID_Company;
                cot.SeqNo = data.SeqNo;
                using (var db = new CompanyDB(Session))
                {
                    var record = db.tCompanyOrgType.Where(x => x.ID == cot.ID).SingleOrDefault();
                    if (record != null) db.Update(cot);
                    else db.Add(cot);
                    db.SaveChanges(true);

                    data.ID = cot.ID;
                }
                return new ReturnSet { Data= data, Type = ReturnType.Result};

            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }
        public ReturnSet DeleteCompanyOrgLevel()
        {
            try
            {
                var ids = Parameter["Data"].ToObject<List<int>>();
                var deletedCount = 0;
                if (ids.Count > 0)
                {
                    using (var db = new CompanyDB(Session))
                    {
                        foreach (var id in ids)
                        {
                            var record = db.tCompanyOrgType.Where(x => x.ID == id).SingleOrDefault();
                            if (record != null) db.Remove(record);
                            deletedCount++;
                        }
                        db.SaveChanges(true);
                    }
                }
                return new ReturnSet { Message = $"{deletedCount} record(s) deleted.", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet LoadJobClass() {
            try
            {
                var ID = Parameter["ID_Company"].IsNull(0).ToInt32();
                var data = new List<vJobClassApproverCandidates>();
                var jobClassApproverCandidates = new List<tJobClassApproverCandidates>();
                var jobClassParentHeirarchy = new List<fOrg>();
                using (var db = new CompanyDB(Session)) {
                    data = db.ExecQuery<vJobClassApproverCandidates>(@"SELECT
                                                                        O.ID,
                                                                        0 ID_Org,
                                                                        OML.Name
                                                                        FROM tOrg AS O
                                                                        INNER JOIN dbo.tCompanyOrgType AS COT ON COT.ID = o.ID_CompanyOrgType
                                                                        LEFT JOIN dbo.tOrgMasterList AS OML ON OML.ID = O.ID_Master
                                                                        LEFT JOIN dbo.tOrgType AS OT ON OT.ID = COT.ID_OrgType
                                                                        LEFT JOIN dbo.tCompany AS C ON c.ID = O.ID_Company
                                                                        WHERE O.ID_Company = {0} and OT.Name = 'Job Class' ORDER BY ID ASC",ID).ToList();
                    foreach (var d in data)
                    {
                        var t = db.tJobClassApproverCandidates.Where(x => x.ID_Org == d.ID).SingleOrDefault();
                        if (t != null)
                            jobClassApproverCandidates.Add(t);

                        db.ExecQuery<fOrg>($"SELECT *,{d.ID} ID_Org FROM dbo.fGetOrg({d.ID}) ORDER BY SeqNo ASC").ToList().ForEach(x => {
                            jobClassParentHeirarchy.Add(x);
                        });
                    }

                }
                return new ReturnSet 
                {
                    Data = new 
                    {
                        Org = data,
                        JobClassApproverCandidates = jobClassApproverCandidates,
                        JobClassParentHeirarchy = jobClassParentHeirarchy
                    },
                    Type = ReturnType.Result
                };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
    }
}
