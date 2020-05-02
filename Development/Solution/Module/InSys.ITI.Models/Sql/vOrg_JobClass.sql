SELECT toJC.*, tjc.Name JobClass FROM dbo.tOrg_JobClass toJC
LEFT JOIN dbo.tJobClass tjc ON toJC.ID_JobClass = tjc.ID