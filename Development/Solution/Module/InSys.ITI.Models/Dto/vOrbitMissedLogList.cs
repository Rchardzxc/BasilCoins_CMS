using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vOrbitMissedLogList
    {
        public int ID { get; set; }
        public DateTime WorkDate { get; set; }
        public bool IsPosted { get; set; }
        public string RefNum { get; set; }
        public string Employee { get; set; }
        public string FilingStatus { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Reasons { get; set; }
    }
}
