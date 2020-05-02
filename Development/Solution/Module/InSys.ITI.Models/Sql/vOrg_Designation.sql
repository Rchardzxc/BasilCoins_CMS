SELECT toDes.*, tdes.Name Designation FROM dbo.tOrg_Designation toDes
LEFT JOIN dbo.tDesignation tdes ON toDes.ID_Designation = tdes.ID