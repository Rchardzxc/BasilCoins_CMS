(SELECT tah.*, tp.Name Approver, tp2.Name Employee FROM dbo.tApprovalHistory tah
LEFT JOIN dbo.tEmployee te ON tah.ID_Employee = te.ID
LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID
LEFT JOIN dbo.tEmployee te2 ON tah.ID_TargetEmployee = te2.ID
LEFT JOIN dbo.tPersona tp2 ON te2.ID_Persona = tp2.ID)a