using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    public class vEmployeeMovementField
    {
        public string SourceTable { get; set; }
        public string DataType { get; set; }
        public int IsRequired { get; set; }
        public int Length { get; set; }
        public string FieldName { get; set; }
    }
}
