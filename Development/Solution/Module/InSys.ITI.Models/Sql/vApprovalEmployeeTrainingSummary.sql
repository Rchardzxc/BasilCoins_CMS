(
	SELECT tt.ID,
		tt.ID_Company,
		tt.ReferenceNumber,
		tt.Title,
		tt.StartDate,
		tt.EndDate,
		tt.Location,
		tt.CourseRating,
		CASE WHEN (tt.ApproverLevel > aema.ID_Level AND tt.ID_FilingStatus = 1) THEN 'Forwarded to Next Approver' ELSE fs.Name END AS Status
	 FROM tTraining tt
		 INNER JOIN tFilingStatus fs ON fs.ID = tt.ID_FilingStatus
		 LEFT OUTER JOIN tApprover_Employee_Module aem ON aem.ID_Employee = tt.ID_Employee --Employee who created the training data
		 LEFT OUTER JOIN tFilingModules fm ON fm.ID = aem.ID_FilingModules
		 LEFT OUTER JOIN tMenus m ON m.ID = fm.ID_Menus
		 LEFT OUTER JOIN tApprover_Employee_Module_Approvers aema ON aema.ID_Approver_Employee_Module = aem.ID
	 WHERE (ISNULL(m.Code, 'TrainingTrainingSummary') = 'TrainingTrainingSummary')
		AND {0} IN (aema.ID_Employee, aema.ID_Employee2, aema.ID_Employee3)
		AND tt.ApproverLevel = aema.ID_Level
		AND (tt.Posted = CAST(1 AS BIT))
)a