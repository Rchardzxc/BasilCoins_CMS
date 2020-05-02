SELECT 
		tm.ID ,
	    tm.Code ,
	    tm.Description ,
	    tm.EffectivityDate ,
	    tm.ID_UserCreatedBy ,
	    tm.DateTimeCreated ,
	    tm.DateTimeModified ,
	    tm.ID_UserModifiedBy ,
	    tm.IsPosted,
		tm.ID_Company,
		ISNULL(ve.EmployeeName, u.LogInName) UserCreatedBy ,
		ISNULL(ve2.EmployeeName, u2.LogInName) UserModifiedBy,
		tc.Name Company
FROM dbo.tMovement tm
LEFT JOIN dbo.tUsers u ON tm.ID_UserCreatedBy = u.ID
LEFT JOIN dbo.vEmployees ve ON u.ID_Employee = ve.ID_Employee
LEFT JOIN dbo.tUsers u2 ON tm.ID_UserModifiedBy = u2.ID
LEFT JOIN dbo.vEmployees ve2 ON u2.ID_Employee = ve2.ID_Employee
LEFT JOIN dbo.tCompany  tc ON tm.ID_Company = tc.ID