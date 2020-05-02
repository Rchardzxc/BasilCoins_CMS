using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vEmployeeFieldLookup
    {
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public string DisplayName { get; set; }
    }
}
