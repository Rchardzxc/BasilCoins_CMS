using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Common
{
    public static class DateTimeHelper
    {
        public static int ConvertDateTimeToMinutes(DateTime dt) {
            return (dt.Hour * 60) + dt.Minute;
        }
    }
}
