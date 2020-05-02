(SELECT kmrf.ID, ISNULL(kmrf.MRFNumber, 'No MRF') AS MRFNumber
	, ISNULL(kd.Name, 'No Designation') AS Designation
	, kmrf.PostingDate, ISNULL(kmrf.RequestHeadCount, 0) RequestHeadCount
	, SUM(CASE WHEN ISNULL(af.[Name], '') IN ('Keep for Reference', 'Short Listed') THEN af.ToTalCount ELSE 0 END) AS ShortList
	, SUM(CASE WHEN ISNULL(af.[Name], '') IN ('Unprocessed') THEN af.ToTalCount ELSE 0 END) AS UnProcessed
	, SUM(CASE WHEN ISNULL(af.[Name], '') IN ('For Examination', 'For Interview') THEN af.ToTalCount ELSE 0 END) AS OnProcess
	, SUM(CASE WHEN ISNULL(af.[Name], '') IN ('Hired') THEN af.ToTalCount ELSE 0 END) AS Hired
	, SUM(CASE WHEN ISNULL(af.[Name], '') NOT IN ('Keep for Reference', 'Short Listed', 'Hired', 'Unprocessed', 'For Examination', 'For Interview') THEN ISNULL(af.ToTalCount, 0) ELSE 0 END) AS Others
	, SUM(ISNULL(af.TotalCount, 0)) AS TotalCount
	, kmrf.ID_Company
	, kmrf.ApprovedDate
FROM dbo.tMRF kmrf
	LEFT OUTER JOIN tDesignation kd ON kd.ID = kmrf.ID_Designation
	LEFT OUTER JOIN tRecruitmentStatus rs ON rs.ID = kmrf.ID_RecruitmentStatus
	LEFT OUTER JOIN (
		SELECT COUNT(af.ID) AS TotalCount, ISNULL(ID_MRF, 0) AS ID_MRF, krs.[Name]
		FROM tApplicationForm af
			LEFT OUTER JOIN tRecruitmentStatus krs ON krs.ID = af.ID_RecruitmentStatus
		WHERE krs.Type = 1
		GROUP BY ISNULL(ID_MRF, 0), krs.[Name], krs.[Type]
) af ON af.ID_MRF = kmrf.ID
GROUP BY kmrf.ID, kmrf.ApprovedDate, kmrf.MRFNumber,  kmrf.PostingDate,
	kmrf.RequestHeadCount, kmrf.EndDate, kd.Name, kmrf.ID_Company)a