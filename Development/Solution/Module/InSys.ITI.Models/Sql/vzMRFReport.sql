SELECT mrf.ID AS ID,
	mrf.MRFNumber,
	td.Name AS Position,
	tdep.Name AS Department,
	tdiv.Name AS Division,
	tjc.Name AS Rank,
	ISNULL(tnor.ID, 0) AS ID_NatureOFRequests,
	CASE WHEN tnor.ID IN (1, 2) THEN '' ELSE tnor.Name END AS JustificationForRequest,
	tes.Name AS EmployeeStatus,
	tes.Code AS EmployeeStatusCode,
	tp.Name AS RequestedBy,
	tp2.Name AS PersonToBeReplaced,
	ea.Name AS EducationalRequirements,
	td.JobSummary AS JobDescription,
	td.Qualifications AS RequiredTechnicalSkills,
	td.Requirements AS RequiredBehavioralSkills,
	ISNULL(rr.ID, 0) AS ID_ReplacementReason,
	CASE WHEN rr.ID IN (1, 2, 3, 6) THEN '' ELSE rr.Name END AS ReplacementReason,
	mrf.LocationStoreName,
	mrf.LocationStoreCode_CostCenter,
	mrf.LocationBranchArea,
	mrf.LocationReportingSchedule,
	mrf.LocationAddress,
	mrf.IsBudgeted,
	mrf.MaleCount,
	mrf.FemaleCount,
	mrf.NoGenderPreference,
	mrf.RequestHeadCount,
	CAST(mrf.MinSalary AS VARCHAR(10)) + ' - ' + CAST(mrf.MaxSalary AS VARCHAR(10))  AS SalaryRange,
	mrf.MinSalary,
	mrf.MaxSalary
FROM tMRF mrf
	LEFT OUTER JOIN tDesignation td ON td.ID = mrf.ID_Designation
	LEFT OUTER JOIN tOrg_Designation odes ON odes.ID_Designation = td.ID
	LEFT OUTER JOIN tOrg_JobClass ojc ON ojc.ID = odes.ID_Org_JobClass
	LEFT OUTER JOIN tJobClass tjc ON tjc.ID = ojc.ID_JobClass
	LEFT OUTER JOIN tOrg_JobClassGroup ojcg ON ojcg.ID = ojc.ID_Org_JobClassGroup
	LEFT OUTER JOIN tOrg_Section osec ON osec.ID = ojcg.ID_Org_Section
	LEFT OUTER JOIN tOrg_Department odep ON odep.ID = osec.ID_Org_Department
	LEFT OUTER JOIN tDepartment tdep ON tdep.ID = odep.ID_Department
	LEFT OUTER JOIN tOrg_Division odiv ON odiv.ID = odep.ID_Org_Division
	LEFT OUTER JOIN tDivision tdiv ON tdiv.ID = odiv.ID_Division
	LEFT OUTER JOIN tEducationalAttainment ea ON ea.ID = td.ID_EducationalAttainment
	LEFT OUTER JOIN tNatureOfRequests tnor ON tnor.ID = mrf.ID_NatureOfRequests
	LEFT OUTER JOIN tEmployeeStatus tes ON tes.ID = mrf.ID_EmployeeStatus
	LEFT OUTER JOIN tEmployee te ON te.ID = mrf.ID_RequestedBy
	LEFT OUTER JOIN tPersona tp ON tp.ID = te.ID_Persona
	LEFT OUTER JOIN tEmployee te2 ON te2.ID = mrf.ID_PersonToBeReplaced
	LEFT OUTER JOIN tPersona tp2 ON tp2.ID = te2.ID_Persona
	LEFT OUTER JOIN dbo.fReplacementReasons() rr ON rr.ID = mrf.ReplacementReason
WHERE mrf.ID = {0}