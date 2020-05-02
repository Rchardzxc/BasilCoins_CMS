using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vOrbitOvertimeList
    {
        public int ID { get; set; }
        public int? ID_Employee { get; set; }
        public int? ID_FilingStatus { get; set; }
        public int? ID_WorkCredit { get; set; }
        public int? ID_ApprovedBy { get; set; }
        public int? StartMinute { get; set; }
        public int? EndMinute { get; set; }
        public int? CurrentApproverLevel { get; set; }
        public int? ID_CreatedBy { get; set; }
        public int? ID_ModifiedBy { get; set; }
        public int? ID_Company { get; set; }
        public DateTime? FilingDate { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool? IsPosted { get; set; }
        public decimal? ComputedHours { get; set; }
        public decimal? ConsideredHours { get; set; }
        public string ReferenceNo { get; set; }
        public string Reason { get; set; }
        public string ApproverComment { get; set; }
        public string Attachment { get; set; }
        public string Employee { get; set; }
        public string FilingStatus { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ApprovedBy { get; set; }
    }
}
