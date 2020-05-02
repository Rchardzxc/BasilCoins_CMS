using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using z.Data;

namespace InSys.ITI.Menu
{
    public class SystemMenu
    {
        private BrowserSession Ctx { get; set; }
        private Pair param { get; set; }
        public SystemMenu(BrowserSession _ctx, Pair _Q)
        {
            this.Ctx = _ctx;
            this.param = _Q;
            if (Ctx == null) throw new Exception("Session Expired.");
        }
        public virtual MenuPrivileges MenuAccessRights(string code)
        {
            var priv = new MenuPrivileges()
            {
                HasView = EncryptionHandler.GetRights("view", code),
                HasNew = EncryptionHandler.GetRights("new", code),
                HasEdit = EncryptionHandler.GetRights("edit", code),
                HasDelete = EncryptionHandler.GetRights("delete", code),
                PostJob = EncryptionHandler.GetRights("postjob", code)
            };
            return priv;
        }
        public virtual ReturnSet LoadMenu()
        {
            var r = new ReturnSet();
            try
            {
                if (Ctx.ID_Roles == 1)
                {
                    r = new ReturnSet() { Data = MenuCollection.SystemMenu.FirstOrDefault().Menus, Type = ReturnType.Result };
                }
                else
                {
                    var data = new List<Menu>();
                    using (var db = new SystemMenuDb(null))
                    {
                        if (Ctx.ID_Roles == 1)
                        {
                            data = MenuCollection.SystemMenu.FirstOrDefault().Menus;
                        }
                        else
                        {
                            var userCompany = MenuCollection.CompanyCollection.Where(x => x.ID == Ctx.ID_Company).FirstOrDefault();
                            data = db.ExecQuery<tRoleAccessRights>("select trar.* from dbo.tRoleAccessRights trar where trar.ID_Roles = {0}", Ctx.ID_Roles)
                                .Where(x => EncryptionHandler.GetRights("view", x.AccessRights) == true)
                                .Join(MenuCollection.SystemMenu.Where(x => x.Company == userCompany).FirstOrDefault().Menus, trar => trar.ID_Menus, menus => menus.ID, (trar, menus) => new Menu()
                                {
                                    Code = menus.Code,
                                    ID = menus.ID,
                                    ID_Menus = menus.ID_Menus,
                                    Name = menus.Name,
                                    SequenceNumber = menus.SequenceNumber,
                                    ID_MenusType = menus.ID_MenusType,
                                    IsVisible = menus.IsVisible
                                })
                                .ToList();
                        }
                    }
                    r = new ReturnSet()
                    {
                        Data = data,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadMenu", Helpers.CurrentUser(Ctx), "InSys.Menu");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }

            return r;
        }
        public ReturnSet LoadSchema()
        {
            var r = new ReturnSet();
            try
            {
                return new ReturnSet()
                {
                    Data = Helpers.GetSchema(param["Table"].ToString()),
                    Type = ReturnType.Result
                };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadMenu", Helpers.CurrentUser(Ctx), "InSys.Menu");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
    }
    public class SystemMenuDb : InSysContext
    {
        public SystemMenuDb(BrowserSession Session) : base(Session)
        {
        }

        public DbSet<tRoleAccessRights> tRoleAccessRights { get; set; }
    }
}
