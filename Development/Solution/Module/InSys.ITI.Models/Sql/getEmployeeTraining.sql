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

	te.ID ID_Employee,
	tc.Name Company, 
	tfs.Name FilingStatus, 
	tta.ID_Status, 
	tta.ReasonForNotAttending, 
	tfs2.Name EmployeeFilingStatus, 
	tta.ID_EmployeeFilingStatus, 
	tta.ApproverLevel,
	tp.Name Employee
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
WHERE tt.ID_FilingStatus = 2 AND te.ID = {0} AND ISNULL(tta.ID_Status, 1) = 1)a