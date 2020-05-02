SELECT tpc.*, tp.Name Policy, tc.Name Company FROM dbo.tPolicy_Company tpc
LEFT JOIN dbo.tPolicy tp ON tpc.ID_Policy = tp.ID
LEFT JOIN dbo.tCompany tc ON tpc.ID_Company = tc.ID