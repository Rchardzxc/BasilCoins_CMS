(SELECT tafd.*,
tp.Name Province,
tc.Name City,
tb.Name Barangay
FROM dbo.tApplicationForm_Address tafd
LEFT JOIN  dbo.tProvince tp ON tafd.ID_Province = tp.ID
LEFT JOIN  dbo.tCity tc ON tafd.ID_City = tc.ID
LEFT JOIN  dbo.tBarangay tb ON tafd.ID_Barangay = tb.ID
WHERE ID_ApplicationForm = {0})