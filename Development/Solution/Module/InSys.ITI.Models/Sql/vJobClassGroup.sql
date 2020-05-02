SELECT tjcg.*, tc.Name Company FROM dbo.tJobClassGroup tjcg
LEFT JOIN dbo.tCompany tc ON tjcg.ID_Company = tc.ID