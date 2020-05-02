using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.Menu
{
    [Route("api/[controller]/[action]")]
    [EndRequestCompressor]
    public class MenuController : CoreController
    {
        public MenuController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadMenu() => await TaskResult(r =>
        {
            var menu = new SystemMenu(Session, Parameter);
            r.ResultSet = menu.LoadMenu();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadRights() => await TaskResult(r =>
        {
            try
            {
                var m = new SystemMenu(Session, Parameter);
                using (var db = new MenuDb(null))
                {
                    if (Session.ID_Roles == 1)
                    {
                        r.ResultSet = new ReturnSet() { Data = new MenuPrivileges() { HasDelete = true, HasEdit = true, HasNew = true, HasView = true, IsApprover = true, ID_Level = 1, IsPowerApprover = true }, Type = ReturnType.Result };
                        return r;
                    }
                    else
                    {
                        var menu = db.Single<tRoleAccessRights>("SELECT trar.* FROM dbo.tRoleAccessRights trar LEFT JOIN dbo.tMenus tm ON trar.ID_Menus = tm.ID WHERE trar.ID_Roles = {0} AND tm.Code = {1}", Session.ID_Roles.ToInt32(), Parameter["Code"].IsNull("").ToString());
                        if (menu == null)
                        {
                            r.ResultSet = new ReturnSet() { Message = "Menu not found.", Type = ReturnType.Error };
                            return r;
                        }
                        else
                        {
                            var accessRights = m.MenuAccessRights(menu.AccessRights);
                            var approver = db.Single<tApprover_Employee_Module_Approvers>("SELECT taema.*"
                                    + " FROM dbo.tApprover_Employee_Module_Approvers taema"
                                    + "     INNER JOIN dbo.tApprover_Employee_Module taem on taema.ID_Approver_Employee_Module = taem.ID"
                                    + "     INNER JOIN dbo.tFilingModules tfm on taem.ID_FilingModules = tfm.ID"
                                    + "     INNER JOIN dbo.tMenus m ON m.ID = tfm.ID_Menus"
                                    + " WHERE m.Code = {0}"
                                    + " AND taema.ID_Employee = {1} or taema.ID_Employee2 ={1} or taema.ID_Employee3 = {1}", Parameter["Code"].IsNull("").ToString(), Session.ID_Employee.ToInt32());
                            if (approver != null)
                            {
                                accessRights.IsApprover = true;
                                accessRights.ID_Level = approver.ID_Level;
                                accessRights.IsPowerApprover = approver.IsPowerApprover;
                            }
                            r.ResultSet = new ReturnSet() { Data = accessRights, Type = ReturnType.Result };
                            return r;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRights", Helpers.CurrentUser(Session), "InSys.Menu");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadSchema() => await TaskResult(r =>
        {
            var menu = new SystemMenu(Session, Parameter);
            r.ResultSet = menu.LoadSchema();
            return r;
        });

    }
}
