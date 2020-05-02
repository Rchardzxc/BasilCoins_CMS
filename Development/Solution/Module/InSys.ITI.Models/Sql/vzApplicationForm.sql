SELECT  taf.ID,
	taf.ID_SourcingPartner,
	taf.LastName,
	taf.FirstName,
	taf.MiddleName,
	ISNULL(taf.LastName, '') + ', ' + ISNULL(taf.FirstName, '') + ISNULL((' ' + SUBSTRING(taf.MiddleName, 1, 1)), '')  AS DisplayName,
	taf.Suffix,
	taf.NickName,
	taf.DesiredPay,
	taf.SourcingPartner AS SourcingPartner_Others,
	taf.Age,
	taf.PhoneNo,
	taf.MobileNo,
	taf.EmailAddress,
	taf.DatetimeCreated,
	taf.AvailableStartDate,
	FORMAT(taf.BirthDate, 'MMMM dd, yyyy') as BirthDate,
	tkd.Name Designation,
	tkd2.Name Designation2,
	tsp.Name SourcingPartner,
	ts.Name Salutation,
	SUBSTRING(tg.Name, 1, 1) Gender,
	taf.Height AS Height,
	taf.Weight AS Weight,
	tn.Name Nationality,
	tc.Name Citizenship,
	tr.Name Religion,
	tcs.Name CivilStatus,
	tbt.Name BloodType,
	tp.Name BirthPlaceProvince,

	PresentAdd.AddressProperty AS PresentAddressProperty,
	PermanentAdd.AddressProperty AS PermanentAddressProperty,

	--Government
	taf.SSSNo AS Gov_SSS,
	taf.HDMFNo AS Gov_HDMF,
	taf.PhilHealthNo AS Gov_PHIC,
	taf.TINNo AS Gov_TINNo,
	taf.DriversLicenseNo AS Gov_DriverLicenseNo,
	taf.PassportNo AS Gov_PassportNo,

	--Other Information
	taf.Skills AS Other_Skills,
	taf.CrimeDetails AS Other_CrimeDetails,
	taf.HospitalizedDetails AS Other_HospitalizedDetails,
	taf.MedicalCondition AS Other_MedicalCondition,
	taf.WorkingRelatives AS Other_WorkingRelatives,

	--Family Members
	--Father details
	taff.FatherName AS F_FatherName,
	taff.FatherAge AS F_FatherAge,
	taff.FatherOccupation AS F_FatherOccupation,
	taff.FatherContactNumber AS F_FatherContactNumber,

	--Mother details
	taff.MotherName AS F_MotherName,
	taff.MotherAge AS F_MotherAge,
	taff.MotherOccupation AS F_MotherOccupation,
	taff.MotherContactNumber AS F_MotherContactNumber,

	--Spouse
	taff.SpouseName AS F_SpouseName,
	taff.SpouseAge AS F_SpouseAge,
	taff.SpouseOccupation AS F_SpouseOccupation,
	taff.SpouseContactNumber AS F_SpouseContactNumber,
	taff.NumberOfChildren AS F_NumberOfChildren,
	taff.AgeRange AS F_AgeRange

FROM dbo.tApplicationForm taf
	LEFT OUTER JOIN dbo.tDesignation tkd ON taf.ID_Designation = tkd.ID
	LEFT OUTER JOIN dbo.tDesignation tkd2 ON taf.ID_Designation2 = tkd2.ID
	LEFT OUTER JOIN dbo.tSourcingPartner tsp ON taf.ID_SourcingPartner = tsp.ID
	LEFT OUTER JOIN dbo.tSalutation ts ON taf.ID_Salutation = ts.ID
	LEFT OUTER JOIN dbo.tGender tg ON taf.ID_Gender = tg.ID
	LEFT OUTER JOIN dbo.tNationality tn ON taf.ID_Nationality = tn.ID
	LEFT OUTER JOIN dbo.tCitizenship tc ON taf.ID_Citizenship = tc.ID
	LEFT OUTER JOIN dbo.tReligion tr ON taf.ID_Religion = tr.ID
	LEFT OUTER JOIN dbo.tCivilStatus tcs ON taf.ID_CivilStatus = tcs.ID
	LEFT OUTER JOIN dbo.tBloodType tbt ON taf.ID_BloodType = tbt.ID
	LEFT OUTER JOIN dbo.tProvince tp ON ID_BirthPlace = tp.ID
	LEFT OUTER JOIN dbo.tApplicationForm_Family taff ON taff.ID_ApplicationForm = taf.ID
	LEFT OUTER JOIN (
			SELECT ID_ApplicationForm, AddressProperty
				AddressProperty
			FROM dbo.tApplicationForm_Address
			WHERE IsPresentAddress = 1
	) PresentAdd ON PresentAdd.ID_ApplicationForm = taf.Id
	LEFT OUTER JOIN (
			SELECT ID_ApplicationForm, AddressProperty
				AddressProperty
			FROM dbo.tApplicationForm_Address
			WHERE IsPresentAddress = 0
	) PermanentAdd ON PermanentAdd.ID_ApplicationForm = taf.Id
WHERE taf.ID = {0}