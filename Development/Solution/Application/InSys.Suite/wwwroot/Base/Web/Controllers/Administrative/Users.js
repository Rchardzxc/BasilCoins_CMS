angular.module('app')
    .controller('AdministrativeUsers', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        //Initialize container for data
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'LogInName', Label: 'Login Name' },
                { Name: 'Employee'},
                { Name: 'Roles', Label: 'Role' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'LogInName', Type: 9, Label: 'Log-in Name' },
                { Name: 'Employee', Type: 9, Label: 'Employee' },
                {
                    Name: 'ID_Roles', Label: "Role", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'Users',
                        method: 'LoadLookup',
                        parameter: { MenuCode: 'AdministrativeUsers', LookupName: 'roles' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Role' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' },
            ]
        };
        $s.RolesList = [];

        $s.employeeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Employee' }
                ],
                Filters: [
                    { Name: 'Name', Type: 9, Label: 'Employee' }
                ] 
            },
            method: 'LoadLookup',
            controller: 'Users',
            parameter: { MenuCode: 'AdministrativeUsers', LookupName: 'employee' }
        };
    
        $s.LoadRoles = function () {
            return $s.Request('LoadLookup', { MenuCode: 'AdministrativeUsers', LookupName: 'roles'}, 'Users').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.RolesList = ret.Data;
                }
                $s.$apply();
            });
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading dropdown', 'loading');
                $s.LoadRoles().then(function () {
                    $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                    $s.LoadForm().then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.TableSchema = ret.Data.Schema;
                            $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                            $s.SetSystemStatus('Ready');
                        }
                        $s.$apply();
                    })
                });
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'Users', { MenuCode: 'AdministrativeUsers'}).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: 'AdministrativeUsers' }, 'Users');
        }

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.users')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, MenuCode: 'AdministrativeUsers'}, 'Users').then(function (ret) {
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
            $s.deleteRow('DeleteRecord', 'Users', { MenuCode: 'AdministrativeUsers'})
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

        $s.test = function () {
            $s.isread = !$s.isread;
        }
    }]);