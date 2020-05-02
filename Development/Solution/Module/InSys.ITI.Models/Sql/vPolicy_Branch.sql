SELECT tpb.*, tp.Name Policy, tb.Name Branch FROM dbo.tPolicy_Branch tpb
LEFT JOIN dbo.tPolicy tp ON tpb.ID_Policy = tp.ID
LEFT JOIN dbo.tBranch tb ON tpb.ID_Branch = tb.ID