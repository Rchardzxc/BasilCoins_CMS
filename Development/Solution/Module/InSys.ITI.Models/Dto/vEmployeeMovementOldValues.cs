using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vEmployeeMovementOldValues
    {
        public string OldValue { get; set; }
        public string DisplayOldValue { get; set; }
    }
}
