SELECT 
	tafe.ID_ApplicationForm,
	tafe.SchoolAttended,
	tafe.CourseDegree,
	tafe.[From],
	tafe.[To],
	(CASE WHEN tafe.ID_EducationAttainmentStatus IN(2,4,7) THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END) AS [IsGraduate]
FROM dbo.tApplicationForm_Educational tafe
WHERE tafe.ID_ApplicationForm = {0}