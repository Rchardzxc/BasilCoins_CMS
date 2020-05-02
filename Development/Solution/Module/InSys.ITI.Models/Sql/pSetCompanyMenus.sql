-- IF OBJECT_ID('pInsertCompanyMenus') IS NOT NULL DROP PROCEDURE [pInsertCompanyMenus]
-- GO
-- SET ANSI_NULLS ON
-- GO
-- SET QUOTED_IDENTIFIER ON
-- GO
-- CREATE PROCEDURE dbo.[pInsertCompanyMenus]
-- AS

DECLARE @Companies TABLE (Id INT)
INSERT INTO @Companies(Id)
SELECT ID
FROM tCompany x
WHERE NOT EXISTS(SELECT TOP 1 ID FROM tMenus tmp WHERE tmp.ID_Company = x.ID)

INSERT INTO tMenus(ID_Company, Code, Name, ID_Menus, ID_MenusType, IsVisible, IsSystem, IsFilingModule, AssemblyName, ClassName)
SELECT c.Id AS ID_Company, Code, Name, ID_Menus, ID_MenusType, IsVisible, IsSystem, IsFilingModule, AssemblyName, ClassName
FROM @Companies c
	CROSS JOIN tMenus m
WHERE m.ID_Company = 1
	AND NOT EXISTS(SELECT TOP 1 ID FROM tMenus tmp WHERE tmp.ID_Company = c.ID)

DECLARE @Menu1 TABLE(Id INT, Code VARCHAR(50), Name VARCHAR(50), ID_Menus INT)
INSERT INTO @Menu1(Id, Code, Name, ID_Menus)
SELECT Id, Code, Name, ID_Menus
FROM tMenus
WHERE ID_Company = 1

DECLARE @Menu2 TABLE(Id INT, Code VARCHAR(50), Name VARCHAR(50), ID_Menus INT)
INSERT INTO @Menu2(Id, Code, Name, ID_Menus)
SELECT Id, Code, Name, ID_Menus
FROM tMenus
WHERE ID_Company = 2

UPDATE m
SET m.ID_Menus = tmp.ParentId
FROM tMenus m
	INNER JOIN (
			SELECT m2.Id AS Id, tmp2.Id AS ParentId
			FROM (
					SELECT m1.Code AS ChildCode, m2.Code AS ParentCode
					FROM @Menu1 m1
						INNER JOIN @Menu1 m2 ON m2.ID = m1.ID_Menus
				) tmp
				INNER JOIN @Menu2 m2 ON m2.Code = tmp.ChildCode
				INNER JOIN @Menu2 tmp2 ON tmp2.Code = tmp.ParentCode
	) tmp ON tmp.Id = m.Id


-- GO
