using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InSys.ITI.Menu
{
    public class MenuCollection
    {
        public MenuCollection() { }
        public static List<MenuGroup> SystemMenu { get; set; }
        public static List<tMenus> AllMenu { get; set; } = new List<tMenus>();
        public static List<tCompany> CompanyCollection { get; set; } = new List<tCompany>();
        public static void Init()
        {
            using (var db = new MenuDb(null))
            {
                SystemMenu = new List<MenuGroup>();
                CompanyCollection = db.ExecQuery<tCompany>("Select * from dbo.tCompany").ToList();
                AllMenu = db.ExecQuery<tMenus>("select * from dbo.tMenus where IsVisible = 1 ORDER BY ID_MenusType, Code").ToList();
                foreach (var company in CompanyCollection)
                {
                    SystemMenu.Add(new MenuGroup() {
                        Company = company,
                        Menus = AllMenu.Where(x => x.ID_Company == company.ID).Select(x => new Menu
                        {
                            Code = x.Code,
                            ID = x.Id,
                            ID_Menus = x.ID_Menus,
                            Name = x.Name,
                            SequenceNumber = x.SequenceNumber,
                            ID_MenusType = x.ID_MenusType,
                            IsVisible = x.IsVisible
                        }).ToList()
                });
                }
            }
        }
    }
    public class MenuGroup {
        public tCompany Company { get; set; }
        public List<Menu> Menus { get; set; }
    }
    public class Menu
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? ID_Menus { get; set; }
        public int? SequenceNumber { get; set; }
        public int ID_MenusType { get; set; } = 1;
        public bool IsVisible { get; set; } = true;
    }
    public class MenuPrivileges
    {
        public bool HasView { get; set; } = false;
        public bool HasNew { get; set; } = false;
        public bool HasEdit { get; set; } = false;
        public bool HasDelete { get; set; } = false;
        public bool PostJob { get; set; } = false;
        public bool IsApprover { get; set; } = false;
        public int ID_Level { get; set; } = 1;
        public bool IsPowerApprover { get; set; } = false;
    }
    public class MenuDb : InSysContext
    {
        public MenuDb(BrowserSession Session) : base(Session)
        {
        }

        public DbSet<tMenus> tMenus { get; set; }
        public DbSet<tCompany> tCompanies { get; set; }
        public DbSet<tRoleAccessRights> tRoleAccessRights { get; set; }
        public DbSet<tApprover_Employee_Module_Approvers> tApprover_Employee_Module_Approvers { get; set; }
    }
}
