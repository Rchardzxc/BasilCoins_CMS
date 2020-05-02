(select taf.*,
tkd.Name Designation,
tkd2.Name Designation2,
tkm.MRFNumber MRFNumber,
tsp.Name SourcingPartner2,
ts.Name Salutation,
tg.Name Gender,
tn.Name Nationality,
tc.Name Citizenship,
tr.Name Religion,
tcs.Name CivilStatus,
tbt.Name BloodType,
trs.Name RecruitmentStatus,
tp.Name AS BirthPlace
from dbo.tApplicationForm taf
LEFT JOIN dbo.tMRF tkm ON taf.ID_MRF = tkm.ID
LEFT JOIN dbo.tDesignation tkd ON taf.ID_Designation = tkd.ID
LEFT JOIN dbo.tDesignation tkd2 ON taf.ID_Designation2 = tkd2.ID
LEFT JOIN dbo.tSourcingPartner tsp ON taf.ID_SourcingPartner = tsp.ID
LEFT JOIN dbo.tSalutation ts ON taf.ID_Salutation = ts.ID
LEFT JOIN dbo.tGender tg ON taf.ID_Gender = tg.ID
LEFT JOIN dbo.tNationality tn ON taf.ID_Nationality = tn.ID
LEFT JOIN dbo.tCitizenship tc ON taf.ID_Citizenship = tc.ID
LEFT JOIN dbo.tReligion tr ON taf.ID_Religion = tr.ID
LEFT JOIN dbo.tCivilStatus tcs ON taf.ID_CivilStatus = tcs.ID
LEFT JOIN dbo.tBloodType tbt ON taf.ID_BloodType = tbt.ID
LEFT JOIN dbo.tRecruitmentStatus trs ON taf.ID_RecruitmentStatus = trs.ID
LEFT JOIN dbo.tProvince tp ON taf.ID_BirthPlace = tp.ID
WHERE taf.ID = {0})