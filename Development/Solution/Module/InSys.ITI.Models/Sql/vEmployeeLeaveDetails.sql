(SELECT gb.ID_Employee, gb.ID_LeavePayrollItem, lt.Name AS Code, gb.Alloted, gb.Used, gb.EndBalance
FROM dbo.[fGetEmployeeBalance]({0}, {1}, {2}) gb
	INNER JOIN tLeaveType lt ON lt.ID = gb.ID_LeavePayrollItem)