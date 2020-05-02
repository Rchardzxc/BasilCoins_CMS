using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using z.Data;

namespace InSys.Helper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InvariantCulture : Attribute
    {
        public InvariantCulture() : base()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }
    }
}
