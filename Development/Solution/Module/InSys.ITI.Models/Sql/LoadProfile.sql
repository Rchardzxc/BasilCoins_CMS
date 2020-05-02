--select tu.ID ID_User, tp.LastName, tp.FirstName, ISNULL(tp.Name, tu.LogInName) LogInName, tp.ImageFile Image,
--td.Name Designation, te.CompanyEmail, tes.Name EmployeeStatus, te.AccessNo, te.HiredDate DateHired, tp.SSSNo, tp.HDMFNo, tp.TINNo, tp.PhilHealthNo
--from dbo.tUsers tu
--left join dbo.tEmployee te on tu.ID_Employee = te.ID
--LEFT JOIN dbo.tOrg_Designation toDes ON te.ID_Org_Designation = toDes.ID
--left join dbo.tDesignation td on toDes.ID_Designation = td.ID
--left join dbo.tEmployeeStatus tes on te.ID_EmployeeStatus = tes.ID
--left join dbo.tPersona tp on te.ID_Persona = tp.ID
SELECT tu.ID ID_User, tp.LastName, tp.FirstName, ISNULL(tp.Name, tu.LogInName) LogInName, tp.ImageFile Image,
org.Name Designation, te.CompanyEmail, tes.Name EmployeeStatus, te.AccessNo, te.HiredDate DateHired, tp.SSSNo, tp.HDMFNo, tp.TINNo, tp.PhilHealthNo
from dbo.tUsers tu
left join dbo.tEmployee te on tu.ID_Employee = te.ID
left join dbo.tEmployeeStatus tes on te.ID_EmployeeStatus = tes.ID
left join dbo.tPersona tp on te.ID_Persona = tp.ID
OUTER APPLY (SELECT Name FROM dbo.fGetOrg(te.ID_Org) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('designation', {0})) org