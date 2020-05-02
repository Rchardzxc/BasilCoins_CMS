angular.module('app')
    .controller('MaintenanceProvince', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];

        $s.tblOptions = {
            Columns: [
                { Name: 'Code' },
                { Name: 'Name' },
                { Name: 'Description' },
                { Name: 'Country' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9 },
                { Name: 'Name', Type: 9 },
                { Name: 'Description', Type: 9 },
                { Name: 'Country', Type: 9 },
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
            controller: 'Province',
            parameter: { MenuCode: 'MaintenanceProvince', LookupName: 'country' }
        };

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
                $s.LoadTable($s.tblOptions, 'LoadList', 'Province', { MenuCode: 'MaintenanceProvince' }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: 'MaintenanceProvince' }, 'Province');
        }

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.province')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, MenuCode: 'MaintenanceProvince' }, 'Province').then(function (ret) {
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
            $s.deleteRow('DeleteRecord', 'Province', { MenuCode: 'MaintenanceProvince' })
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }]);