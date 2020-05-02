using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using z.Data;

namespace InSys.ITI.Controller
{
    public class CoreParameters
    {
        private Pair Parameter { get; set; }
        private BrowserSession Session { get; set; }

        DbContext context { get; set; }

        public void SetParameter(Pair _Parameter) {
            this.Parameter = _Parameter;
        }

        public void SetSession(BrowserSession _BrowserSession) {
            this.Session = _BrowserSession;
        }
    }
}
