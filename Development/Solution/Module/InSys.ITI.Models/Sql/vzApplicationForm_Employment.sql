SELECT ID, ID_ApplicationForm, Company, [Address], [From], [To], Position, Salary, ReasonForLeaving
FROM dbo.tApplicationForm_Employment
WHERE ID_ApplicationForm = {0}