using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Models.Common
{
    public class AuditEntity
    {
        public int ID { get; set; }
        public int ID_CreatedBy { get; set; }
        public int? ID_ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
