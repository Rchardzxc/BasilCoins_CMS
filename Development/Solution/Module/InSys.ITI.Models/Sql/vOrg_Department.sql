SELECT toDep.*, tdep.Name Department, tp.Name HeadEmployee FROM dbo.tOrg_Department toDep
LEFT JOIN dbo.tDepartment tdep ON toDep.ID_Department = tdep.ID
LEFT JOIN dbo.tEmployee te ON toDep.ID_HeadEmployee = te.ID
LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID