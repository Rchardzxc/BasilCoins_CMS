SELECT tjc.*, tc.Name Company FROM dbo.tJobClass tjc
LEFT JOIN dbo.tCompany tc ON tjc.ID_Company = tc.ID