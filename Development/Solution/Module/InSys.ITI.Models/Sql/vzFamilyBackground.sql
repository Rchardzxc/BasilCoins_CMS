SELECT (CAST(ROW_NUMBER() OVER(PARTITION BY ID_ApplicationForm ORDER BY ID_ApplicationForm, Age) AS VARCHAR(2)) + ')') AS Number, ID_ApplicationForm, Name, Age, Occupation, ContactNumber
FROM tApplicationForm_Siblings
WHERE ID_ApplicationForm = {0}