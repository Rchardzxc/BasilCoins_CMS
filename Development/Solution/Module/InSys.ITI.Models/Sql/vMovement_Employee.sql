SELECT 
		 tme.ID ,
		 tme.ID_Movement ,
		 tme.ID_Employee ,
		 tme.ID_UserCreatedBy ,
		 tme.DateTimeCreated ,
		 tme.DateTimeModified ,
		 tme.ID_UserModifiedBy,
		ISNULL(ve.EmployeeName, u.LogInName) UserCreatedBy ,
		ISNULL(ve2.EmployeeName, u2.LogInName) UserModifiedBy ,
		ve3.FullName Employee
FROM dbo.tMovement_Employee tme
LEFT JOIN dbo.tUsers u ON tme.ID_UserCreatedBy = u.ID
LEFT JOIN dbo.vEmployees ve ON u.ID_Employee = ve.ID_Employee
LEFT JOIN dbo.tUsers u2 ON tme.ID_UserModifiedBy = u2.ID
LEFT JOIN dbo.vEmployees ve2 ON u2.ID_Employee = ve2.ID_Employee
LEFT JOIN dbo.vEmployees ve3 ON tme.ID_Employee = ve3.ID_Employee