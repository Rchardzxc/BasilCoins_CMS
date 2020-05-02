SELECT tpd.*, tp.Name Policy, td.Name Department FROM dbo.tPolicy_Department tpd
LEFT JOIN dbo.tPolicy tp ON tpd.ID_Policy = tp.ID
LEFT JOIN dbo.tDepartment td ON tpd.ID_Department = td.ID