using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using z.Data;

namespace InSys.ITI.TaxExemption
{
    public class TaxExemption : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            throw new NotImplementedException();
        }
        public TaxExemption() { }

        public TaxExemption(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public override ReturnSet LoadList()
        {
            return base.LoadList();
        }
        public override ReturnSet LoadForm()
        {
            return base.LoadForm();
        }
        public override ReturnSet Save()
        {
            return base.Save();
        }
        public override ReturnSet DeleteRecord()
        {
            return base.DeleteRecord();
        }
    }
}
