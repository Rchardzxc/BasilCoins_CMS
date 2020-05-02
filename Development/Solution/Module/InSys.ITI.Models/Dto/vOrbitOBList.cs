using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vOrbitOBList
    {
        public int Id { get; set; }
        public DateTime FiledDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsPosted { get; set; }
        public decimal ComputedHours { get; set; }
        public decimal ConsideredHours { get; set; }
        public string RefNum { get; set; }
        public string Employee { get; set; }
        public string FilingStatus { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Reasons { get; set; }
    }
}
