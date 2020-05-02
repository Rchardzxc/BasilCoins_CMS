(SELECT ID_Employee
	, 2.00 AS Leave
	, 1.00 AS Absent
	, 50.00 AS Tardy
	, 20.00 AS Undertime
	, 200.00 AS Overtime
FROM vEmployees
WHERE ID_Employee = {0})