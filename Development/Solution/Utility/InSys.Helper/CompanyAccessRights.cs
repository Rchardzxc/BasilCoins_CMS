using InSys.Context;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InSys.Helper
{
    public class CompanyAccessRights
    {
        public CompanyAccessRights() { }
        public static int GetDefaultCompany(int ID_Roles)
        {
            using(var db = new CompanyAccessRightsDb(null))
            {
                if(ID_Roles == 1) return db.ExecScalarInt("Select TOP 1 ID as Value from dbo.tCompany");
                else return db.ExecScalarInt("Select TOP 1 ID_Company as Value from dbo.tCompanyAccessRights where ID_Roles = {0}", ID_Roles);
            }
        }
    }
    public class CompanyAccessRightsDb: InSysContext
    {
        public CompanyAccessRightsDb(BrowserSession Session) : base(Session)
        {
        }

        public DbQuery<IntReturn> intReturns { get; set; }
    }
}
