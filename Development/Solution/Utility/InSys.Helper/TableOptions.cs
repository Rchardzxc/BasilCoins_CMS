using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Helper
{
    /// <summary>
    /// This is used to save the value of Sort Name, Sort Direction (ASC, DESC), Page, Count and
    /// all the filters that will be used. Normally it is used in Listing/View Page of the system.
    /// </summary>
    public class TableOptions : ICloneable
    {
        public string SortName { get; set; }
        public string SortDirection { get; set; }
        public int? Page { get; set; }
        public int? Count { get; set; }
        public List<FilterSchema> Filters { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
