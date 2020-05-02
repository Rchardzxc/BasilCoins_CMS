SELECT ID_ApplicationForm, Name, Company, Position, ContactNo
FROM dbo.tApplicationForm_CharacterReference
WHERE IsEmploymentReference = 0
	AND ID_ApplicationForm = {0}