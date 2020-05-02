angular.module('app')
    .controller('AdministrativePolicy', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.TableSchema = [];
        $s.myController = 'Policy';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Name', Label: 'Policy' },
                { Name: 'Company' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Policy' },
                {
                    Name: 'ID_Company', Label: "Company", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: $s.myController,
                        method: 'LoadCompanyLookup',
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Company' }
                            ],
                            Filters: [
                                { Name: 'Company', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
            ]
        };

        $s.FilterSelectedCompany = {};
        $s.SetSelectedCompany = function () {
            $s.FilterSelectedCompany = {
                Filters: [
                    { Name: 'ID_Company', Value: $s.Schema.ID_Company, Type: 1 }
                ]
            }
            return $s.FilterSelectedCompany;
        }

        $s.CompanyList = [];
        $s.IsAllCompany = ($s.Session('ID_Company') == 0);
        $s.GetLookUp = function () {
            if ($s.IsAllCompany) {
                $s.Request('LoadCompanyLookup', {}, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.CompanyList = ret.Data.Rows;
                        $s.CompanyList.push({ ID: null, Name: '-Select-' });
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }
        $s.Init = function () {
            $s.initDetailOptions();
            $s.loadEmployee();
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.GetLookUp();
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.initDetailOptions();
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
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
        $s.SetSystemStatus('Loading table', 'loading');

        $s.changeTab = function (idx) {
            $('.lt-item').removeClass('active');
            $('#lt-' + idx).addClass('active');
            $('.content-tab').removeClass('active');
            $('#ct-' + idx).addClass('active');
            $('.dp-item').removeClass('active');
            $('#dp-lt-' + idx).addClass('active');
            $('#d-lt-' + idx).addClass('active');
        }
        
        $s.saveForm = function () {
            if (($s.employeeOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.employeeOptions.tblOptions.TableSchema, 'employee') : true) && 
                ($s.designationOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.designationOptions.tblOptions.TableSchema, 'designation') : true) && 
                ($s.jobclassOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.jobclassOptions.tblOptions.TableSchema, 'jobclass') : true) && 
                ($s.jobclassgroupOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.jobclassgroupOptions.tblOptions.TableSchema, 'jobclassgroup') : true) && 
                ($s.sectionOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.sectionOptions.tblOptions.TableSchema, 'section') : true) && 
                ($s.departmentOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.departmentOptions.tblOptions.TableSchema, 'department') : true) && 
                ($s.divisionOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.divisionOptions.tblOptions.TableSchema, 'division') : true) &&
                ($s.branchOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.branchOptions.tblOptions.TableSchema, 'branch') : true) && 
                ($s.companyOptions.tblOptions.Data.Rows.length > 0 ? $s.IsTabsValid('form.policy', $s.companyOptions.tblOptions.TableSchema, 'company') : true)) {
                $s.SetSystemStatus('Saving record policies', 'loading');
                
                $s.Request('SaveForm', {
                    Employee: $s.employeeOptions.tblOptions.Data.Rows,
                    Designation: $s.designationOptions.tblOptions.Data.Rows,
                    JobClass: $s.jobclassOptions.tblOptions.Data.Rows,
                    JobClassGroup: $s.jobclassgroupOptions.tblOptions.Data.Rows,
                    Section: $s.sectionOptions.tblOptions.Data.Rows,
                    Department: $s.departmentOptions.tblOptions.Data.Rows,
                    Division: $s.divisionOptions.tblOptions.Data.Rows,
                    Branch: $s.branchOptions.tblOptions.Data.Rows,
                    Company: $s.companyOptions.tblOptions.Data.Rows
                }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { }, { reload: true });
                    }
                });
            }
        }

        $s.saveFormMain = function () {
            if ($s.IsTabsValid('form.policylist')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveFormMain', { Data: $s.Schema }, $s.myController).then(function (ret) {
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

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController)
        }

        //detail options
        $s.employeeOptions = { Data: {Total: 0, Rows: []} };
        $s.designationOptions = { Data: { Total: 0, Rows: [] }};
        $s.jobclassOptions = { Data: { Total: 0, Rows: [] }};
        $s.jobclassgroupOptions = { Data: { Total: 0, Rows: [] }};
        $s.sectionOptions = { Data: { Total: 0, Rows: [] }};
        $s.departmentOptions = { Data: { Total: 0, Rows: [] }};
        $s.divisionOptions = { Data: { Total: 0, Rows: [] }};
        $s.branchOptions = { Data: { Total: 0, Rows: [] }};
        $s.companyOptions = { Data: { Total: 0, Rows: [] }};
        $s.initDetailOptions = function () {
            $s.employeeOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Employee', Label: 'Employee', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Employee' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadEmployeeLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Employee'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Employee', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteEmployee', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadEmployee',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.designationOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Designation', Label: 'Designation', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Designation' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadDesignationLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Designation'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Designation', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteDesignation', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadDesignation',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.jobclassOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_JobClass', Label: 'Job Class', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'JobClass' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadJobClassLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'JobClass'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'JobClass', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteJobClass', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadJobClass',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.jobclassgroupOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_JobClassGroup', Label: 'Job Class Group', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'JobClassGroup' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadJobClassGroupLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'JobClassGroup'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'JobClassGroup', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteJobClassGroup', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadJobClassGroup',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.sectionOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Section', Label: 'Section', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Section' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadSectionLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Section'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Section', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteSection', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadSection',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.departmentOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Department', Label: 'Department', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Department' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadDepartmentLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Department'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Department', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteDepartment', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadDepartment',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.divisionOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Division', Label: 'Division', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Division' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadDivisionLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Division'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Division', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteDivision', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadDivision',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.branchOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Branch', Label: 'Branch', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Branch' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadBranchLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Branch'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Branch', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteBranch', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadBranch',
                Controller: 'Policy',
                Parameter: {}
            };
            $s.companyOptions = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        {
                            Name: 'ID_Company', Label: 'Company', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Company' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadCompanyLookup',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Company'
                        },
                        {
                            Name: 'ID_Policy', Label: 'Policy', ControlType: 'lookup',
                            Options: {
                                tblOptions: {
                                    Columns: [
                                        { Name: 'ID', Label: '#' },
                                        { Name: 'Name', Label: 'Policy' }
                                    ],
                                    Filters: [
                                        { Name: 'Name', Type: 9 }
                                    ]
                                },
                                method: 'LoadList',
                                controller: 'Policy',
                                parameter: {},
                            },
                            DisplayContainer: 'Policy'
                        },
                        {
                            Name: 'Value', ControlType: 'text',
                        }
                    ],
                    Filters: [
                        { Name: 'Company', Type: 9, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteCompany', 'Policy', tblOptions);
                    },
                    isEditable: true,
                    hasOpenIcon: false,
                    hasOpenIconClick: function (row) { },
                    openForm: function () { }
                },
                Method: 'LoadCompany',
                Controller: 'Policy',
                Parameter: {}
            };
        }
        $s.loadEmployee = function () {
            if ($s.employeeOptions.tblOptions.Data != undefined)
                if ($s.employeeOptions.tblOptions.Data.Rows != undefined)
                    if ($s.employeeOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Employee' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.employeeOptions.tblOptions.TableSchema = ret2.Data;
                    $s.employeeOptions.tblOptions.HasNew = true;
                    $s.employeeOptions.tblOptions.HasEdit = true;
                    $s.employeeOptions.tblOptions.HasDelete = true;
                    $s.employeeOptions.trigger().then(function (tblOptions) {
                        $s.employeeOptions.tblOptions = tblOptions;
                        $s.employeeOptions.tblOptions.newForm = function () {
                            $s.employeeOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Employee: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadDesignation = function () {
            if ($s.designationOptions.tblOptions.Data != undefined)
                if ($s.designationOptions.tblOptions.Data.Rows != undefined)
                    if ($s.designationOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Designation' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.designationOptions.tblOptions.TableSchema = ret2.Data;
                    $s.designationOptions.tblOptions.HasNew = true;
                    $s.designationOptions.tblOptions.HasEdit = true;
                    $s.designationOptions.tblOptions.HasDelete = true;
                    $s.designationOptions.trigger().then(function (tblOptions) {
                        $s.designationOptions.tblOptions = tblOptions;
                        $s.designationOptions.tblOptions.newForm = function () {
                            $s.designationOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Designation: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadJobClass = function () {
            if ($s.jobclassOptions.tblOptions.Data != undefined)
                if ($s.jobclassOptions.tblOptions.Data.Rows != undefined)
                    if ($s.jobclassOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_JobClass' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.jobclassOptions.tblOptions.TableSchema = ret2.Data;
                    $s.jobclassOptions.tblOptions.HasNew = true;
                    $s.jobclassOptions.tblOptions.HasEdit = true;
                    $s.jobclassOptions.tblOptions.HasDelete = true;
                    $s.jobclassOptions.trigger().then(function (tblOptions) {
                        $s.jobclassOptions.tblOptions = tblOptions;
                        $s.jobclassOptions.tblOptions.newForm = function () {
                            $s.jobclassOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_JobClass: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadJobClassGroup = function () {
            if ($s.jobclassgroupOptions.tblOptions.Data != undefined)
                if ($s.jobclassgroupOptions.tblOptions.Data.Rows != undefined)
                    if ($s.jobclassgroupOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_JobClassGroup' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.jobclassgroupOptions.tblOptions.TableSchema = ret2.Data;
                    $s.jobclassgroupOptions.tblOptions.HasNew = true;
                    $s.jobclassgroupOptions.tblOptions.HasEdit = true;
                    $s.jobclassgroupOptions.tblOptions.HasDelete = true;
                    $s.jobclassgroupOptions.trigger().then(function (tblOptions) {
                        $s.jobclassgroupOptions.tblOptions = tblOptions;
                        $s.jobclassgroupOptions.tblOptions.newForm = function () {
                            $s.jobclassgroupOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_JobClassGroup: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadSection = function () {
            if ($s.sectionOptions.tblOptions.Data != undefined)
                if ($s.sectionOptions.tblOptions.Data.Rows != undefined)
                    if ($s.sectionOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Section' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.sectionOptions.tblOptions.TableSchema = ret2.Data;
                    $s.sectionOptions.tblOptions.HasNew = true;
                    $s.sectionOptions.tblOptions.HasEdit = true;
                    $s.sectionOptions.tblOptions.HasDelete = true;
                    $s.sectionOptions.trigger().then(function (tblOptions) {
                        $s.sectionOptions.tblOptions = tblOptions;
                        $s.sectionOptions.tblOptions.newForm = function () {
                            $s.sectionOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Section: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadDepartment = function () {
            if ($s.departmentOptions.tblOptions.Data != undefined)
                if ($s.departmentOptions.tblOptions.Data.Rows != undefined)
                    if ($s.departmentOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Department' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.departmentOptions.tblOptions.TableSchema = ret2.Data;
                    $s.departmentOptions.tblOptions.HasNew = true;
                    $s.departmentOptions.tblOptions.HasEdit = true;
                    $s.departmentOptions.tblOptions.HasDelete = true;
                    $s.departmentOptions.trigger().then(function (tblOptions) {
                        $s.departmentOptions.tblOptions = tblOptions;
                        $s.departmentOptions.tblOptions.newForm = function () {
                            $s.departmentOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Department: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadDivision = function () {
            if ($s.divisionOptions.tblOptions.Data != undefined)
                if ($s.divisionOptions.tblOptions.Data.Rows != undefined)
                    if ($s.divisionOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Division' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.divisionOptions.tblOptions.TableSchema = ret2.Data;
                    $s.divisionOptions.tblOptions.HasNew = true;
                    $s.divisionOptions.tblOptions.HasEdit = true;
                    $s.divisionOptions.tblOptions.HasDelete = true;
                    $s.divisionOptions.trigger().then(function (tblOptions) {
                        $s.divisionOptions.tblOptions = tblOptions;
                        $s.divisionOptions.tblOptions.newForm = function () {
                            $s.divisionOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Division: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadBranch = function () {
            if ($s.branchOptions.tblOptions.Data != undefined)
                if ($s.branchOptions.tblOptions.Data.Rows != undefined)
                    if ($s.branchOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Branch' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.branchOptions.tblOptions.TableSchema = ret2.Data;
                    $s.branchOptions.tblOptions.HasNew = true;
                    $s.branchOptions.tblOptions.HasEdit = true;
                    $s.branchOptions.tblOptions.HasDelete = true;
                    $s.branchOptions.trigger().then(function (tblOptions) {
                        $s.branchOptions.tblOptions = tblOptions;
                        $s.branchOptions.tblOptions.newForm = function () {
                            $s.branchOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Branch: null, Value: null
                            });
                        }
                    });
                }
            });
        }
        $s.loadCompany = function () {
            if ($s.companyOptions.tblOptions.Data != undefined)
                if ($s.companyOptions.tblOptions.Data.Rows != undefined)
                    if ($s.companyOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPolicy_Company' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.companyOptions.tblOptions.TableSchema = ret2.Data;
                    $s.companyOptions.tblOptions.HasNew = true;
                    $s.companyOptions.tblOptions.HasEdit = true;
                    $s.companyOptions.tblOptions.HasDelete = true;
                    $s.companyOptions.trigger().then(function (tblOptions) {
                        $s.companyOptions.tblOptions = tblOptions;
                        $s.companyOptions.tblOptions.newForm = function () {
                            $s.companyOptions.tblOptions.Data.Rows.unshift({
                                ID: 0, ID_Policy: null, ID_Company: $s.Schema.ID_Company, Value: null
                            });
                        }
                    });
                }
            });
        }

        $s.Init();
    }])