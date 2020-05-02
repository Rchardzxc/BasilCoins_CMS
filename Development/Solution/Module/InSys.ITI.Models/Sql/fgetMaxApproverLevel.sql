ALTER FUNCTION fgetMaxApproverLevel(@ID_Employee INT, @ID_Menus INT)
RETURNS INT
BEGIN
	RETURN (SELECT MAX(ID_Level) FROM dbo.fApproverMatrix(@ID_Menus) WHERE ID_Employee = @ID_Employee)
END
GO
