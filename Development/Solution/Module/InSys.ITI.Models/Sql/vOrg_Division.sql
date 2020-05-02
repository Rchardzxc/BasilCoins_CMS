SELECT toDiv.*, tdiv.Name Division, tp.Name HeadEmployee FROM dbo.tOrg_Division toDiv
LEFT JOIN dbo.tDivision tdiv ON toDiv.ID_Division = tdiv.ID
LEFT JOIN dbo.tEmployee te ON toDiv.ID_HeadEmployee = te.ID
LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID