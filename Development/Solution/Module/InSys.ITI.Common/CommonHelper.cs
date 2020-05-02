using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InSys.ITI.Common
{
    public static class CommonHelper
    {
        /// <summary>
        /// Get Lookup data, normally used for dropdown data. The fields are Id, Name and IsActive
        /// </summary>
        /// <param name="LookUpName"></param>
        /// <param name="to"></param>
        /// <param name="Session"></param>
        /// <returns></returns>
        public static ReturnSet GetLookUp(string LookUpName, TableOptions to, BrowserSession Session) {
            var data = new List<tLookUpData>();
            string SqlCommandText = "";
            try {
                using (var db = new CommonHelperDb(Session))
                {
                    switch (LookUpName)
                    {
                        case "employee":
                            SqlCommandText = String.Format("(SELECT ID_Employee AS ID, EmployeeName AS [Name]," +
                                "   CAST(1 AS BIT) AS IsActive FROM vEmployees WHERE ID_Company = {0})a", Session.ID_Company);
                            break;
                        case "employeesuggestion":
                        case "movementtype":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM dbo.tMovementType)a";
                            break;
                        case "employeemovementfields":
                            SqlCommandText = "(SELECT Id, DisplayName AS Name, CAST(1 AS BIT) AS IsActive " +
                                "FROM tEmployeeMovementFields)a";
                            break;
                        case "filingstatus":
                            SqlCommandText = "(SELECT ID, Name, CAST(1 AS BIT) AS IsActive " +
                                "FROM dbo.tFilingStatus WHERE (ID <> 4))a";
                            break;
                        case "branch":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tBranch)a";
                            break;
                        case "accountnumbertype":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tAccountNumberType)a";
                            break;
                        case "company":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tCompany)a";
                            break;
                        case "currency":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tCurrency)a";
                            break;
                        case "department":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tDepartment)a";
                            break;
                        case "designation":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tDesignation)a";
                            break;
                        case "division":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tDivision)a";
                            break;
                        case "jobclass":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tJobClass)a";
                            break;
                        case "jobclassgroup":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tJobClassGroup)a";
                            break;
                        case "leaveparameter":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tLeaveParameter)a";
                            break;
                        case "parameter":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tParameter)a";
                            break;
                        case "paymentmode":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tPaymentMode)a";
                            break;
                        case "payrollclassification":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tPayrollClassification)a";
                            break;
                        case "payrollfrequency":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tPayrollFrequency)a";
                            break;
                        case "payrollstatus":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tPayrollStatus)a";
                            break;
                        case "section":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tSection)a";
                            break;
                        case "bloodtype":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tBloodType)a";
                            break;
                        case "citizenship":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tCitizenship)a";
                            break;
                        case "civilstatus":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tCivilStatus)a";
                            break;
                        case "gender":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tGender)a";
                            break;
                        case "nationality":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tNationality)a";
                            break;
                        case "religion":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tReligion)a";
                            break;
                        case "sssstatus":
                            SqlCommandText = "(SELECT ID, Name, CAST(1 AS BIT) AS IsActive FROM tSSSStatus)a";
	                        break;
                        case "employeestatus":
                            SqlCommandText = "(SELECT Id, Name, IsActive FROM tEmployeeStatus)a";
                            break;
                        case "leavetype":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tPayrollItem WHERE IsForLeave = 1)a";
                            break;
                        case "roles":
                            SqlCommandText = "(SELECT ID, Name, CAST(1 AS BIT) AS IsActive FROM tRoles)a";
                            break;
                        case "weeklyschedule":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tWeeklySchedule)a";
                            break;
                        case "workcredit":
                            SqlCommandText = "(SELECT ID, Name, IsActive FROM tWorkCredit)a";
                            break;
                        default:
                            throw new Exception("Method not found.");
                    }


                    data = db.QueryTable<tLookUpData>(SqlCommandText, to).Data.ToList();

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


        public static ReturnSet GetApprovalHistory(string MenuCode, int ID_Reference, BrowserSession Session) {
            try {
                using (var db = new CommonHelperDb(Session)) {
                    var data = db.ExecQuery<vApprovalHistory>("SELECT * FROM dbo.fApprovalHistory({0}, {1})", MenuCode, ID_Reference).ToList();

                    return new ReturnSet()
                    {
                        Data = data,
                        Type = ReturnType.Result
                    };
                }
            } catch (Exception ex) {
                return new ReturnSet()
                {
                    Message = ex.Message,
                    Type = ReturnType.Error
                };
            }
        }

        public static ReturnSet PostApprovalHistory(tApprovalHistory data, BrowserSession session)
        {
            try
            {
                using (var db = new CommonHelperDb(session))
                {
                    db.Add(data);
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Successfully Posted.", Type = ReturnType.Result };
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

        //public static ReturnSet PostApprovalHistory(tApprovalHistory data, BrowserSession session)
        //{
        //    try
        //    {
        //        using (var db = new CommonHelperDb(session)) {
        //            db.Add(data);
        //            db.SaveChanges();

        //        }

        //        return new ReturnSet() { Message = "Successfully Posted.", Type = ReturnType.Result };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ReturnSet()
        //        {
        //            Message = ex.Message,
        //            Type = ReturnType.Error
        //        };
        //    }

        //}


    }
}
