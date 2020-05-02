ALTER FUNCTION dbo.fApproverMatrix(@ID_Menus INT)
RETURNS TABLE
AS RETURN
	SELECT tfm.ID_Menus, 
		tm.Name Menus,
		taema.ID_Level,
		tp.Name Approver1,
		tp2.Name Approver2,
		tp3.Name Approver3,
		taema.ID_Employee ID_Approver1,
		taema.ID_Employee2 ID_Approver2,
		taema.ID_Employee3 ID_Approver3,
		taema.IsPowerApprover,
		taem.*, -- Employees under the Approver
		tp4.Name Employee
	FROM dbo.tApprover_Employee_Module taem
	LEFT JOIN dbo.tFilingModules tfm ON taem.ID_FilingModules = tfm.ID
	LEFT JOIN dbo.tMenus tm ON tfm.ID_Menus = tm.ID
	LEFT JOIN dbo.tApprover_Employee_Module_Approvers taema ON taem.ID = taema.ID_Approver_Employee_Module
	LEFT JOIN dbo.tEmployee te ON taema.ID_Employee = te.ID
	LEFT JOIN dbo.tEmployee te2 ON taema.ID_Employee2 = te2.ID
	LEFT JOIN dbo.tEmployee te3 ON taema.ID_Employee3 = te3.ID
	LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID
	LEFT JOIN dbo.tPersona tp2 ON te2.ID_Persona = tp2.ID
	LEFT JOIN dbo.tPersona tp3 ON te3.ID_Persona = tp3.ID
	LEFT JOIN dbo.tEmployee te4 ON taem.ID_Employee = te4.ID
	LEFT JOIN dbo.tPersona tp4 ON te4.ID_Persona = tp4.ID
	WHERE tfm.ID_Menus = @ID_Menus
GO
