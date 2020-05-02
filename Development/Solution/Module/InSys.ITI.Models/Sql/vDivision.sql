SELECT td.*, tc.Name Company FROM dbo.tDivision td
LEFT JOIN dbo.tCompany tc ON td.ID_Company = tc.ID