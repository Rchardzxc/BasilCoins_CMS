SELECT te.*, ISNULL(ve.EmployeeName, tu.LogInName) UserCreatedBy FROM dbo.tEtfMigration te
LEFT JOIN dbo.tUsers tu ON te.ID_UserCreatedBy = tu.ID
LEFT JOIN dbo.vEmployees ve ON tu.ID_Employee = ve.ID_Employee