(SELECT
d.ID ID_Designation,
d.Name Designation,
NOR.ID ID_NatureOfRequests,
NOR.Name NatureOfRequests,
ES.ID ID_EmployeeStatus,
ES.Name EmployeeStatus 

FROM dbo.tMRF AS M
LEFT OUTER JOIN dbo.tDesignation AS D ON d.ID = M.ID_Designation
LEFT OUTER JOIN dbo.tNatureOfRequests AS NOR ON nor.ID = m.ID_NatureOfRequests
LEFT OUTER JOIN dbo.tEmployeeStatus AS ES ON ES.ID = M.ID_EmployeeStatus
WHERE M.ID ={0})