SELECT 
		 tmed.ID ,
		 tmed.ID_Movement_Employee ,
		 tmed.ID_MovementType ,
		 tmed.ID_UserCreatedBy ,
		 tmed.DateTimeCreated ,
		 tmed.DateTimeModified ,
		 tmed.ID_UserModifiedBy ,
		 tmed.OldValue ,
		 tmed.NewValue ,
		 tmed.DateTimeApplied ,
		 tmed.IsApplied ,
		ISNULL(ve.EmployeeName, u.LogInName) UserCreatedBy ,
		ISNULL(ve2.EmployeeName, u2.LogInName) UserModifiedBy ,
		tmt.Name MovementType ,
		CASE WHEN tmed.ID_MovementType = 4 THEN (SELECT Name FROM dbo.fGetOrg(CAST(tmed.OldValue AS INT)) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('branch', tm.ID_Company))
		ELSE NULL END OldBranch,
		CASE WHEN tmed.ID_MovementType = 6 THEN (SELECT Name FROM dbo.fGetOrg(CAST(tmed.OldValue AS INT)) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('department', tm.ID_Company))
		ELSE NULL END OldDepartment,
		CASE WHEN tmed.ID_MovementType = 7 THEN (SELECT Name FROM dbo.fGetOrg(CAST(tmed.OldValue AS INT)) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('designation', tm.ID_Company))
		ELSE NULL END OldDesignation,
		CASE WHEN tmed.ID_MovementType = 8 THEN (SELECT Name FROM dbo.tEmployeeStatus WHERE ID = CAST(tmed.OldValue AS INT))
		ELSE NULL END OldEmployeeStatus,
		CASE WHEN tmed.ID_MovementType = 13 THEN (SELECT Name FROM dbo.tLeaveParameter WHERE ID = CAST(tmed.OldValue AS INT))
		ELSE NULL END OldLeaveParameter,
		CASE WHEN tmed.ID_MovementType = 4 THEN (SELECT Name FROM dbo.fGetOrg(CAST(tmed.NewValue AS INT)) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('branch', tm.ID_Company))
		ELSE NULL END NewBranch,
		CASE WHEN tmed.ID_MovementType = 6 THEN (SELECT Name FROM dbo.fGetOrg(CAST(tmed.NewValue AS INT)) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('department', tm.ID_Company))
		ELSE NULL END NewDepartment,
		CASE WHEN tmed.ID_MovementType = 7 THEN (SELECT Name FROM dbo.fGetOrg(CAST(tmed.NewValue AS INT)) org WHERE org.ID_CompanyOrgType = dbo.fGetCompanyOrgType('designation', tm.ID_Company))
		ELSE NULL END NewDesignation
FROM dbo.tMovement_Employee_Detail tmed
LEFT JOIN dbo.tMovement_Employee tme ON tmed.ID_Movement_Employee = tme.ID
LEFT JOIN dbo.tMovement tm ON tme.ID_Movement = tm.ID
LEFT JOIN dbo.tUsers u ON tmed.ID_UserCreatedBy = u.ID
LEFT JOIN dbo.vEmployees ve ON u.ID_Employee = ve.ID_Employee
LEFT JOIN dbo.tUsers u2 ON tmed.ID_UserModifiedBy = u2.ID
LEFT JOIN dbo.vEmployees ve2 ON u2.ID_Employee = ve2.ID_Employee
LEFT JOIN dbo.tMovementType tmt ON tmed.ID_MovementType = tmt.ID