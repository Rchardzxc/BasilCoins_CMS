select  e.*, per.MiddleInitial, per.FirstName, per.LastName, per.MiddleName, per.Name Persona,
tes.Name EmployeeStatus, tps.Name PayrollScheme, tpf.Name PayrollFrequency, tcba.BankName CompanyBankAcct,
tte.Name TaxExemption, tpm.Name PaymentMode, tp.Name Parameter, tlp.Name LeaveParameter, 
tcur.Name Currency,
tpss.Name PayrollStatus, tpc.Name PayrollClassification, tft.Name FacultyType, tfi.Name FacultyInstitute, tant.Name AccountNumberType,  org.Designation ,
        org.JobClass ,
        org.JobClassGroup ,
        org.Section ,
        org.Department ,
        org.Division ,
        org.Branch, NULL CostCenter,
		tc.Name Company,
		ws.Name WeeklySchedule
FROM dbo.tEmployee e
	LEFT JOIN dbo.tCompany tc ON e.ID_Company = tc.ID
	LEFT JOIN dbo.tPersona per ON e.ID_Persona = per.ID
	LEFT JOIN dbo.tEmployeeStatus tes ON e.ID_EmployeeStatus = tes.ID
	LEFT JOIN dbo.tPayrollScheme tps ON e.ID_PayrollScheme = tps.ID
	LEFT JOIN dbo.tPayrollFrequency tpf ON e.ID_PayrollFrequency = tpf.ID
	LEFT JOIN dbo.tCompanyBankAcct tcba ON e.ID_CompanyBankAcct = tcba.ID
	LEFT JOIN dbo.tTaxExemption tte ON e.ID_TaxExemption = tte.ID
	LEFT JOIN dbo.tPaymentMode tpm ON e.ID_PaymentMode = tpm.ID
	LEFT JOIN dbo.tParameter tp ON e.ID_Parameter = tp.ID
	LEFT JOIN dbo.tLeaveParameter tlp ON e.ID_LeaveParameter = tlp.ID
	LEFT JOIN dbo.tCurrency tcur ON e.ID_Currency = tcur.ID
	LEFT JOIN dbo.tPayrollStatus tpss ON e.ID_PayrollStatus = tpss.ID
	LEFT JOIN dbo.tPayrollClassification tpc ON e.ID_PayrollClassification = tpc.ID
	LEFT JOIN dbo.tFacultyType tft ON e.ID_FacultyType = tft.ID
	LEFT JOIN dbo.tFacultyInstitute tfi ON e.ID_FacultyInstitute = tfi.ID
	LEFT JOIN dbo.tAccountNumberType tant ON e.ID_AccountNumberType = tant.ID
	LEFT JOIN dbo.tWeeklySchedule ws ON e.ID_WeeklySchedule = ws.ID
	CROSS APPLY dbo.fGetTableOrganization(e.ID_Org) org