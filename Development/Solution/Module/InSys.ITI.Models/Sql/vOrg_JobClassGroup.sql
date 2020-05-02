SELECT toJCG.*, tjcg.Name JobClassGroup FROM dbo.tOrg_JobClassGroup toJCG
LEFT JOIN dbo.tJobClassGroup tjcg ON toJCG.ID_JobClassGroup = tjcg.ID