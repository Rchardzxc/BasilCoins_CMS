SELECT tps.*, tp.Name Policy, ts.Name Section FROM dbo.tPolicy_Section tps
LEFT JOIN dbo.tPolicy tp ON tps.ID_Policy = tp.ID
LEFT JOIN dbo.tSection ts ON tps.ID_Section = ts.ID