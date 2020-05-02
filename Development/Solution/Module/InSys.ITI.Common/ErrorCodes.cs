using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Common
{
    public static class ErrorCodes
    {
        public static string GetError(string Code) {
            string ErrorMessage = "";
            switch (Code.ToLower()) {
                case "duplicatetin": {
                        ErrorMessage = "Duplicate TIN";
                        break;
                    }
                default:
                    break;
            }

            return ErrorMessage;
        }
    }
}
