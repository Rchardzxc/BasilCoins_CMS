angular.module('app')
    .controller('PeopleEmploymentRecord', ['$scope', '$controller', '$priv', '$state', '$rootScope', function ($s, $c, $priv, $st, $rs) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'EmploymentRecord';
        $s.Schema = {};
        $s.TableSchema = [];
        $s.TrainingBondSchema = [];
        $s.AttachmentTabSchema = [];

        //Persona Objects
        $s.Persona = {};
        $s.CharacterReference = [];
        $s.EducationalBackGround = [];
        $s.LicensesAndCertificates = [];
        $s.PreviousEmployment = [];
        $s.OrgChart = [];


        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', },
                { Name: 'LastName', Label: 'Last Name' },
                { Name: 'FirstName', Label: 'First Name' },
                { Name: 'MiddleInitial', Label: 'Middle Initial' },
                { Name: 'Company' },
                { Name: 'Branch' },
                { Name: 'Department' },
                { Name: 'Designation', Label: 'Position' },
                { Name: 'EmployeeStatus', Label: 'Status' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'LastName', Type: 9, Label: 'Last Name' },
                { Name: 'FirstName', Type: 9, Label: 'First Name' },
                { Name: 'MiddleName', Type: 9, Label: 'Middle Name' },
                {
                    Name: 'ID_Company', Label: "Company", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'GetLookUp',
                        method: 'LoadLookUp',
                        parameter: { Name: 'Company' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Company' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
                {
                    Name: 'ID_Branch', Label: "Branch", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'GetLookUp',
                        method: 'LoadLookUp',
                        parameter: { Name: 'Branch' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Branch' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
                {
                    Name: 'ID_Department', Label: "Department", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'GetLookUp',
                        method: 'LoadLookUp',
                        parameter: { Name: 'Department' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Department' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
                {
                    Name: 'ID_Designation', Label: "Position", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'GetLookUp',
                        method: 'LoadLookUp',
                        parameter: { Name: 'Position' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Position' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
                {
                    Name: 'ID_EmployeeStatus', Label: "Status", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'GetLookUp',
                        method: 'LoadLookUp',
                        parameter: { Name: 'EmployeeStatus' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Status' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
            ]
        };

        //////Persona Wizard
        $s.OpenPersonaWizard = function () {

            $s.Dialog({
                template: 'PersonaSelect',
                controller: 'dlgPersonaSelect',
                size: 'sm',
                windowClass: 'persona-select-dlg', //Please create new windowClass
            }).result.then(function (ret) {
                
                if (ret != undefined) {
                    if (ret == 0) {
                        $s.Dialog({
                            template: 'PersonaWizard',
                            controller: 'dlgPersonaWizard',
                            size: 'lg',
                            windowClass: 'employee-wizard-dlg er-wizard-dlg', //Please create new windowClass
                            data: {}
                        }).result.then(function (ret) {
                            if (ret != undefined) {
                                $s.Persona = ret.Persona;
                                $s.CharacterReference = ret.CharacterReference;
                                $s.EducationalBackGround = ret.EducationalBackGround;
                                $s.LicensesAndCertificates = ret.LicensesAndCertificates;
                                $s.PreviousEmployment = ret.PreviousEmployment;

                                $s.Schema.FirstName = $s.Persona.FirstName;
                                $s.Schema.LastName = $s.Persona.LastName;
                            }
                        });
                    } else if (ret == -1) { //Since this is close, then redirect to listing
                        //redirect to list.
                        $st.go("PeopleEmploymentRecord", {}, { reload: true });
                    } else {
                        $s.Schema.ID_Persona = ret.ID;
                        $s.Schema.FirstName = ret.FirstName;
                        $s.Schema.LastName = ret.LastName;
                    }
                }
            });
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
                
                //New Employment, show wizard to insert Persona data
                if ($s.RecordID == 0) {
                    $s.OpenPersonaWizard();
                }

            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message);
                    } else {
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID }, $s.myController);
        }

        $s.ValidateBeforeSave = function () {
            var msg = '';
            msg += $s.ValidateTrainingBond();
            return msg;
        }

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.employmentrecord')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');

                var countError = 0;
                var errorMsg = "";
                Enumerable.From($s.FilingModulesSelected).ForEach(function (fm) {
                    if (fm.Default.length == 0) {
                        countError += 1;
                        errorMsg = 'Atleast 1 approver is required for module ' + fm.Name + '.';
                        return;
                    } else {
                        Enumerable.From(fm.Default).ForEach(function (approvers, idx) {
                            approvers.ID_Level = idx + 1;
                            if (approvers.ID_Employee == null || approvers.ID_Employee == undefined) {
                                countError += 1;
                                errorMsg = 'Approver 1 is required.';
                                return;
                            }
                        })
                    }
                });
                if (countError > 0) {
                    $s.Prompt(errorMsg);
                    $s.SetSystemStatus('Ready');
                    return;
                } else {
                    //TRAINING BOND TAB VALIDATION
                    if ($s.TrainingBondSchema.length > 0) {
                        if (!$s.IsTabsValid('form.employmentrecord', $s.TrainingBondSchema, 'trainingbond'))
                            return;
                    }
                    //ATTACHMENTS TAB VALIDATION
                    if ($s.AttachmentTabSchema.length > 0) {
                        if (!$s.IsTabsValid('form.employmentrecord', $s.AttachmentTabSchema, 'attachments'))
                            return;
                    }

                    var validatedMessage = $s.ValidateBeforeSave();
                    if (validatedMessage != '') {
                        $s.Prompt(validatedMessage);
                        $s.SetSystemStatus(validatedMessage, 'error');
                        return;
                    }
                    $s.Request('SaveForm', {
                        Data: $s.Schema,
                        FilingModules: $s.FilingModulesSelected,
                        TrainingBondRecord: $s.TrainingBondRecord,
                        DeletedTrainingBond: $s.DeletedTrainingBondRecords,
                        AttachmentRecord: $s.EmployeeAttachmentRecord,
                        DeletedAttachmentRecord: $s.DeletedAttachmentRecords,
                        Persona: $s.Persona,
                        CharacterReference: $s.CharacterReference,
                        EducationalBackGround: $s.EducationalBackGround,
                        LicensesAndCertificates: $s.LicensesAndCertificates,
                        PreviousEmployment: $s.PreviousEmployment
                    }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                            $s.$apply();
                        } else {
                            $s.SetSystemStatus('Successfully Saved.', 'success', true);
                            $s.SetDirtyFormToFalse($s.form);
                            $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                        }
                    });
                }
            }
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController)
        }

        $s.Val = function (val) {
            return val || '-';
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();


        $s.OrgChart.push({ Id: 1, Caption: 'Company', Field: 'ID_Org_Company', Field2: 'Company' });
        $s.OrgChart.push({ Id: 2, Caption: 'Branch', Field: 'ID_Org_Branch', Field2: 'Branch' });
        $s.OrgChart.push({ Id: 3, Caption: 'Division', Field: 'ID_Org_Division', Field2: 'Division' });
        $s.OrgChart.push({ Id: 4, Caption: 'Department', Field: 'ID_Org_Department', Field2: 'Department' });
        $s.OrgChart.push({ Id: 5, Caption: 'Section', Field: 'ID_Org_Section', Field2: 'Section' });
        $s.OrgChart.push({ Id: 6, Caption: 'Job Class Group', Field: 'ID_Org_JobClassGroup', Field2: 'JobClassGroup' });
        $s.OrgChart.push({ Id: 7, Caption: 'Job Class', Field: 'ID_Org_JobClass', Field2: 'JobClass' });
        $s.OrgChart.push({ Id: 8, Caption: 'Designation', Field: 'ID_Org_Designation', Field2: 'Designation' });

        ///LOAD EMPLOYEE LOOKUP
        
        $s.companyLookup = {};
        $s.branchLookup = {};
        $s.divisionLookup = {};
        $s.departmentLookup = {};
        $s.sectionLookup = {};
        $s.jobClassGroupLookup = {};
        $s.jobClassLookup = {};
        $s.designationLookup = {};
        
        $s.companyLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Company' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Company' }
        };

        $s.branchLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Branch' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Branch' }
        };

        $s.RefreshLookUp = function () {
            $s.divisionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Division' }
                    ]
                },
                method: 'LoadLookUp',
                controller: 'EmploymentRecord',
                parameter: { Name: 'Division', ParentId: $s.Schema.ID_Branch }
            }

            $s.departmentLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Department' }
                    ]
                },
                method: 'LoadLookUp',
                controller: 'EmploymentRecord',
                parameter: { Name: 'Department', ParentId: $s.Schema.ID_Org_Division }
            }

            $s.sectionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Section' }
                    ]
                },
                method: 'LoadLookUp',
                controller: 'EmploymentRecord',
                parameter: { Name: 'Section', ParentId: $s.Schema.ID_Org_Department }
            }

            $s.jobClassGroupLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Job Class Group' }
                    ]
                },
                method: 'LoadLookUp',
                controller: 'EmploymentRecord',
                parameter: { Name: 'JobClassGroup', ParentId: $s.Schema.ID_Org_Section }
            };

            $s.jobClassLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Job Class' }
                    ]
                },
                method: 'LoadLookUp',
                controller: 'EmploymentRecord',
                parameter: { Name: 'JobClass', ParentId: $s.Schema.ID_Org_JobClassGroup }
            };

            $s.designationLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Designation' }
                    ]
                },
                method: 'LoadLookUp',
                controller: 'EmploymentRecord',
                parameter: { Name: 'Designation', ParentId: $s.Schema.ID_Org_JobClass }
            };
        }


        $s.RefreshLookUp();

        $s.CurrencyLookup = {};
        $s.PayrollSchemeLookup = {};
        $s.PayrollFrequencyLookup = {};
        $s.PaymentModeLookup = {};
        $s.ParameterLookup = {};
        $s.LeaveParameterLookup = {};
        $s.PayrollStatusLookup = {};
        $s.PayrollClassificationLookup = {};
        $s.AccountNumberTypeLookup = {};
        $s.CompanyBankAcctLookup = {};
        $s.TaxExemptionLookup = {};

        $s.CurrencyLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Currency' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Currency' }
        };

        $s.PayrollSchemeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Payroll Scheme' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'PayrollScheme' }
        };


        $s.PayrollFrequencyLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Payroll Frequency' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'PayrollFrequency' }
        };


        $s.PaymentModeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Payment Mode' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'PaymentMode' }
        };

        $s.ParameterLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Parameter' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Parameter' }
        };

        $s.LeaveParameterLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Leave Parameter' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'LeaveParameter' }
        };

        $s.PayrollStatusLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Payroll Status' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'PayrollStatus' }
        };

        $s.PayrollClassificationLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Payroll Classification' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'PayrollClassification' }
        };

        $s.AccountNumberTypeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Account Number Type' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'AccountNumberType' }
        };

        $s.CompanyBankAcctLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Company Bank Acct' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'CompanyBankAcct' }
        };

        $s.TaxExemptionLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Tax Exemption' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'TaxExemption' }
        };

        
        $s.WeeklyScheduleLookup = {};
        $s.CostCenterLookup = {};
        $s.EmployeeStatusLookup = {};


        $s.WeeklyScheduleLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Weekly Schedule' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'WeeklySchedule' }
        };


        $s.CostCenterLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Cost Center' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'CostCenter' }
        };

        $s.EmployeeStatusLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Employee Status' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'EmployeeStatus' }
        };
       
        //APPROVER TAB
        $s.FilingModulesSelected = [];
        $s.CurrentFilingModule = null;

        $s.LoadEmployeeFilingModules = function () {
            if ($s.FilingModulesSelected.length > 0) return;
            $s.SetSystemStatus('Loading employee modules', 'loading');
            $s.Request('LoadEmployeeFilingModules', { ID: $s.RecordID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.FilingModulesSelected = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.openFilingModules = function () {
            $s.Dialog({
                template: 'FilingModules',
                controller: 'dlgFilingModules',
                size: 'md',
                windowClass: 'filingModules-dlg',
                data: { ID: $s.RecordID, Data: $s.FilingModulesSelected }
            }).result.then(function (ret) {
                if (ret != undefined) {
                    $s.FilingModulesSelected = ret;
                    $s.CurrentFilingModule = null;
                    $s.ApproverSelected = [];
                }
            });
        }

        $s.removeModule = function (mod) {
            if ($s.CurrentFilingModule == mod.ID_FilingModules) $s.CurrentFilingModule = null; $s.ApproverSelected = [];
            var idx = Enumerable.From($s.FilingModulesSelected).Select(x => x.ID_FilingModules).IndexOf(mod.ID_FilingModules);
            if ($s.FilingModulesSelected[idx].ID_FilingModules == $s.CurrentFilingModule.ID_FilingModules) {
                $s.CurrentFilingModule = null;
            }
            if ($s.Schema.DeletedModules == undefined) $s.Schema.DeletedModules = [];
            if (mod.ID > 0) $s.Schema.DeletedModules.push(mod.ID);
            $s.FilingModulesSelected.splice(idx, 1);
        }

        $s.LoadApproverSchema = function () {
            if ($s.CurrentFilingModule == null) {
                $s.SetSystemStatus('Please select a Filing Type.', 'warning');
                return;
            }
            if ($s.CurrentFilingModule.Default == undefined) $s.CurrentFilingModule.Default = [];
            if ($s.CurrentFilingModule.Default.length < 5) {
                $s.Request('LoadApproverSchema', {}, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        var sc = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, 0);
                        sc.ID_Level = $s.CurrentFilingModule.Default.length + 1;
                        sc.ID_Approver_Employee_Module = $s.CurrentFilingModule.ID;
                        $s.CurrentFilingModule.Default.push(sc);
                    }
                });
            } else {
                $s.SetSystemStatus('Max approver level reached.', 'warning');
            }
        }

        $s.loadApprovers = function (mod, idx) {
            $s.CurrentFilingModule = mod;
            $('.am-filing-item').removeClass('selected');
            $('#afi-' + idx).addClass('selected');
            $('#afim-' + idx).addClass('selected');

            if (mod.ID == 0) return;
            if (mod.Default != undefined && mod.Default.length > 0) return;
            $s.SetSystemStatus('Loading approvers for ' + mod.Name, 'loading')
            $s.Request('LoadApproverByModule', { ID: mod.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.CurrentFilingModule.Default = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
            })
        }

        $s.AddApprover = function (targetColumn, model) {
            $s.Dialog({
                template: 'EmployeeList',
                controller: 'dlgEmployeeList',
                size: 'md',
                windowClass: 'select-approver-dlg',
                data: { Data: $s.ApproverSelected }
            }).result.then(function (ret) {
                if (ret != undefined) {
                    model[targetColumn] = ret.ID;
                    model[targetColumn.substr(3)] = ret.Name;
                }
            });
        }

        $s.RemoveApprover = function (targetColumn, model) {
            model[targetColumn] = null;
            model[targetColumn.substr(3)] = null;
        }

        $s.RemoveDefaultApprover = function (idx) {
            var deleted = $s.CurrentFilingModule.Default[idx];
            if ($s.CurrentFilingModule.DeletedApprovers == undefined) $s.CurrentFilingModule.DeletedApprovers = [];
            if (deleted.ID > 0) $s.CurrentFilingModule.DeletedApprovers.push(deleted.ID);
            $s.CurrentFilingModule.Default.splice(idx, 1);
        }

        $s.LoadApproverTemplate = function () {
            $s.Dialog({
                template: 'ApproverTemplate',
                controller: 'dlgApproverTemplate',
                size: 'md',
                windowClass: 'select-approver-dlg'
            }).result.then(function (ret) {
                if (ret != undefined) {
                    if ($s.FilingModulesSelected.length > 0) {
                        $s.Confirm('Your current setup for the approvers will be cleared and replace with this template. Are you sure you want to proceed?').then(function (rr) {
                            $s.SetSystemStatus('Loading template', 'loading');
                            $s.Request('LoadSelectedApproverTemplate', { ID: ret.ID, ID_Employee: $s.RecordID }, $s.myController).then(function (ret) {
                                if (ret.Type == 2) {
                                    $s.SetSystemStatus(ret.Message, 'error');
                                } else {
                                    var oldModules = Enumerable.From($s.FilingModulesSelected).Select(x => x.ID).ToArray();
                                    if ($s.Schema.DeletedModules == undefined) $s.Schema.DeletedModules = [];
                                    $s.Schema.DeletedModules = $s.Schema.DeletedModules.concat(oldModules);
                                    $s.FilingModulesSelected = ret.Data;
                                    $s.SetSystemStatus('Ready');
                                    $s.CurrentFilingModule = null;
                                }
                                $s.$apply();
                            });
                        });
                    } else {
                        $s.SetSystemStatus('Loading template', 'loading');
                        $s.Request('LoadSelectedApproverTemplate', { ID: ret.ID, ID_Employee: $s.RecordID }, $s.myController).then(function (ret) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                            } else {
                                $s.FilingModulesSelected = ret.Data;
                                $s.SetSystemStatus('Ready');
                                $s.CurrentFilingModule = null;
                            }
                            $s.$apply();
                        });
                    }
                }
            });
        }

        /////////TRAINING BOND
        $s.IsTrainingBondClicked = 0;
        $s.TrainingBondRecord = [];
        $s.DeletedTrainingBondRecords = [];

        $s.newTrainingBondRecord = function () {
            var newRecord = {
                ID: 0
                , ID_Employee: $s.Schema.ID
                , ID_Training: null
                , NumberOfMonths: 0
                , StartDate: new Date()
                , EndDate: new Date()
                , IsActive: true
                , IsCancelled: false
            };
            $s.TrainingBondRecord.push(newRecord);
        }

        $s.SelectAllTrainingBondRows = false;
        $s.CheckAllTrainingBondRecord = function () {
            $s.SelectAllTrainingBondRows = !$s.SelectAllTrainingBondRows;
            Enumerable.From($s.TrainingBondRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllTrainingBondRows;
            });
        }

        $s.delTrainingBondRecord = function () {

            var rows = Enumerable
                .From($s.TrainingBondRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.TrainingBondRecord.indexOf(rows[y]);
                        if ($s.TrainingBondRecord[Index].ID > 0) {
                            $s.DeletedTrainingBondRecords.push($s.TrainingBondRecord[Index].ID);
                        }
                        $s.TrainingBondRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.TrainingRecord = [];
        $s.LoadTrainingBond = function () {

            if ($s.IsTrainingBondClicked == 1) return;

            $s.Request('LoadLookUp', { Name: 'Training' }, 'GetLookUp').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.TrainingRecord = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
            $s.Request('LoadTrainingBond', { ID_Employee: $s.RecordID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.TrainingBondRecord = ret.Data.Rows;
                    $s.TrainingBondSchema = ret.Data.Schema;
                    $s.SetSystemStatus('Ready');
                    $s.CurrentFilingModule = null;
                }
                $s.$apply();
            });

            $s.IsTrainingBondClicked = 1;
        }

        $s.CountNumberOfMonths = function (_fromDate, _toDate, _data) {
            //ValidateDate
            if (_fromDate == null || _toDate == null) return;

            var fromDate = new Date(_fromDate);
            var toDate = new Date(_toDate);

            var usrDate = fromDate;
            var curDate = toDate;
            var usrYear, usrMonth = usrDate.getMonth() + 1;
            var curYear, curMonth = curDate.getMonth() + 1;
            if ((usrYear = usrDate.getFullYear()) < (curYear = curDate.getFullYear())) {
                curMonth += (curYear - usrYear) * 12;
            }
            var months = curMonth - usrMonth;
            if (usrDate.getDate() > curDate.getDate()) months--;


            _data.NumberOfMonths = months < 0 ? 0 : months;
        }

        $s.ValidateTrainingBond = function () {
            var _TrainingBondRecord = angular.copy($s.TrainingBondRecord);
            var ConflictRows = "";
            var msg = "";
            var y = 1; //index

            _TrainingBondRecord.forEach(function (x) {
                var StartDate = new Date(x.StartDate);
                var EndDate = new Date(x.EndDate);

                if (StartDate > EndDate) { ConflictRows += y.toString() + ", "; }
                y += 1;

            });

            if (ConflictRows != "") {
                msg = "Rows#[" + ConflictRows.substring(0, ConflictRows.length - 2) + "]: Start Date is greater than End Date.";
            }
            return msg;
        }

        ///Employee Training Bond Report

        $s.ViewEmployeeTrainingBond = function () {

            if (($s.TrainingBondRecord.length == 0)
                || ($s.TrainingBondRecord.filter(x => { return x.ID_Training > 0; }).length == 0)) {
                $s.Prompt("No Training Record Found");
                return;
            }

            $s.Dialog({
                template: 'InSysReport',
                controller: ['$scope', '$uibModalInstance', function ($ss, $mi) {
                    $ss.close = function () {
                        $mi.dismiss();
                    }
                    $ss.rptOptions = {
                        Pdf:
                        {
                            Method: 'LoadTrainingBondReport',
                            Parameter: { ID: $s.Schema.ID, Source: 'EmploymentRecord' }, //Source is used to check if report is for specific employee
                            Controller: 'TrainingSummary'
                        },
                        hasExportXls: false,
                        hasLoadPdf: false
                    };
                    $ss.$watch('rptOptions.reportReady', function (nv, ov) {
                        if (nv == true) {
                            $ss.rptOptions.triggerLoadPdf();
                        }
                    })
                }],
                windowClass: ' insys-report-dlg',
                size: 'lg'
            })
        }

        /////////////////EMPLOYEE ATTACHMENTS
        $s.EmployeeAttachmentRecord = [];
        $s.IsEmployeeAttachmentTabClicked = 0;

        $s.LoadAttachments = function () {

            if ($s.IsEmployeeAttachmentTabClicked == 1) return;

            $s.Request('LoadAttachments', { ID_Employee: $s.Schema.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.EmployeeAttachmentRecord = ret.Data.Rows;
                    $s.GetTableSchema('tEmployeeAttachments').then(function (ret) {
                        $s.AttachmentTabSchema = ret.Data;
                    });
                    $s.SetSystemStatus('Ready');
                }
            });
            $s.IsEmployeeAttachmentTabClicked = 1;
        }

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        $s.newAttachmentRecord = function () {
            var newRec = {
                ID_Employee: $s.Schema.ID,
                Name: '',
                Notes: '',
                CreationDate: null,
                Attachments: null
            };
            $s.EmployeeAttachmentRecord.push(newRec);
        }

        $s.SelectAllAttachmentRows = false;
        $s.CheckAllAttachmentRows = function () {
            $s.SelectAllAttachmentRows = !$s.SelectAllAttachmentRows;
            Enumerable.From($s.EmployeeAttachmentRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllAttachmentRows;
            });
        }

        $s.DeletedAttachmentRecords = [];
        $s.delAttachmentRecord = function () {

            var rows = Enumerable
                .From($s.EmployeeAttachmentRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.EmployeeAttachmentRecord.indexOf(rows[y]);

                        if ($s.EmployeeAttachmentRecord[Index].Id > 0) {
                            $s.DeletedAttachmentRecords.push($s.EmployeeAttachmentRecord[Index].Id);
                        }
                        $s.EmployeeAttachmentRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.getSchema = function (num, row) {
            
            if (num == 2) $s.divisionLookup.parameter.ParentId = row.ID;
            if (num == 3) $s.departmentLookup.parameter.ParentId = row.ID;
            if (num == 4) $s.sectionLookup.parameter.ParentId = row.ID;
            if (num == 5) $s.jobClassGroupLookup.parameter.ParentId = row.ID;
            if (num == 6) $s.jobClassLookup.parameter.ParentId = row.ID;
            if (num == 7) $s.designationLookup.parameter.ParentId = row.ID;

            var current_field = $s.OrgChart.filter(y => { return num == y.Id })[0].Field;

            if ($s.Schema[current_field] == row.ID) return;

            for (var x = (num + 1); x <= $s.OrgChart.length; x++) {
                var Field = $s.OrgChart.filter(y => { return x == y.Id })[0].Field;
                var Field2 = $s.OrgChart.filter(y => { return x == y.Id })[0].Field2;
                console.log(row);
                $s.Schema[Field] = null;
                $s.Schema[Field2] = '';
            }
        }



    }])
    .controller('dlgFilingModules', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });
        $s.FilingModules = [];
        $s.Request('LoadFilingModules', {}, 'EmploymentRecord').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message, 'error');
                $s.$apply();
            } else {
                $s.FilingModules = Enumerable.From(ret.Data).Select(x => {
                    return $s.IsExist(x)
                }).ToArray();
            }
        });

        $s.countSelected = function () {
            var a = Enumerable.From($s.FilingModules).Count(x => x.IsChecked);
            return a;
        }

        $s.IsExist = function (mod) {
            var existingData = Enumerable.From($dlgData.Data).Where(x => x.ID_FilingModules == mod.ID).FirstOrDefault();
            var nData = {};
            if (existingData == undefined) {
                nData = { IsChecked: false, ID: 0, ID_FilingModules: mod.ID, ID_Employee: $dlgData.ID, IsActive: true, Name: mod.Name, Default: [] };
            } else {
                nData = { IsChecked: true, ID: existingData.ID || 0, ID_FilingModules: existingData.ID_FilingModules || mod.ID, ID_Employee: existingData.ID_Employee || $dlgData.ID, IsActive: true, Name: mod.Name, Default: existingData.Default || [] };
            }
            return nData;
        }

        $s.load = function () {
            var selected = Enumerable.From($s.FilingModules).Where(x => x.IsChecked == true).ToArray();
            $mi.close(selected);
        }

        $s.close = function () {
            $mi.close();
        }
    }])
    .controller('dlgEmployeeList', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });

        $s.tblOptions = {
            Columns: [
                { Name: 'Name' }
            ]
        };
        $s.LoadTable($s.tblOptions, 'LoadEmployeeList', 'EmploymentRecord').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message);
            } else {
                $s.SetSystemStatus('Ready');
            }
            $s.$apply();
        });

        $s.openForm = function (emp) {
            $mi.close(emp)
        }

        $s.close = function () {
            $mi.close();
        }
    }])
    .controller('dlgApproverTemplate', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.tblOptions = {
            Columns: [
                { Name: 'Name' }
            ]
        };
        $s.LoadTable($s.tblOptions, 'LoadApproverTemplate', 'EmploymentRecord').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message);
            } else {
                $s.SetSystemStatus('Ready');
            }
            $s.$apply();
        });

        $s.openForm = function (emp) {
            $mi.close(emp)
        }

        $s.close = function () {
            $mi.close();
        }
    }]);


//.controller('dlgPersonaSelect', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
//    $c('insysTableController', { $scope: $s });

//    $s.PersonaWithoutEmploymentList = [];
//    $s.Schema = {};
//    $s.IsSelectEmployee = false;

//    $s.PersonaLookUp = {
//        tblOptions: {
//            Columns: [
//                { Name: 'ID', Label: '#' }
//                , { Name: 'Name', Label: 'Employee' }
//                , { Name: 'FirstName', Label: 'Firstname' }
//                , { Name: 'LastName', Label: 'Lastname' }
//            ]
//        },
//        method: 'LoadPersonaWithoutEmployment',
//        controller: 'EmploymentRecord',
//        parameter: {}
//    };

//    $s.getSchema = function (ret) {
//        $s.Schema = ret;
//    }

//    $s.Select = function () {
//        $mi.close($s.Schema);
//    }

//    $s.CreateNew = function () {
//        $mi.close(0);
//    }

//    $s.close = function () {
//        $mi.close(-1);
//    }

//}])