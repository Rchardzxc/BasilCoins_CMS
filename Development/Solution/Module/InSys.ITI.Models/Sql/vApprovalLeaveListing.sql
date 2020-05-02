(SELECT 
	l.Id
	, l.RefNum
	, e.EmployeeName
	, pri.Name AS LeaveType
	, l.FiledDate
	, l.StartDate
	, l.EndDate
	, CASE WHEN (l.ApproverLevel BETWEEN 2 AND aema.ID_Level) AND l.ID_FilingStatus = 1 THEN
		CAST((l.ApproverLevel-1) AS VARCHAR(10)) + '/' + CAST(aema.ID_Level AS VARCHAR(10))
		ELSE fs.Name END AS Status
	, l.Reasons
FROM tLeave l
	INNER JOIN tPayrollItem pri ON pri.ID = l.ID_LeavePayrollItem
	INNER JOIN vEmployees e ON e.ID_Employee = l.ID_Employee
	INNER JOIN tFilingStatus fs ON fs.ID = l.ID_FilingStatus
	LEFT OUTER JOIN tApprover_Employee_Module aem ON aem.ID_Employee = l.ID_Employee
	LEFT OUTER JOIN tFilingModules fm ON fm.ID = aem.ID_FilingModules
	LEFT OUTER JOIN tMenus m ON m.ID = fm.ID_Menus
	LEFT OUTER JOIN tApprover_Employee_Module_Approvers aema ON aema.ID_Approver_Employee_Module = aem.ID
	LEFT OUTER JOIN (
		SELECT ID_Approver_Employee_Module, MAX(ID_Level) AS ID_Level
		FROM tApprover_Employee_Module_Approvers
		GROUP BY ID_Approver_Employee_Module
	) aema2 ON aema2.ID_Approver_Employee_Module = aem.ID
WHERE (ISNULL(m.Code, 'DashBoard_FilingApplication_ApprovalLeave') = 'DashBoard_FilingApplication_ApprovalLeave')
	AND (aema.ID_Level <= l.ApproverLevel)
	AND (l.IsPosted = 1)
	AND {0} IN (aema.ID_Employee, aema.ID_Employee2, aema.ID_Employee3)
)a