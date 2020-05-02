using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vEmployeeRecordList
    {
        public int ID { get; set; }
        public int ID_Company { get; set; }
        public int ID_EmployeeStatus { get; set; }
        public int? ID_PayrollScheme { get; set; }
        public int? ID_PayrollFrequency { get; set; }
        public int? ID_Parameter { get; set; }
        public int? ID_JobClass { get; set; }
        public int? ID_Designation { get; set; }
        public int? ID_LeaveParameter { get; set; }
        public int? ID_Branch { get; set; }
        public int? ID_Division { get; set; }
        public int? ID_Department { get; set; }
        public int? ID_Section { get; set; }
        public int? ID_JobClassGroup { get; set; }
        public string Code { get; set; }
        public  string Name { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string EmployeeStatus { get; set; }
        public string PayrollScheme { get; set; }
        public string Section { get; set; }
        public string JobClassGroup { get; set; }
        public string JobClass { get; set; }
        public string Designation { get; set; }
        public string PayrollFrequency { get; set; }
        public string Parameter { get; set; }
        public string LeaveParameter { get; set; }
        public string Branch { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
    }
}
