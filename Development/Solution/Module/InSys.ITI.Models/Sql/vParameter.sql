SELECT 
p.*,
c.Name Company,
tc.Name AS TaxComputation
FROM dbo.tParameter p
LEFT JOIN dbo.tTaxComputation tc ON p.ID_TaxComputation = tc.ID
LEFT JOIN dbo.tCompany c ON p.ID_Company = c.ID