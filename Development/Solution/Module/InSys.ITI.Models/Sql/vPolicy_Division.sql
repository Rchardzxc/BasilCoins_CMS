SELECT tpd.*, tp.Name Policy, td.Name Division FROM dbo.tPolicy_Division tpd
LEFT JOIN dbo.tPolicy tp ON tpd.ID_Policy = tp.ID
LEFT JOIN dbo.tDivision td ON tpd.ID_Division = td.ID