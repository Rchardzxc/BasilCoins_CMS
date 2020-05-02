SELECT ts.*, tc.Name Company FROM dbo.tSection ts
LEFT JOIN dbo.tCompany tc ON ts.ID_Company = tc.ID