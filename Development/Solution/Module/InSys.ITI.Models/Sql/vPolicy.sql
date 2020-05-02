SELECT tp.*, tc.Name Company FROM dbo.tPolicy tp
LEFT JOIN dbo.tCompany tc ON tp.ID_Company = tc.ID