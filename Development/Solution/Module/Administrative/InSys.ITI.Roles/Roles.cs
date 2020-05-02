using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Roles
{
    public class Roles : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Roles(_Session, _Parameter);
        }
        public Roles(BrowserSession _Session, Pair _Parameter)
        {
            Session = _Session;
            Parameter = _Parameter;
        }
        public Roles() { }

        public override ReturnSet Save()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new RolesDb(Session))
                {
                    tRoles tr = Parameter["Data"].ToObject<tRoles>();
                    List<vRoleAccessRights> rar = Parameter["Menus"].ToObject<List<vRoleAccessRights>>();
                    List<vCompanyAccessRights> car = Parameter["Companies"].ToObject<List<vCompanyAccessRights>>();

                    if (tr.ID == 1) return new ReturnSet() { Data = tr.ID, Type = ReturnType.Result };

                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //Validate, User should have at least one access to company.
                            if (car.Where(x => x.IsChecked == true).ToList().Count == 0 && !db.Any("(SELECT Id AS Value FROM tCompanyAccessRights WHERE ID_Roles = {0})a", tr.ID))
                                throw new Exception("Role should have at least one company access (Companies Tab).");

                            if (db.Any("(select * from dbo.tRoles where ID = {0})a", tr.ID)) db.Update(tr);
                            else db.Add(tr);
                            db.SaveChanges(true);

                            if (rar.Count > 0)
                            {
                                foreach (var rar2 in rar)
                                {
                                    var roleAccRights = new tRoleAccessRights()
                                    {
                                        ID = rar2.ID,
                                        ID_Menus = rar2.ID_Menus,
                                        ID_Roles = rar2.ID_Roles
                                    };
                                    if (!rar2.IsChecked)
                                    {
                                        if (db.Any("(select * from dbo.tRoleAccessRights where ID = {0})a", roleAccRights.ID))
                                        {
                                            if (rar2.Children.Count > 0)
                                            {
                                                SaveChildRoleAccessRights(rar2.Children, db, tr.ID);
                                            }
                                            db.Remove(roleAccRights);
                                        }
                                    }
                                    else
                                    {
                                        var menu = db.Single<tMenus>("select * from dbo.tMenus where Id = {0}", rar2.ID_Menus);
                                        rar2.AccessRights.HasView = true;
                                        roleAccRights.AccessRights = EncryptionHandler.EncryptRights(rar2.AccessRights, menu);
                                        roleAccRights.ID_Roles = tr.ID;

                                        if (db.Any("(select * from dbo.tRoleAccessRights where ID = {0})a", roleAccRights.ID)) db.Update(roleAccRights);
                                        else db.Add(roleAccRights);

                                        if (rar2.Children.Count > 0)
                                        {
                                            SaveChildRoleAccessRights(rar2.Children, db, tr.ID);
                                        }
                                    }
                                }
                            }

                            if (car.Count > 0)
                            {
                                foreach (var car2 in car)
                                {
                                    var companyAccRights = new tCompanyAccessRights()
                                    {
                                        ID_Roles = car2.ID_Roles,
                                        ID = car2.ID,
                                        ID_Company = car2.ID_Company
                                    };
                                    if (!car2.IsChecked)
                                    {
                                        if (db.Any("(select * from dbo.tCompanyAccessRights where ID = {0})a", companyAccRights.ID)) db.Remove(companyAccRights);
                                    }
                                    else
                                    {
                                        companyAccRights.ID_Roles = tr.ID;
                                        if (db.Any("(select * from dbo.tCompanyAccessRights where ID = {0})a", companyAccRights.ID)) db.Update(companyAccRights);
                                        else db.Add(companyAccRights);
                                    }
                                }
                            }
                            db.SaveChanges();
                            tran.Commit();
                            return new ReturnSet() { Data = tr.ID, Type = ReturnType.Result };
                        }
                        catch (Exception exx)
                        {
                            tran.Rollback();
                            string message = (exx.InnerException ?? exx).Message;
                            Logger.LogError(ref message, "SaveForm", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                            r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                            return r;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveForm", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new RolesDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tRoles>("select * from dbo.tRoles where ID = {0} and ID > 1", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteRecord", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new RolesDb(Session))
                {
                    var data = db.Single<tRoles>("select * from dbo.tRoles where ID = {0} and ID > 1", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tRoles()),
                            Schema = Helpers.GetSchema("tRoles")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet LoadList()
        {
            string message = "";
            ReturnSet res = new ReturnSet();

            TableOptions to = Parameter["Data"].ToObject<TableOptions>();

            try
            {
                using (var db = new RolesDb(Session))
                {
                    var data = db.QueryTable<tRoles>("(select * from dbo.tRoles where ID > 1)a", to);
                    res = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRoles", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                res = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }

            return res;
        }
        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "rolesmenu":
                    return LoadRolesMenu();
                case "rolescompany":
                    return LoadRolesCompany();
                default:
                    throw new Exception("Method not found.");
            }
        }
        public virtual ReturnSet LoadRolesMenu()
        {
            var r = new ReturnSet();
            int ID = Parameter["ID"].IsNull(0).ToInt32();
            try
            {
                using (var db = new RolesDb(Session))
                {
                    var data = new List<vRoleAccessRights>();
                    var data2 = new List<vRoleAccessRights>();

                    data = db.ExecQuery<vRoleAccessRights>("select trar.AccessRights EncodedRights, trar.ID_Roles, trar.ID_Menus, tm.Code, tm.Name Menus, trar.ID, tm.ID_Menus ID_Menus_Parent, tr.Name Roles, cast(1 as bit) IsChecked, tm.ID_MenusType from dbo.tRoleAccessRights trar" +
                        " inner join dbo.tMenus tm on trar.ID_Menus = tm.Id and tm.IsSystem = 0" +
                        " inner join dbo.tRoles tr on trar.ID_Roles = tr.ID" +
                        " where trar.ID_Roles = {0} and tm.IsVisible = 1", ID)
                        .Map(x =>
                        {
                            x.AccessRights = new AccessRights()
                            {
                                HasDelete = EncryptionHandler.GetRights("delete", x.EncodedRights),
                                HasEdit = EncryptionHandler.GetRights("edit", x.EncodedRights),
                                HasNew = EncryptionHandler.GetRights("new", x.EncodedRights),
                                HasView = EncryptionHandler.GetRights("view", x.EncodedRights),
                                PostJob = EncryptionHandler.GetRights("postjob", x.EncodedRights)
                            };
                            return x;
                        }).ToList();
                    var to = new TableOptions() { Filters = new List<FilterSchema>() };
                    if (data.Count > 0)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Menus",
                            Type = FilterType.NotIN,
                            Value = data.ToListArray("ID_Menus")
                        });
                    data2 = db.QueryTable<vRoleAccessRights>("(select 0 ID, 0 ID_Roles, null Roles, tm.Id ID_Menus, tm.Name Menus, tm.Code, tm.ID_Menus ID_Menus_Parent, cast(0 as bit) IsChecked, null EncodedRights, tm.ID_MenusType from dbo.tMenus tm where tm.IsSystem = 0 and tm.IsVisible = 1 and tm.ID_Company = {0})a", to, Session.ID_Company)
                        .Data
                        .Map(x => {
                            x.AccessRights = new AccessRights();
                            x.Children = new List<vRoleAccessRights>();
                            return x;
                        })
                        .ToList();
                    data.AddRange(data2);
                    return new ReturnSet()
                    {
                        Data = data,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRolesMenu", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadRolesCompany()
        {
            var r = new ReturnSet();
            int ID = Parameter["ID"].IsNull(0).ToInt32();
            try
            {
                using (var db = new RolesDb(Session))
                {
                    var data2 = new List<vCompanyAccessRights>();
                    var data = db.ExecQuery<vCompanyAccessRights>("select tc.Name Company, tcar.ID, tcar.ID_Company, tcar.ID_Roles, tc.Name, tr.Name Roles, null Children, CAST(1 as BIT) IsChecked from dbo.tCompanyAccessRights tcar" +
                        " left join dbo.tCompany tc on tcar.ID_Company = tc.ID" +
                        " left join dbo.tRoles tr on tcar.ID_Roles = tr.ID" +
                        " where tcar.ID_Roles = {0}", ID).ToList();
                    var to = new TableOptions() { Filters = new List<FilterSchema>() };
                    if (data.Count > 0)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Company",
                            Type = FilterType.NotIN,
                            Value = data.ToListArray("ID_Company")
                        });
                    data2 = db.QueryTable<vCompanyAccessRights>("(select 0 ID, tc.ID ID_Company, 0 ID_Roles, tc.Name Company, null Roles, null Children, cast(0 as bit) IsChecked from dbo.tCompany tc)a", to).Data.ToList();
                    data.AddRange(data2);
                    return new ReturnSet()
                    {
                        Data = data,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRolesCompany", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public void SaveChildRoleAccessRights(List<vRoleAccessRights> rar, InSysContext db, int ID)
        {
            foreach (var rar2 in rar)
            {
                var roleAccRights = new tRoleAccessRights()
                {
                    ID = rar2.ID,
                    ID_Menus = rar2.ID_Menus,
                    ID_Roles = rar2.ID_Roles
                };
                if (!rar2.IsChecked)
                {
                    if (db.Any("(select * from dbo.tRoleAccessRights where ID = {0})a", roleAccRights.ID))
                    {
                        if (rar2.Children.Count > 0)
                        {
                            SaveChildRoleAccessRights(rar2.Children, db, ID);
                        }
                        db.Remove(roleAccRights);
                    }
                }
                else
                {
                    var menu = db.Single<tMenus>("select * from dbo.tMenus where Id = {0}", rar2.ID_Menus);
                    rar2.AccessRights.HasView = true;
                    roleAccRights.AccessRights = EncryptionHandler.EncryptRights(rar2.AccessRights, menu);
                    roleAccRights.ID_Roles = ID;
                    if (db.Any("(select * from dbo.tRoleAccessRights where ID = {0})a", roleAccRights.ID)) db.Update(roleAccRights);
                    else db.Add(roleAccRights);

                    if (rar2.Children.Count > 0)
                    {
                        SaveChildRoleAccessRights(rar2.Children, db, ID);
                    }
                }
            }
        }
    }
    public class RolesDb : InSysContext
    {
        public RolesDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tRoles> tRoles { get; set; }
        public DbSet<tRoleAccessRights> tRoleAccessRights { get; set; }
        public DbSet<tCompanyAccessRights> tCompanyAccessRights { get; set; }
        public DbQuery<vRoleAccessRights> vRoleAccessRights { get; set; }
        public DbQuery<vCompanyAccessRights> vCompanyAccessRights { get; set; }
        public DbSet<tMenus> tMenus { get; set; }
    }
}
