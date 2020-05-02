using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace InSys.ITI.Models.Dto
{
    [NotMapped]
    public class vFilingModules
    {
        public int ID { get; set; }
        public int ID_Menus { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
