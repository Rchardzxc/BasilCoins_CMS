using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vEmployeeMovementFormDetail
    {
        public int? ID { get; set; }
        public int? ID_EmployeeMovement { get; set; }
        public string DataType { get; set; }
        public string TargetTable { get; set; }
        public string LookUpTable { get; set; }
        public string LookUpName { get; set; }
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public string OldValue { get; set; }
        public string DisplayOldValue { get; set; }
        public string NewValue { get; set; }
        public string DisplayNewValue { get; set; }
    }
}
