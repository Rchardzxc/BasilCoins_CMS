using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.MovementType
{
    public class MovementTypeDb : InSysContext
    {
        public MovementTypeDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tMovementType> tMovementType { get; set; }
        public DbSet<tMovementTypeFields> tMovementTypeFields { get; set; }
        public DbQuery<vEmployeeFieldLookup> vEmployeeFieldLookup { get; set; }
        public DbQuery<vMovementTypeList> vMovementTypeList { get; set; }
    }
}
