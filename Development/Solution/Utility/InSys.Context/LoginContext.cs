using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Context
{
    public class LoginContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ContextConfig.SQLConnection != null) optionsBuilder.UseSqlServer(ContextConfig.SQLConnection);
            else throw new Exception("No database connection defined.");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
