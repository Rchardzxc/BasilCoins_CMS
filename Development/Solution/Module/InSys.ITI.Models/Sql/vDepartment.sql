SELECT tdep.*, tc.Name Company FROM dbo.tDepartment tdep
LEFT JOIN dbo.tCompany tc ON tdep.ID_Company = tc.ID