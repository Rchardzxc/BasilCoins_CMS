using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vApprovalHistory
    {
        public int ID { get; set; }
        //public int ID_Menus { get; set; } This was changed by MenuCode
        public int ID_Reference { get; set; }
        public int ID_Employee { get; set; }
        public string ApproverComment { get; set; }
        public string ApprovalAction { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public int ApprovalLevel { get; set; } = 1;
        public int ID_TargetEmployee { get; set; }
        public string Approver { get; set; }
        public string Employee { get; set; }
    }
}
