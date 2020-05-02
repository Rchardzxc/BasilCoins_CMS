using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Context
{
    public class KioskContext:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ContextConfig.KioskConnection != null) optionsBuilder.UseSqlServer(ContextConfig.KioskConnection);
            else throw new Exception("No database connection defined.");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
