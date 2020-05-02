using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    public class vMovementTypeList
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
