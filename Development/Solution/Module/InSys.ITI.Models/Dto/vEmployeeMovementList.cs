using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vEmployeeMovementList
    {
        public int ID { get; set; }
        public string RefNumber { get; set; }
        public string EmployeeName { get; set; }
        public string MovementType { get; set; }
        public DateTime EffectivityDate { get; set; }
        public string FilingStatus { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Remarks { get; set; }
        public bool Posted { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
