(SELECT mrf.ID
	, mrf.ID_Company
	, mrf.MRFNumber
	, td.Name AS Designation
	, nor.Name AS NatureOfRequest
	, mrf.RequestHeadCount
	, CASE WHEN (mrf.ApproverLevel > aema.ID_Level AND rs.[Name] = 'Filed') THEN 'Forwarded to Next Approver' ELSE rs.[Name] END AS [Status]
FROM tMRF mrf
	INNER JOIN tDesignation td ON td.ID = mrf.ID_Designation
	INNER JOIN tNatureOfRequests nor ON nor.ID = mrf.ID_NatureOfRequests
	INNER JOIN tRecruitmentStatus rs ON rs.ID = mrf.ID_RecruitmentStatus
	LEFT OUTER JOIN tApprover_Employee_Module aem ON aem.ID_Employee = mrf.ID_RequestedBy --Employee who created the training data
	LEFT OUTER JOIN tFilingModules fm ON fm.ID = aem.ID_FilingModules
	LEFT OUTER JOIN tMenus m ON m.ID = fm.ID_Menus
	LEFT OUTER JOIN tApprover_Employee_Module_Approvers aema ON aema.ID_Approver_Employee_Module = aem.ID
WHERE rs.Type = 2
	AND (ISNULL(m.Code, 'RecruitmentManpowerRequisitionForm') = 'RecruitmentManpowerRequisitionForm')
	AND {0} IN (aema.ID_Employee, aema.ID_Employee2, aema.ID_Employee3)
	AND mrf.ApproverLevel = aema.ID_Level
	AND (mrf.Posted = CAST(1 AS BIT)))a