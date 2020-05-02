angular.module('app').config(['$stateProvider', function ($stateProvider) {
    var v = localStorage.getItem("HtmlTemplatePath") + "Web/Views/";


    //Name - This will used as AngularJS Controller and Parameter on MenuCode
    //Url - This will be shown in URL Addres
    //ParentFolder - Folder where the html file is saved
    //TemplateUrl - the html file itself.
    var menus = [
        ////////////////////INDEX
        {
            Name: "MyModule", Url: '/My-Module',
            ParentFolder: v + '/', TemplateUrl: 'MyModule'
        },

        ////////////////////DASHBOARD
        {
            Name: "IONSDashBoardFilingApplications_ApprovalsLeave", Url: '/Dashboard/Approvals/Leave',
            ParentFolder: v + 'Dashboard/Approvals/', TemplateUrl: 'Leave'
        },
        {
            Name: "IONSDashBoardFilingApplications_ApprovalsOB", Url: '/Dashboard/Approvals/Official-Business',
            ParentFolder: v + 'Dashboard/Approvals/', TemplateUrl: 'OfficialBusiness'
        },
        {
            Name: "IONSDashBoardFilingApplications_ApprovalsOvertime", Url: '/Dashboard/Approvals/Overtime',
            ParentFolder: v + 'Dashboard/Approvals/', TemplateUrl: 'Overtime'
        },

        ////////////////////RECRUITMENT
        {
            Name: "RecruitmentMaintenanceExamination", Url: '/Recruitment/Maintenance/Examination',
            ParentFolder: v + 'Recruitment/Maintenance/', TemplateUrl: 'Examination'
        },
        {
            Name: "RecruitmentMaintenanceNatureofRequests", Url: '/Recruitment/Maintenance/Nature-of-Requests',
            ParentFolder: v + 'Recruitment/Maintenance/', TemplateUrl: 'NatureOfRequest'
        },
        {
            Name: "RecruitmentMaintenanceRecruitmentStatus", Url: '/Recruitment/Maintenance/Recruitment-Status',
            ParentFolder: v + 'Recruitment/Maintenance/', TemplateUrl: 'RecruitmentStatus'
        },
        {
            Name: "RecruitmentManpowerPlanning", Url: '/Recruitment/Manpower-Planning',
            ParentFolder: v + 'Recruitment/', TemplateUrl: 'ManpowerPlanning'
        },
        {
            Name: "RecruitmentManpowerRequisitionForm", Url: '/Recruitment/Manpower-Requisition-Form',
            ParentFolder: v + 'Recruitment/', TemplateUrl: 'ManpowerRequisitionForm', ForApproval: true
        },
        {
            Name: "RecruitmentRecruitmentReport", Url: '/Recruitment/Recruitment-Report',
            ParentFolder: v + 'Recruitment/', TemplateUrl: 'RecruitmentReport'
        },
        {
            Name: "RecruitmentResumeBank&Search", Url: '/Recruitment/Resume-Bank-And-Search',
            ParentFolder: v + 'Recruitment/', TemplateUrl: 'ResumeBankAndSearch'
        },

        ////////////////////IONS
        {
            Name: "IONSTraining", Url: '/IONS',
            ParentFolder: v + 'IONS/', TemplateUrl: 'Training'
        },

        ////////////////////ORBIT
        {
            Name: "OrbitLeaveCredit", Url: '/Orbit/Leave-Credit',
            ParentFolder: v + 'Orbit/', TemplateUrl: 'LeaveCredit'
        },
        {
            Name: "OrbitTimeKeepingItems", Url: '/Orbit/Time-Keeping-Items',
            ParentFolder: v + 'Orbit/', TemplateUrl: 'TimeKeepingItems'
        },
        {
            Name: "OrbitMaintenanceLeaveParameter", Url: '/Orbit/Maintenance/Leave-Parameter',
            ParentFolder: v + 'Orbit/Maintenance/', TemplateUrl: 'LeaveParameter'
        },
        {
            Name: "OrbitMaintenanceLeaveType", Url: '/Orbit/Maintenance/Leave-Type',
            ParentFolder: v + 'Orbit/Maintenance/', TemplateUrl: 'LeaveType'
        },
        {
            Name: "OrbitMaintenanceShiftSchedule", Url: '/Orbit/Maintenance/Shift-Schedule',
            ParentFolder: v + 'Orbit/Maintenance/', TemplateUrl: 'ShiftSchedule'
        },
        {
            Name: "OrbitMaintenanceWeeklySchedule", Url: '/ Orbit/Maintenance/Weekly-Schedule',
            ParentFolder: v + 'Orbit/Maintenance/', TemplateUrl: 'WeeklySchedule'
        },
        {
            Name: "OrbitWorkScheduleScheduleFile", Url: '/Orbit/Work-Schedule/Schedule-File',
            ParentFolder: v + 'Orbit/WorkSchedule/', TemplateUrl: 'ScheduleFile'
        },

        ///////////////////////////FILINGS AND APPLICATIONS
        {
            Name: "OrbitFilings&ApprovalsLeave", Url: '/Orbit/Filngs-Approval/Leave',
            ParentFolder: v + 'Orbit/FilingsApprovals/', TemplateUrl: 'OrbitLeave'
        },
        {
            Name: "OrbitFilings&ApprovalsOvertime", Url: '/Orbit/Filngs-Approval/Overtime',
            ParentFolder: v + 'Orbit/FilingsApprovals/', TemplateUrl: 'OrbitOvertime'
        },
        {
            Name: "OrbitFilings&ApprovalsOB", Url: '/Orbit/Filngs-Approval/Official-Business',
            ParentFolder: v + 'Orbit/FilingsApprovals/', TemplateUrl: 'OrbitOB'
        },
        {
            Name: "OrbitFilings&ApprovalsMissedLog", Url: '/Orbit/Filngs-Approval/Missed-Log',
            ParentFolder: v + 'Orbit/FilingsApprovals/', TemplateUrl: 'OrbitMissedLog'
        },
        {
            Name: "OrbitFilings&ApprovalsChangeOfSchedule", Url: '/Orbit/Filngs-Approval/Change-Of-Schedule',
            ParentFolder: v + 'Orbit/FilingsApprovals/', TemplateUrl: 'OrbitChangeOfSchedule'
        },


        
        ////////////////////MAINTENANCE
        {
            Name: "MaintenanceProducts", Url: '/Maintenance/Products',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Products'
        },

        {
            Name: "MaintenanceCategory", Url: '/Maintenance/Category',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Category'
        },

        {
            Name: "MaintenanceProductBrand", Url: '/Maintenance/ProductBrand',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'ProductBrand'
        },

        {
            Name: "MaintenanceYearsofMint", Url: '/Maintenance/YearsofMint',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'YearsofMint'
        },

        {
            Name: "MaintenanceDenomination", Url: '/Maintenance/Denomination',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Denomination'
        },

        {
            Name: "MaintenanceColor", Url: '/Maintenance/Color',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Color'
        },

        {
            Name: "MaintenanceCompanySettings", Url: '/Maintenance/CompanySettings',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'CompanySettings'
        },

        {
            Name: "MaintenanceCustomer", Url: '/Maintenance/Customer',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Customer'
        },

        {
            Name: "MaintenanceFilingStatus", Url: '/Maintenance/FilingStatus',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'FilingStatus'
        },

        {
            Name: "MaintenanceOrders", Url: '/Maintenance/Orders',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Orders'
        },

        {
            Name: "MaintenancePaymentMethod", Url: '/Maintenance/PaymentMethod',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'PaymentMethod'
        },

        {
            Name: "MaintenanceCountry", Url: '/Maintenance/Country',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Country'
        },

        {
            Name: "MaintenanceProvince", Url: '/Maintenance/Province',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'Province'
        },

        {
            Name: "MaintenanceProductsSale", Url: '/Maintenance/ProductsSale',
            ParentFolder: v + 'Maintenance/', TemplateUrl: 'ProductsSale'
        },

        {
            Name: "MaintenanceHomePageBanners", Url: '/Maintenance/HomePage/Banners',
            ParentFolder: v + 'Maintenance/HomePage/', TemplateUrl: 'Banners'
        },

        {
            Name: "MaintenanceHomePageProductsRecommended", Url: '/Maintenance/HomePage/ProductsRecommended',
            ParentFolder: v + 'Maintenance/HomePage/', TemplateUrl: 'ProductsRecommended'
        },

        {
            Name: "MaintenanceHomePageOurCollections", Url: '/Maintenance/HomePage/OurCollections',
            ParentFolder: v + 'Maintenance/HomePage/', TemplateUrl: 'OurCollections'
        },


        ////////////////////ADMINSTRATION
        {
            Name: "AdministrativeApproverMatrix", Url: '/Administrative/Approver-Matrix',
            ParentFolder: v + 'Administrative/', TemplateUrl: 'ApproverMatrix'
        },

        {
            Name: "AdministrativePolicy", Url: '/Administrative/Policy',
            ParentFolder: v + 'Administrative/', TemplateUrl: 'Policy'
        },

        {
            Name: "AdministrativeFilingModules", Url: '/Administrative/Filing-Modules',
            ParentFolder: v + 'Administrative/', TemplateUrl: 'FilingModules'
        },

        {
            Name: "AdministrativeRoles", Url: '/Administrative/Roles',
            ParentFolder: v + 'Administrative/', TemplateUrl: 'Roles'
        },

        {
            Name: "AdministrativeUsers", Url: '/Administrative/Users',
            ParentFolder: v + 'Administrative/', TemplateUrl: 'Users'
        },

        {
            Name: "AdministrativeSignatories", Url: '/Administrative/Signatories',
            ParentFolder: v + 'Administrative/', TemplateUrl: 'Signatories'
        },

        ////////////////////PEOPLE
        {
            Name: "PeopleEmployeeMovement", Url: '/People/Employee-Movement',
            ParentFolder: v + 'People/', TemplateUrl: 'EmployeeMovement'
        },

        {
            Name: "PeopleEmployeeRecord", Url: '/People/Employee-Record',
            ParentFolder: v + 'People/', TemplateUrl: 'EmployeeRecord'
        },

        {
            Name: "PeopleEmploymentRecord", Url: '/People/Employment-Record',
            ParentFolder: v + 'People/', TemplateUrl: 'EmploymentRecord'
        },

        {
            Name: "PeopleInitialMigration", Url: '/People/Initial-Migration',
            ParentFolder: v + 'People/', TemplateUrl: 'InitialMigration'
        },

        {
            Name: "PeopleMaintenanceEmployeeStatus", Url: '/People/Maintenance/Employee-Status',
            ParentFolder: v + 'People/Maintenance/', TemplateUrl: 'EmployeeStatus'
        },

        {
            Name: "PeopleMaintenanceMovementType", Url: '/People/Maintenance/Movement-Type',
            ParentFolder: v + 'People/Maintenance/', TemplateUrl: 'MovementType'
        },

        {
            Name: "PeopleOrganizationCompany", Url: '/People/Organization/Company',
            ParentFolder: v + 'People/Organization/', TemplateUrl: 'Company'
        },

        {
            Name: "PeopleOrganizationCostCenter", Url: '/People/Organization/CostCenter',
            ParentFolder: v + 'People/Organization/', TemplateUrl: 'CostCenter'
        },

        {
            Name: "PeopleOrganizationOrgMasterList", Url: '/People/Organization/OrgMasterList',
            ParentFolder: v + 'People/Organization/', TemplateUrl: 'OrgMasterList'
        },

        ////////////////////PAYROLL
        {
            Name: "PayrollMaintenanceCurrency", Url: '/Payroll/Maintenance/Currency',
            ParentFolder: v + 'Payroll/Maintenance/', TemplateUrl: 'Currency'
        },

        {
            Name: "PayrollMaintenancePaymentMode", Url: '/Payroll/Maintenance/Payment-Mode',
            ParentFolder: v + 'Payroll/Maintenance/', TemplateUrl: 'PaymentMode'
        },

        {
            Name: "PayrollMaintenancePayrollClassification", Url: '/Payroll/Maintenance/Payroll-Classification',
            ParentFolder: v + 'Payroll/Maintenance/', TemplateUrl: 'PayrollClassification'
        },

        {
            Name: "PayrollMaintenancePayrollParameter", Url: '/Payroll/Maintenance/Payroll-Parameter',
            ParentFolder: v + 'Payroll/Maintenance/', TemplateUrl: 'PayrollParameter'
        },

        {
            Name: "PayrollMaintenanceTaxExemption", Url: '/Payroll/Maintenance/Tax-Exemption',
            ParentFolder: v + 'Payroll/Maintenance/', TemplateUrl: 'TaxExemption'
        },

        {
            Name: "PayrollMaintenancePayrollItem", Url: '/Payroll/Maintenance/Payroll-Item',
            ParentFolder: v + 'Payroll/Maintenance/', TemplateUrl: 'PayrollItem'
        },

        ////////////////////SUPPLEMENTARY SETTINGS
        {
            Name: "SupplementarySettingsBloodType", Url: '/Supplementary-Settings/Blood-Type',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'BloodType'
        },

        {
            Name: "SupplementarySettingsCitizenship", Url: '/Supplementary-Settings/Citizenship',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'Citizenship'
        },

        {
            Name: "SupplementarySettingsCivilStatus", Url: '/Supplementary-Settings/Civil-Status',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'CivilStatus'
        },

        {
            Name: "SupplementarySettingsEducationalAttainment", Url: '/Supplementary-Settings/Educational-Attainment',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'EducationalAttainment'
        },

        {
            Name: "SupplementarySettingsProvince", Url: '/Supplementary-Settings/Province',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'Province',
            Children: [
                { Controller: 'SupplementarySettingsCity', ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'CityRecord' }
            ]
        },

        {
            Name: "SupplementarySettingsNationality", Url: '/Supplementary-Settings/Nationality',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'Nationality'
        },

        {
            Name: "SupplementarySettingsReligion", Url: '/Supplementary-Settings/Religion',
            ParentFolder: v + 'SupplementarySettings/', TemplateUrl: 'Religion'
        },

        ////////////////////Training
        {
            Name: "TrainingTrainingSummary", Url: '/Training/Training-Summary',
            ParentFolder: v + 'Training/', TemplateUrl: 'TrainingSummary'
        }
    ];

    RegisterStates = function (menus) {
        Enumerable.From(menus).ForEach(function (menu) {
            var Name = menu.Name;
            var Controller = menu.Name;
            var MenuCode = menu.Name;
            var Url = menu.Url;
            var TemplateUrl = menu.ParentFolder + menu.TemplateUrl + '.html';
            var FormName = Name + '.Form';
            var TemplateUrlForm = menu.ParentFolder + menu.TemplateUrl + 'Record.html'; //Formulate the name path of physical form html

            $stateProvider.state({
                name: Name,
                url: Url,
                controller: Controller,
                templateUrl: TemplateUrl,
                resolve: {
                    rights: ['DataService', '$state', '$rootScope', function ($ds, $st, $rs) {
                        return $ds.Post('LoadRights', { Code: MenuCode }, 'Menu').then(function (ret) {
                            if (!ret.Data.HasView) $st.go('404')
                            else return ret;
                        })
                    }],
                    $priv: ['rights', function (data) {
                        return data;
                    }]
                }
            });

            $stateProvider.state({
                name: FormName,
                url: '/Form/{ID}',
                templateUrl: TemplateUrlForm
            });

            //Composed Routes for Approval
            if ('ForApproval' in menu) {
                var ApprovalTemplate = menu.ParentFolder + menu.TemplateUrl + 'ApproveRecord.html';
                var ApprovalForm = Name + '.ApprovalForm';
                if (menu.ForApproval) {
                    $stateProvider.state({
                        name: ApprovalForm,
                        url: '/ApprovalForm/{ID}',
                        templateUrl: ApprovalTemplate
                    })
                }
            }

            //Composed Routes for defined child

            if ('Children', menu) {
                Enumerable.From(menu.Children).ForEach(function(child) {
                    var ChildName = Name + '.Child'
                    var Controller = child.Controller;
                    var MenuCode = child.Controller;
                    var ParentFolder = child.ParentFolder;
                    var TemplateUrl = ParentFolder + child.TemplateUrl + '.html';
                    
                    $stateProvider.state({
                        name: ChildName,
                        url: '/Form/{ID}/{cID}',
                        controller: Controller,
                        templateUrl: TemplateUrl,
                        resolve: {
                            rights: ['DataService', '$state', '$rootScope', function ($ds, $st, $rs) {
                                return $ds.Post('LoadRights', { Code: MenuCode }, 'Menu').then(function (ret) {
                                    if (!ret.Data.HasView) $st.go('404')
                                    else return ret;
                                })
                            }],
                            $priv: ['rights', function (data) {
                                return data;
                            }]
                        }
                    });
                });
            }
        });
    }
    RegisterStates(menus);
    //Special Cases 
    

}]);