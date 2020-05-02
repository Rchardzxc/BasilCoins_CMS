SELECT tp.*,
	tg.Name Gender,
	tr.Name Religion,
	tn.Name Nationality,
	tc.Name Citizenship,
	tcv.Name CivilStatus,
	tb.Name BloodType,
	tsss.Name SSSStatus
FROM dbo.tPersona tp
LEFT JOIN dbo.tGender tg ON tp.ID_Gender = tg.ID
LEFT JOIN dbo.tReligion tr ON tp.ID_Religion = tr.ID
LEFT JOIN dbo.tNationality tn ON tp.ID_Nationality = tn.ID
LEFT JOIN dbo.tCitizenship tc ON tp.ID_Citizenship = tc.ID
LEFT JOIN dbo.tCivilStatus tcv ON tp.ID_CivilStatus = tcv.ID
LEFT JOIN dbo.tBloodType tb ON tp.ID_BloodType = tb.ID
LEFT JOIN dbo.tSSSStatus tsss ON tp.ID_SSSStatus = tsss.ID