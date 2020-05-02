angular.module('app')
    .controller('SupplementarySettingsCity', ['$scope', '$controller', '$state', '$priv', function ($s, $c, $st, $priv) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.TableSchema = [];
        $s.RecordID = $st.params.cID == undefined ? '' : $st.params.cID;
        $s.MenuCode = 'SupplementarySettingsCity';

        $s.$parent.Override_Return = function () {
            $st.go($st.$current.parent.name + '.Form', { ID: $st.params.ID }, { reload: true });
        }

        $s.Init = function () {
            $s.LoadForm().then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                    $s.TableSchema = ret.Data.Schema;
                    $s.SetSystemStatus('Ready');
                    $s.$parent.ListTitle = $s.Schema.Name;
                }
                $s.$apply();
            })
        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, 'City');
        }

        $s.saveForm = function () {
            if (!$s.MenuPrivileges.HasEdit && $s.RecordID > 0) {
                $s.Prompt('You are not allowed to update this record.');
                return;
            }
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.Request('SaveForm', { Data: $s.Schema, Barangay: $s.brgyOptions.tblOptions.Data == undefined ? [] : $s.brgyOptions.tblOptions.Data.Rows, MenuCode: $s.MenuCode }, 'City').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { cID: ret.Data }, { reload: true });
                }
            });
        }

        $s.delete = function () {
            if (!$s.MenuPrivileges.HasDelete) {
                $s.Prompt('You are not allowed to delete record(s).');
                return;
            }
            $s.deleteRow('DeleteRecord', 'City', { MenuCode: $s.MenuCode })
        }

        $s.SetSystemStatus('Loading form', 'loading');
        $s.Init();

        $s.brgyOptions = {
            trigger: function () { },
            tblOptions: {
                Columns: [
                    //{ Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Name', ControlType: 'text' },
                ],
                Filters: [
                    { Name: 'Name', Type: 9, Label: 'Name' },
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteRecord', 'City', tblOptions, { MenuCode: $s.MenuCode });
                },
                isEditable: true,
                openForm: function () { }
            },
            Method: 'LoadList',
            Controller: 'City',
            Parameter: { ID_City: $s.RecordID, MenuCode: $s.MenuCode }
        };
        $s.loadBarangay = function () {
            if ($s.brgyOptions.tblOptions.Data != undefined)
                if ($s.brgyOptions.tblOptions.Data.Rows != undefined)
                    if ($s.brgyOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadRights', { Code: 'SupplementarySettingsBarangay' }, 'Menu').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.Request('LoadSchema', { Table: 'tBarangay' }, 'Menu').then(function (ret2) {
                        if (ret2.Type == 2) {
                            $s.SetSystemStatus(ret2.Message, 'error');
                            $s.$apply();
                        } else {
                            $s.brgyOptions.tblOptions.TableSchema = ret2.Data;
                            $s.brgyOptions.tblOptions.HasNew = ret.Data.HasNew;
                            $s.brgyOptions.tblOptions.HasEdit = ret.Data.HasEdit;
                            $s.brgyOptions.tblOptions.HasDelete = ret.Data.HasDelete;
                            $s.brgyOptions.trigger().then(function (tblOptions) {
                                $s.brgyOptions.tblOptions = tblOptions;
                                $s.brgyOptions.tblOptions.newForm = function () {
                                    $s.brgyOptions.tblOptions.Data.Rows.unshift({
                                        ID: 0, ID_City: $s.RecordID, IsActive: true, Name: null
                                    });
                                }
                            });
                        }
                    });
                }
            });
        }
    }])