SELECT td.*, tc.Name Company, tea.Name EducationalAttainment FROM dbo.tDesignation td
LEFT JOIN dbo.tCompany tc ON td.ID_Company = tc.ID
LEFT JOIN dbo.tEducationalAttainment tea ON td.ID_EducationalAttainment = tea.ID