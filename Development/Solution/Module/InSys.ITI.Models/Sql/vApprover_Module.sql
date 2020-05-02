SELECT
am.ID,
m.Name Name,
am.ID_Approver,
am.ID_FilingModules,
am.IsActive
FROM dbo.tApprover_Module am
LEFT JOIN dbo.tFilingModules fm ON fm.ID = am.ID_FilingModules
LEFT JOIN dbo.tMenus m ON m.Id = fm.ID_Menus
WHERE ID_Approver = {0} AND am.IsActive = 1