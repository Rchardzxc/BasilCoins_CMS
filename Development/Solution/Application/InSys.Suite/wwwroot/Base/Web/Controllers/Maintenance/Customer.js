angular.module('app')
    .controller('MaintenanceCustomer', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];

        $s.tblOptions = {
            Columns: [
                { Name: 'Name' },
                { Name: 'EmailAddress', Label: 'Email Address' },
                { Name: 'City' },
                { Name: 'Province' },
                { Name: 'Country' },
                { Name: 'DateVerified', Label: 'Date Verified' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9 },
                { Name: 'EmailAddress', Type: 9, Label: 'Email Address' },
                { Name: 'City', Type: 9 },
                { Name: 'Province', Type: 9 },
                { Name: 'Country', Type: 9 },
                { Name: 'DateVerified', Type: 9, Label: 'Date Verified' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' }
            ]
        };

        $s.countryLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'Code', Label: 'Code' },
                    { Name: 'Name', Label: 'Name' }
                ],
                Filters: [
                    { Name: 'Code', Type: 9, Label: 'Code' },
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ]
            },
            method: 'LoadLookup',
            controller: 'Customer',
            parameter: { MenuCode: 'MaintenanceCustomer', LookupName: 'country' }
        };

        $s.provinceLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'Code', Label: 'Code' },
                    { Name: 'Name', Label: 'Name' }
                ],
                Filters: [
                    { Name: 'Code', Type: 9, Label: 'Code' },
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ]
            },
            method: 'LoadLookup',
            controller: 'Customer',
            parameter: { MenuCode: 'MaintenanceCustomer', LookupName: 'province' }
        };

        $s.changePassword = function (id) {
            console.log('chard');
            //channge password
            $s.Dialog({
                template: 'ChangePassword',
                controller: 'dlgChangePassword',
                size: 'md',
                data: { Data: $s.RecordID }
            })
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
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
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'Customer', { MenuCode: 'MaintenanceCustomer' }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: 'MaintenanceCustomer' }, 'Customer');
        }

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.customer')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, MenuCode: 'MaintenanceCustomer' }, 'Customer').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data.ID }, { reload: true });
                    }
                });
            }
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', 'Customer', { MenuCode: 'MaintenanceCustomer' })
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }])

    .controller('dlgChangePassword', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.MyController = 'Customer';

        $s.UserData = {
            NewPassword: null,
            ConfirmPassword: null,
            ID_Customer: $dlgData.Data
        };

        $s.SetupAccount = function () {
            console.log($s.UserData, $s.UserData.NewPassword, $s.UserData.ConfirmPassword);
            if ($s.UserData.NewPassword != $s.UserData.ConfirmPassword) {
                $s.Prompt('Password does not match.');
                return;
            }

            $s.Request('ExecuteAction', { Action: { Name: 'SavePasswordChange', Parameter: { Data: $s.UserData } }, MenuCode: 'MaintenanceCustomer' }, $s.MyController).then(function (res) {
                if (res.Type == 2) {
                    $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                    $s.$apply();
                    return;
                }
                else {
                    $s.Prompt('Successfully update password!', 'Success');
                    $s.$apply();
                    $mi.close();
                }
            });
        }

        $s.close = function () {
            $mi.close();
        }
    }])