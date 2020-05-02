(SELECT DISTINCT 
	tt.ID,
	tt.Title,
	tt.[Description],
	tt.StartDate,
	tt.EndDate,
	tt.ApplicationExpirationDate,
	tt.CourseRating,
	tt.ExpectedAttendees,
	tt.Attachments,
	tt.PenaltyForWithdrawal,
	tt.PenaltyForNoShow,
	tt.WithCertification,
	tt.Price,
	tt.Code,
	tt.StartTime,
	tt.EndTime,
	tt.Posted,
	tt.ID_FilingStatus,
	tt.ReferenceNumber,
	tt.PreRequisite,
	tt.ID_Company,
	tt.Location,
	tt.[Provider],
	tt.BondInMonths,
	tt.ID_PaymentTerms,
	tt.HideTrainingBond,

	fam.ID_Approver1,
	fam.ID_Approver2,
	fam.ID_Approver3,
	fam.Approver1,
	fam.Approver2,
	fam.Approver3,
	fam.ID_Level,
	tp.Name Employee,
	te.ID ID_Employee,
	tc.Name Company, 
	tfs.Name FilingStatus, 
	tta.ID_Status, 
	tta.ReasonForNotAttending, 
	tfs2.Name EmployeeFilingStatus, 
	tta.ID_EmployeeFilingStatus, 
	tta.ApproverLevel
	FROM dbo.tTraining tt
LEFT JOIN dbo.tEligibleTrainee tet ON tt.ID = tet.ID_Training
INNER JOIN dbo.fEmployeeOrg(0) te ON 
	(tet.ID_Type = 1 AND tet.ID_Value = te.ID_Department) OR 
	(tet.ID_Type = 2 AND tet.ID_Value = te.ID_Designation) OR
	(tet.ID_Type = 3 AND tet.ID_Value = te.ID)
LEFT JOIN dbo.tCompany tc ON tt.ID_Company = tc.ID
LEFT JOIN dbo.tFilingStatus tfs ON tt.ID_FilingStatus = tfs.ID
LEFT JOIN dbo.tTrainingApplicants tta ON tt.ID = tta.ID_Training AND tta.ID_Employee = te.ID
LEFT JOIN dbo.tFilingStatus tfs2 ON tta.ID_EmployeeFilingStatus = tfs2.ID
LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID
INNER JOIN dbo.fApproverMatrix({0}) fam ON te.ID = fam.ID_Employee AND fam.ID_Level = tta.ApproverLevel
WHERE tt.ID_FilingStatus = 2
	AND tta.ID_EmployeeFilingStatus = 1 /*FilingStatus ng Employee*/
	AND tta.ID_Status = 1 /*Accept*/
	AND ({1} IN (fam.ID_Approver1, fam.ID_Approver2, fam.ID_Approver3))
)a