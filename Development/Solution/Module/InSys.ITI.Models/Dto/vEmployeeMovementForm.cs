using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vEmployeeMovementForm
    {
        public int ID { get; set; }
        public int ID_Company { get; set; }
        public int ID_Employee { get; set; }
        public int ID_MovementType { get; set; }
        public int ID_FilingStatus { get; set; }
        public int? ID_ApprovedBy { get; set; }
        public int ID_CreatedBy { get; set; }
        public int? ID_ModifiedBy { get; set; }
        public bool Posted { get; set; }
        public string RefNumber { get; set; }
        public string EmployeeName { get; set; }
        public string MovementType { get; set; }
        public string FilingStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Remarks { get; set; }
        public DateTime? EffectivityDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
