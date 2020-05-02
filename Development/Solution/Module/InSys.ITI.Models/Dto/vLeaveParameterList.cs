using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vLeaveParameterList
    {
        public string Comment { get; set; }
        public int ID { get; set; }
        public int? SeqNo { get; set; }
        public int? ID_CreatedBy { get; set; }
        public int? ID_ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedAt { get; set; }
    }
}
