SELECT tpd.*, tp.Name Policy, td.Name Designation FROM dbo.tPolicy_Designation tpd
LEFT JOIN dbo.tPolicy tp ON tpd.ID_Policy = tp.ID
LEFT JOIN dbo.tDesignation td ON tpd.ID_Designation = td.ID