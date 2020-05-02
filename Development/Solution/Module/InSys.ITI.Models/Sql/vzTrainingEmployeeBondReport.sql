SELECT DISTINCT
	t.ID
	, ve.ID_Employee
	, c.Name AS CompanyName
	, ve.EmployeeName
	, t.Title
	, 'In consideration of the abovementioned training, you will undertake and agree to continue to work with ' + 
		c.Name + ' or any member of its group of companies (hereinafter collectively called “the Company”) for ' +
		CAST(ISNULL(etb.NumberOfMonths, 0) AS VARCHAR(50)) + ' (months) commencing from the date of completion of the training programme. ' +
		'(hereinafter referred to as “employment bond”)' AS second_paragraph
	, CONVERT(VARCHAR, etb.StartDate, 101) + ' - ' + CONVERT(VARCHAR, etb.EndDate, 101) AS BondDurationDate
	, etb.NumberOfMonths
	, t.Title AS NameOfCourse
	, t.[Location] AS CourseVenue
	, CONVERT(VARCHAR, t.StartDate, 101) + ' - ' + CONVERT(VARCHAR, t.EndDate, 101) AS TrainingDate --Training Start and End Date
	, t.ConductedBy
	, 'You agree to reimburse to the Company the sum of ' +
		UPPER(CASE WHEN ISNULL(t.TrainingBondAmount, 0) = 0 THEN 'zero' ELSE dbo.fNumberToWords(t.TrainingBondAmount) END) +
		' being the cost spent by the Company for your training as follows:' AS AmountToWords
	, CONVERT(VARCHAR, GETDATE(), 107)AS CurrentDate
FROM tTraining t
	INNER JOIN tTrainingApplicants ta ON ta.ID_Training = t.ID
	INNER JOIN tEmployeeTrainingBond etb ON etb.ID_Training = ta.ID_Training
		AND etb.ID_Employee = ta.ID_Employee
	INNER JOIN vEmployees ve ON ve.ID_Employee = etb.ID_Employee
	INNER JOIN tCompany c ON c.ID = t.ID_Company
WHERE ta.ID_EmployeeFilingStatus = 2 -- Only Approved
	AND ta.ID_Employee = {0}