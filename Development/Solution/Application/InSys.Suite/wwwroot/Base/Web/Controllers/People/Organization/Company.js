angular.module('app')
    .controller('PeopleOrganizationCompany', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });

        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.CompanyChildren = [];
        $s.TableSchema = [];
        $s.MenuCode = 'PeopleOrganizationCompany';
        $s.CompanyOrg = {};
        $s.DeleteOrg = [];
        $s.jobClassList = [];
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code' },
                { Name: 'Name', Label: 'Company Name' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Value: null },
                { Name: 'Name', Type: 9, Value: null }
            ]
        };

        $s.Init = function () {
            if ($s.RecordID != '' || $s.Session('ID_Roles') > 1) {
                $s.SetSystemStatus('Loading record #' + $s.RecordID || $s.Session('ID_Company'), 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Company, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'Company', { MenuCode: $s.MenuCode }).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message);
                    } else {
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }
        $s.LoadFirstChild = function () {
            $s.Request('GetChildOrg', { IsCompany: true, ID: $s.Schema.ID, MenuCode: $s.MenuCode }, 'Company').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message);
                } else {
                    $s.CompanyOrg = ret.Data.CompanyChildren
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }
        var SelectedJobclass = [];
        $s.JobClassParentHeirarchy = [];
        $s.LoadJobClass = function (ID_Company) {
            $s.Request('LoadJobClass', { ID_Company: ID_Company, MenuCode: $s.MenuCode },'Company').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message);
                } else {
                    $s.jobClassList = ret.Data.Org;
                    SelectedJobclass = ret.Data.JobClassApproverCandidates;
                    $s.JobClassParentHeirarchy = ret.Data.JobClassParentHeirarchy;
                    Enumerable.From($s.jobClassList).ForEach(function (x) {
                        x.ParentList = $s.JobClassParentHeirarchy.filter(y => y.ID_Org == x.ID && y.ID != x.ID);
                    });
                    if (SelectedJobclass.length > 0) {
                        Enumerable.From(SelectedJobclass).ForEach(function (x) {
                            var z = $s.jobClassList.find(function (e) {
                                return e.ID == x.ID_Org;
                            });
                            z.IsChecked = true;

                        });
                    }
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }
        $s.companyOrgOptions = {
            Data: { Rows: [] },
            trigger: function () { },
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    {
                        Name: 'ID_OrgType', Label: 'Organizational Type', ControlType: 'lookup',
                        Options: {
                            tblOptions: {
                                Columns: [
                                    { Name: 'ID', Label: '#' },
                                    { Name: 'Name', Label: 'Organizational Type' }
                                ],
                                Filters: [
                                    { Name: 'Name', Type: 9 }
                                ]
                            },
                            method: 'LoadLookup',
                            controller: 'Company',
                            parameter: { Name: 'OrgType', MenuCode: $s.MenuCode },
                        },
                        DisplayContainer: 'OrgType'
                    },
                    { Name: 'SeqNo', Label: 'Sequence No.', ControlType: 'number' },
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteCompanyOrgLevel', 'Company', tblOptions);
                },
            }, 
            Method: 'LoadCompanyOrg',
            Controller: 'Company',
            isEditable: true,
            Parameter: { ID: ($s.RecordID || $s.Session('ID_Company')), MenuCode: $s.MenuCode }
        };

        $s.LoadOrgType = function () {
            if ($s.companyOrgOptions.tblOptions.Data != undefined)
                if ($s.companyOrgOptions.tblOptions.Data.Rows != undefined)
                    if ($s.companyOrgOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tCompanyOrgType' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.companyOrgOptions.tblOptions.TableSchema = ret2.Data;
                    $s.companyOrgOptions.tblOptions.HasNew = true;
                    $s.companyOrgOptions.tblOptions.HasEdit = true;
                    $s.companyOrgOptions.tblOptions.HasDelete = true;
                    $s.companyOrgOptions.trigger().then(function (tblOptions) {
                        $s.companyOrgOptions.tblOptions = tblOptions;
                        $s.companyOrgOptions.tblOptions.newForm = function () {
                            var data = $s.PlotDefault({}, ret2.Data, 0);
                            $s.companyOrgOptions.tblOptions.Data.Rows.unshift(data);
                        }
                    });
                }
            });
        }
        
        $s.Delete = function () {
            if ($s.DeleteOrg.length > 0) {
                $s.Confirm('Are you sure you want to delete ' + $s.DeleteOrg.length + ' record(s)?', 'Delete').then(function () {
                    $s.Request('DeleteSelectedHeirarch', { Data: $s.DeleteOrg, MenuCode: $s.MenuCode }, 'Company').then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message);
                        } else {
                            $s.Prompt(ret.Message);
                            $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, {}, { reload: true });
                        }
                    });
                });
            }
        }
        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: ($s.RecordID || $s.Session('ID_Company')), MenuCode: $s.MenuCode }, 'Company');
        }

        $s.AddCompanyLevel = function (ID_Company) {
            return $s.Dialog({
                template: 'CompanyLevel',
                controller: 'CompanyLevel',
                size: 'sm',
                windowClass: 'company-lvl-dlg',
                data: { ID_Company: ID_Company || $s.Session('ID_Company') }
            });
        }
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.company') && $s.IsTabsValid('form.orgtype')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                var CompanyOrgTypes = $s.companyOrgOptions.tblOptions.Data != undefined ? $s.companyOrgOptions.tblOptions.Data.Rows : [];
                var jobclass = $s.jobClassList.filter(x => x.IsChecked == true);
                var removedJobclassApproverCandidates = $s.jobClassList.filter(x => x.IsChecked == false || x.IsChecked == undefined);
                $s.Request('SaveForm', { Data: $s.Schema, JobClassApproverCandidates: jobclass, RemovedJobClassApproverCandidates: removedJobclassApproverCandidates, CompanyOrgType: CompanyOrgTypes, MenuCode: $s.MenuCode }, 'Company').then(function (ret) {
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
            $s.deleteRow('DeleteRecord', 'Company', { MenuCode: $s.MenuCode })
        }
        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

    }])
    .controller('CompanyLevel', ['$scope', '$controller','dData','$uibModalInstance', function ($s, $c,$dlg,$modal) {
        $c('BaseController', { $scope: $s });
        $s.CompanyOrgType = {};
        $s.OrgTypeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Name' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'Company',
            parameter: { Name:"OrgType", MenuCode: 'PeopleOrganizationCompany' }
        };
        $s.Submit = function () {
            $s.CompanyOrgType.ID_Company = $dlg.ID_Company;
            $s.Request('SaveCompanyOrgType', { Data: $s.CompanyOrgType, MenuCode: 'PeopleOrganizationCompany' }, 'Company').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.CompanyOrgType = ret.Data;
                    $s.Prompt('Save successfully.', 'Success');
                    $s.$apply();
                }
            });
            
        }
        $s.Close = function () {
            $modal.dismiss();
        }
    }]);