using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Common
{
    public static class StringHelper
    {
        public static string ComposedRowMsg(string Msg, int RowNumber) {
            return "Row " + RowNumber.ToString() + "-" + Msg;
        }
    }
}
