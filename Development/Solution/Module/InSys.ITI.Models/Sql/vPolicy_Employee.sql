SELECT tpe.*, tp.Name Policy, tper.Name Employee FROM dbo.tPolicy_Employee tpe
LEFT JOIN dbo.tPolicy tp ON tpe.ID_Policy = tp.ID
LEFT JOIN dbo.tEmployee te ON tpe.ID_Employee = te.ID
LEFT JOIN dbo.tPersona tper ON te.ID_Persona = tper.ID