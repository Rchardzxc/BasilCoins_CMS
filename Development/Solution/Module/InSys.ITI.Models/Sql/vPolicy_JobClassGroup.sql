SELECT tpjg.*, tp.Name Policy, tjcg.Name JobClassGroup FROM dbo.tPolicy_JobClassGroup tpjg
LEFT JOIN dbo.tPolicy tp ON tpjg.ID_Policy = tp.ID
LEFT JOIN dbo.tJobClassGroup tjcg ON tpjg.ID_JobClassGroup = tjcg.ID