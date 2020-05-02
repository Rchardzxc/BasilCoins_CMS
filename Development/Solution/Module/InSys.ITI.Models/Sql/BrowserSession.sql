--select tu.ID_Employee, tu.ID ID_User, null ID_Company, tu.ID_Roles, tr.Name Roles, null Company, ISNULL(tp.Name, tu.LogInName) Name, tdes.Name Designation, tp.ImageFile Image, null WebNotificationServer from dbo.tUsers tu
--left join dbo.tRoles tr on tu.ID_Roles = tr.ID
--left join dbo.tEmployee te on tu.ID_Employee = te.ID
--left join dbo.tPersona tp on te.ID_Persona = tp.ID
--LEFT JOIN dbo.tOrg_Designation toDes ON te.ID_Org_Designation = toDes.ID
--left join dbo.tDesignation tdes on toDes.ID_Designation = tdes.ID
select tu.ID_Employee, tu.ID ID_User, null ID_Company, tu.ID_Roles, tr.Name Roles, null Company, ISNULL(tp.Name, tu.LogInName) Name, org.Name Designation, tp.ImageFile Image, null WebNotificationServer, NULL SessionID from dbo.tUsers tu
left join dbo.tRoles tr on tu.ID_Roles = tr.ID
left join dbo.tEmployee te on tu.ID_Employee = te.ID
left join dbo.tPersona tp on te.ID_Persona = tp.ID
OUTER APPLY (SELECT Name FROM dbo.fGetOrg(te.ID_Org) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('designation', {0})) org