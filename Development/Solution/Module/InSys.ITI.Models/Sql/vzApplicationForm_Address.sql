SELECT tafa.ID,
	tafa.ID_ApplicationForm,
	tafa.HouseNo,
	tafa.StreetName,
	tafa.ZipCode,
	tc.Name City,
	tp.Name Province,
	tb.Name Barangay,

	tmp.HouseNo AS PermanentHouseNo,
	tmp.StreetName AS PermanentStreetName,
	tmp.City AS PermanentCity,
	tmp.Province AS PermanentProvince,
	tmp.Barangay AS PermanentBarangay,
	tmp.ZipCode AS PermanentZipCode
FROM dbo.tApplicationForm_Address tafa
	INNER JOIN (
			SELECT tafa.ID_ApplicationForm,
				tafa.HouseNo,
				tafa.StreetName,
				tafa.ZipCode,
				tc.Name City,
				tp.Name Province,
				tb.Name Barangay
			FROM dbo.tApplicationForm_Address tafa
				LEFT JOIN dbo.tCity tc ON tafa.ID_City = tc.ID
				LEFT JOIN dbo.tProvince tp ON tafa.ID_Province = tp.ID
				LEFT JOIN dbo.tBarangay tb ON tafa.ID_Barangay = tb.ID
			WHERE tafa.IsPresentAddress = 0
	) tmp ON tmp.ID_ApplicationForm = tafa.ID_ApplicationForm
	LEFT JOIN dbo.tCity tc ON tafa.ID_City = tc.ID
	LEFT JOIN dbo.tProvince tp ON tafa.ID_Province = tp.ID
	LEFT JOIN dbo.tBarangay tb ON tafa.ID_Barangay = tb.ID
WHERE tafa.IsPresentAddress = 1
	AND tafa.ID_ApplicationForm = {0}