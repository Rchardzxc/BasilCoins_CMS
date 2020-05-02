ALTER FUNCTION fGetApprovalHistory(@ID_Menus INT, @ID_Reference INT, @ID_TargetEmployee INT)
RETURNS VARCHAR(MAX)
BEGIN
	RETURN (SELECT DISTINCT
	(STUFF((
	SELECT '(' + tah.ApprovalAction + ') ' + tp.Name + (CASE WHEN ISNULL(tah.ApproverComment, '') = '' THEN '' ELSE ': ' END) + ISNULL(tah.ApproverComment, '') + ' ('+ FORMAT(tah.DateTimeCreated, 'MMM dd, yyyy hh:mm tt') +')' + '<br>'
	FROM dbo.tApprovalHistory tah
	LEFT JOIN dbo.tEmployee te ON tah.ID_Employee = te.ID
	LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID
	WHERE tah.ID_Menus = @ID_Menus AND tah.ID_Reference = @ID_Reference AND tah.ID_TargetEmployee = @ID_TargetEmployee
	ORDER BY tah.ApprovalLevel ASC
	FOR XML PATH(''), TYPE, ROOT).value('root[1]', 'nvarchar(max)'), 2, 0, ''))
	)
END
GO
