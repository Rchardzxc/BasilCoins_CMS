using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ReportGenerator.Classes
{
    public class ReturnSet
    {
        public object Data { get; set; }
        public string Message { get; set; }
        public ReturnType Type { get; set; }
    }
    public enum ReturnType
    {
        Result = 1,
        Error = 2,
        ChangePassword = 3,
        FirstLogin = 4,
        WrongPassword = 5,
        Blocked = 6,
        ValidationError = 7,
        PageNotFound = 8
    }
}
