IF OBJECT_ID('fApproverMatrixPerLevel') IS NOT NULL DROP FUNCTION [fApproverMatrixPerLevel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION dbo.[fApproverMatrixPerLevel](@ID_Menu INT, @ID_Employee INT= 0, @ID_Level INT = 0)
RETURNS @AproverMatrixPerLevel TABLE (
	ID_Menu INT
	, ID_FilingModule INT
	, ID_Employee INT
	, Employee VARCHAR(150)
	, ID_Level INT
	, Approver1 VARCHAR(150)
	, Approver2 VARCHAR(150)
	, Approver3 VARCHAR(150)
	, ID_Approver1 INT
	, ID_Approver2 INT
	, ID_Approver3 INT
	, IsPowerApprover BIT
) AS
BEGIN

	DECLARE @EmployeeList TABLE (ID_Employee INT, ID_User INT, Name VARCHAR(150))
	INSERT INTO @EmployeeList(ID_Employee, ID_User, Name)
	SELECT te.ID, tu.ID, tp.Name
	FROM tEmployee te
		INNER JOIN tPersona tp ON tp.ID = te.ID_Persona
		INNER JOIN tUsers tu ON tu.ID_Employee = te.ID

	INSERT INTO @AproverMatrixPerLevel (ID_Menu
			, ID_FilingModule
			, ID_Employee
			, Employee
			, ID_Level
			, Approver1
			, Approver2
			, Approver3
			, ID_Approver1
			, ID_Approver2
			, ID_Approver3
			, IsPowerApprover)
	SELECT tfm.ID_Menus
		, taem.ID_FilingModules
		, taem.ID_Employee
		, el.Name
		, taema.ID_Level
		, el1.Name AS Approver1
		, el2.Name AS Approver2
		, el3.Name AS Approver3
		, ISNULL(el1.ID_User, 0)
		, ISNULL(el2.ID_User, 0)
		, ISNULL(el3.ID_User, 0)
		, taema.IsPowerApprover
	FROM tApprover_Employee_Module taem
		INNER JOIN tFilingModules tfm ON tfm.ID = taem.ID_FilingModules
		INNER JOIN tApprover_Employee_Module_Approvers taema ON taema.ID_Approver_Employee_Module = taem.ID
		LEFT OUTER JOIN @EmployeeList el ON el.ID_Employee = taem.ID_Employee
		LEFT OUTER JOIN @EmployeeList el1 ON el1.ID_Employee = taema.ID_Employee
		LEFT OUTER JOIN @EmployeeList el2 ON el2.ID_Employee = taema.ID_Employee2
		LEFT OUTER JOIN @EmployeeList el3 ON el3.ID_Employee = taema.ID_Employee3
	WHERE tfm.ID_Menus = @ID_Menu
		AND (taem.ID_Employee = @ID_Employee OR @ID_Employee = 0)
		AND (taema.ID_Level = @ID_Level OR @ID_Level = 0)

	RETURN

END


GO
