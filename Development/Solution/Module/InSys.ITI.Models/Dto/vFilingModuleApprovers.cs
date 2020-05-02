using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vFilingModuleApprovers
    {
        public int? ID { get; set; }
        public int? ID_Approver { get; set; }
        public string Approver { get; set; }
        public int? ID_FilingModules { get; set; }
        public int? ID_Menus { get; set; }
        public string Menus { get; set; }
        public int? ID_Employee { get; set; }
        public string Employee { get; set; }
        public int? ID_Employee2 { get; set; }
        public string Employee2 { get; set; }
        public int? ID_Employee3 { get; set; }
        public string Employee3 { get; set; }
        public int? ID_Level { get; set; }
        public bool? IsPowerApprover { get; set; }
        public bool? IsActive { get; set; }
    }
}
