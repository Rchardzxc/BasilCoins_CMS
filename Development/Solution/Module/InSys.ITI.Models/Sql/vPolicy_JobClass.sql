SELECT tpj.*, tp.Name Policy, tj.Name JobClass FROM dbo.tPolicy_JobClass tpj
LEFT JOIN dbo.tPolicy tp ON tpj.ID_Policy = tp.ID
LEFT JOIN dbo.tJobClass tj ON tpj.ID_JobClass = tj.ID