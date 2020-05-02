angular.module('app')
    .controller('AdministrativeApproverMatrix', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.TableSchema = [];
        $s.Employees = [];
        $s.EmployeeTmpList = [];
        $s.FilingModuleApproverSchema = {};
        $s.FilingModuleEmployees = [];

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Name' },
                { Name: 'IsActive', Label: 'Active' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' },
            ]
        };

        $s.MyController = 'ApproverMatrix';
        $s.MCode = 'AdministrativeApproverMatrix';
        $s.FilingModulesSelected = [];
        $s.CurrentFilingModule = null;
        $s.BaseFilingModule = [];

        //lookupconfig
        $s.companyLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Company' }
                ]
            },
            method: 'LoadCompanyList',
            controller: $s.MyController
        };
        $s.updateCompanyLookup = function (data) {
            $s.Schema.ID_Company = data.ID;
            $s.Schema.Company = data.Name;
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
                        $s.FilingModulesSelected = ret.Data.FilingModules;
                        $s.Employees = ret.Data.AllEmployees;
                        $s.EmployeeTmpList = angular.copy($s.Employees);
                        $s.FilingModuleApproverSchema = ret.Data.FilingModuleApproverSchema
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                    if (ret.Type != 2) {
                        $s.FilingModulesSelected.map(x => {
                            var data = x;
                            data.Default = $s.Employees.filter(e => { return e.ID_Menus == x.ID_Menus; });
                            $s.BaseFilingModule.push(data);
                        });
                        $s.LoadApprovers($s.BaseFilingModule[0].ID);
                    }
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.MyController, { MenuCode: $s.MCode }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MCode }, $s.MyController);
        }

        $s.saveForm = function () {
            var _FilingModuleApprovers = [];
            $s.PostToBaseFilingModule();

            $s.BaseFilingModule.map(x => {
                if (x.Default != undefined || x.Default != null || x.Default != []) {
                    x.Default.map(e => {
                        if (e.ID_Employee != null || e.ID_Employee2 != null || e.ID_Employee3 != null) {
                            //convert array value into object value.
                            _FilingModuleApprovers.push(($.isArray(e) ? Object.assign({}, e) : e));
                        }
                    });
                }
            });

            $s.Request('SaveForm', {
                Data: $s.Schema,
                FilingModuleApprovers: _FilingModuleApprovers,
                FilingModuleEmployees: $s.FilingModuleEmployees,
                RemoveFilingModuleEmployees: $s.RemoveEmployeeLists,
                MenuCode: $s.MCode
            }, $s.MyController).then(function (ret) {
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

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.MyController, { MenuCode: $s.MCode })
        }


        $s.LoadApproverSchema = function () {
            if ($s.CurrentFilingModule == null) {
                $s.SetSystemStatus('Please select a Filing Type.', 'warning');
                return;
            }
            if ($s.CurrentFilingModule.Default == undefined) $s.CurrentFilingModule.Default = [];

            if ($s.CurrentFilingModule.Default.length < 4) {
                var sc = $s.PlotDefault([], $s.FilingModuleApproverSchema, 0);
                sc.ID_Approver = $s.Schema.ID;
                sc.ID_FilingModules = $s.CurrentFilingModule.ID;
                sc.ID_Menus = $s.CurrentFilingModule.ID_Menus;
                sc.IsPowerApprover = false;
                sc.ID_Level = $s.CurrentFilingModule.Default.length + 1;
                sc.IsActive = true;
                $s.CurrentFilingModule.Default.push(sc);
            } else {
                $s.SetSystemStatus('Max approver level reached.', 'warning');
            }
        }

        $s.PostToBaseFilingModule = function () {
            //Saving the current filing module data to Base filing module.
            var length = $s.BaseFilingModule.length;
            for (var x = 0; x < length; x++) {
                if ($s.BaseFilingModule[x].ID == $s.CurrentFilingModule.ID) {
                    $s.BaseFilingModule[x] = $s.CurrentFilingModule;
                }
            }
        }

        $s.LoadApprovers = function (idx) {
            //Posting Current Filing Module to Base Filing Module
            if ($s.CurrentFilingModule != null || $s.CurrentFilingModule != undefined)
                $s.PostToBaseFilingModule();

            $('.am-filing-item').removeClass('selected');
            $('#afi-' + idx).addClass('selected');
            $('#afim-' + idx).addClass('selected');

            $s.CurrentFilingModule = [];
            $s.SetSystemStatus('Loading approvers for ' + $s.FilingModulesSelected.filter(x => { return x.ID == idx; }).Name, 'loading');
            $s.CurrentFilingModule = $s.BaseFilingModule.filter(x => { return x.ID == idx; })[0];
            $s.SetSystemStatus('Ready');
        }

        $s.LoadEmployees = function (id) {

            var dlgData = {
                ID_Company: $s.Schema.ID_Company || $s.Session('ID_Company'),
                Title: "Employee Lists",
                IsMultiSelect: true,
                EnabledLoadButton: true,
                RemoveIds: $s.FilingModuleEmployees.map(x => x.ID)
            };

            $s.Dialog({
                template: 'EmployeeList',
                controller: 'dlgApproverEmployeeList',
                size: 'md',
                windowClass: 'select-approver-dlg',
                data: { Data: dlgData }
            }).result.then(function (ret) {
                if (ret != undefined) {
                    $s.FilingModuleEmployees.push(...ret);
                    $s.CheckAllData($s.FilingModuleEmployees, false);
                }
            });
        }

        $s.CheckAllData = function (Data, CheckedValue) {
            Enumerable.From(Data).ForEach(function (x) {
                x.IsChecked = CheckedValue;
            });
        }

        $s.checkSource = function (data) {
            var source = $s.Employees.find(x => { return x.ID_Employee == data.ID_Employee });
            source.IsChecked = data.IsChecked;
            $s.selectedCount = Enumerable.From($s.Employees).Count(x => x.IsChecked);
        }

        $s.AddApprover = function (targetColumn, model) {


            var RemoveIds = [];
            var CurrentFilingModuleEmployees = $s.CurrentFilingModule.Default;

            
            CurrentFilingModuleEmployees.forEach(x => {               
                if (x.ID_Employee) RemoveIds.push(x.ID_Employee);
                if (x.ID_Employee2) RemoveIds.push(x.ID_Employee2);
                if (x.ID_Employee3) RemoveIds.push(x.ID_Employee3);
            });




            var dlgData = {
                IsMultiSelect: false,
                ID_Company: $s.Schema.ID_Company || $s.Session('ID_Company'),
                Title: "Approver Lists",
                RemoveIds //supports by ES6
            };

            $s.Dialog({
                template: 'EmployeeList',
                controller: 'dlgApproverEmployeeList',
                size: 'md',
                windowClass: 'select-approver-dlg',
                data: { Data: dlgData }
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

        $s.SelectAllEmployees = false;
        $s.CheckAllEmployees = function () {
            $s.SelectAllEmployees = !$s.SelectAllEmployees;
            $s.CheckAllData($s.FilingModuleEmployees, $s.SelectAllEmployees);
        }

        $s.RemoveEmployeeLists = [];
        $s.RemoveEmployees = function () {

            var rows = Enumerable
                .From($s.FilingModuleEmployees)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray(); //, ID : x.ID

            var errorMsg = "";
            var rowLength = rows.length;

            if (rowLength > 0) {

                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.FilingModuleEmployees.indexOf(rows[y]);
                        if ($s.FilingModuleEmployees[Index].ID > 0) {
                            $s.RemoveEmployeeLists.push($s.FilingModuleEmployees[Index].ID);
                        }
                        $s.FilingModuleEmployees.splice(Index, 1);
                    }
                });
            }

        }

        $s.IsClickedFilingModuleEmployees = false;
        $s.LoadFilingModuleEmployees = function () {

            if ($s.IsClickedFilingModuleEmployees) return;

            $s.Request("ExecuteAction", {
                MenuCode: $s.MCode,
                ID_Approver: $s.Schema.ID,
                MethodName: 'LoadFilingModuleEmployeeList'
            }, $s.MyController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, "error");
                } else {
                    $s.FilingModuleEmployees = ret.Data;
                    $s.IsClickedFilingModuleEmployees = true;
                };
                $s.$apply();
            });
        }

        $s.RemoveDefaultApprover = function (idx) {
            var deleted = $s.CurrentFilingModule.Default[idx];
            if ($s.CurrentFilingModule.DeletedApprovers == undefined) $s.CurrentFilingModule.DeletedApprovers = [];
            if (deleted.ID > 0) $s.CurrentFilingModule.DeletedApprovers.push(deleted.ID);
            $s.CurrentFilingModule.Default.splice(idx, 1);
        }

        $s.ApplyEmployees = function () {
            $s.Confirm("This will apply to database, do you wish to continue?").then(function (r) {
                $s.Request("ExecuteAction", {
                    MenuCode: $s.MCode,
                    ID_Approver: $s.Schema.ID,
                    FilingModuleEmployees: $s.FilingModuleEmployees,
                    RemoveFilingModuleEmployees: $s.RemoveEmployeeLists,
                    MethodName: "PostApproverMatrixTemplate"
                }, $s.MyController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                    }
                });
            });
        }
        

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }])
    .controller('dlgApproverEmployeeList', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.MyController = 'ApproverMatrix';

        $s.Param = {
            Title: "Employee Lists",
            ID_Company: 1,
            IsMultiSelect: false,
            RemoveIds: [] //This will be used to remove the existing request IDS to avoid duplicate
        }

        $.extend($s.Param, $dlgData.Data);

        $s.tblOptions = {
            Columns: [
                { Name: 'Name', Label: 'Name' },
                { Name: 'Designation', Label: 'Position' }
            ],
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'Designation', Type: 9, Label: 'Position' }
            ],
            HasMultiSelect: $s.Param.IsMultiSelect,
            EnabledLoadButton: $s.Param.EnabledLoadButton
        };

        $s.LoadTable($s.tblOptions, 'ExecuteAction',
                $s.MyController, {
                MethodName: 'LoadApproverList',
                MenuCode: 'AdministrativeApproverMatrix'
        }).then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message,'error');
            } else {
                //Remove Duplicate Ids.
                for (var i = ret.Data.Rows.length - 1; i >= 0; --i) {
                    var data = $s.Param.RemoveIds.filter(x => { return x == ret.Data.Rows[i].ID; })[0];

                    if (data!= undefined && data > 0) {
                        ret.Data.Rows.splice(i, 1);
                    }
                }
                
                $s.SetSystemStatus('Ready');
            }
            $s.$apply();
        });

        $s.LoadEmployees = function () {
            //Load/Return all selected/checked employees.
            $mi.close($s.tblOptions.Data.Rows.filter(x => { return x.IsChecked == true; }));
        }

        $s.openForm = function (emp) {
            $mi.close(emp)
        }

        $s.close = function () {
            $mi.close();
        }
    }]);