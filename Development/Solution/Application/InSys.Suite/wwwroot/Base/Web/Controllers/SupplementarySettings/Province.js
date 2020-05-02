angular.module('app')
    .controller('SupplementarySettingsProvince', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.TableSchema = [];
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID
        $s.MenuCode = 'SupplementarySettingsProvince';
        
        //Initialize container for data
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Name', Label: 'Name' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' }
            ]
        };

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
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'Province', { MenuCode: $s.MenuCode }).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }

        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, 'Province');
        }

        $s.saveForm = function () {

            //Validate General Tab
            if (!$s.IsTabsValid('form.province', $s.TableSchema, 'general')) { return; }

            //Validate Province Tab
            if (!$s.IsTabsValid('form.province', $s.cityOptions.tblOptions.TableSchema, 'city')) { return; }

            if (!$s.MenuPrivileges.HasEdit && $s.RecordID > 0) {
                $s.Prompt('You are not allowed to update this record.');
                return;
            }
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.Request('SaveForm', { Data: $s.Schema, City: $s.cityOptions.tblOptions.Data.Rows, MenuCode: $s.MenuCode }, 'Province').then(function (ret) {
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
            if (!$s.MenuPrivileges.HasDelete) {
                $s.Prompt('You are not allowed to delete record(s).');
                return;
            }
            $s.deleteRow('DeleteRecord', 'Province', { MenuCode: $s.MenuCode })
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

        $s.cityOptions = {
            trigger: function () { },
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Name', ControlType: 'text' },
                ],
                HasNew: true,
                HasDelete: true,
                Filters: [
                    { Name: 'Name', Type: 9, Label: 'Name' },
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteCity', 'Province', tblOptions, { MenuCode: $s.MenuCode });
                },
                isEditable: true,
                hasOpenIcon: true,
                hasOpenIconClick: function (row) {
                    $st.go($st.$current.parent.name + '.Child', { ID: $s.RecordID, cID: row.ID, Data: row, MenuCode: $s.MenuCode }, { reload: true });
                },
                openForm: function () { }
            },
            Method: 'LoadCity',
            Controller: 'Province',
            Parameter: { ID_Province: $s.RecordID, MenuCode: $s.MenuCode }
        };
        $s.loadCity = function () {
            if ($s.cityOptions.tblOptions.Data != undefined)
                if ($s.cityOptions.tblOptions.Data.Rows != undefined)
                    if ($s.cityOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadRights', { Code: 'SupplementarySettingsCity' }, 'Menu').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.Request('LoadSchema', { Table: 'tCity' }, 'Menu').then(function (ret2) {
                        if (ret2.Type == 2) {
                            $s.SetSystemStatus(ret2.Message, 'error');
                            $s.$apply();
                        } else {
                            $s.cityOptions.tblOptions.TableSchema = ret2.Data;
                            $s.cityOptions.tblOptions.HasNew = ret.Data.HasNew;
                            $s.cityOptions.tblOptions.HasEdit = ret.Data.HasEdit;
                            $s.cityOptions.tblOptions.HasDelete = ret.Data.HasDelete;
                            $s.cityOptions.trigger().then(function (tblOptions) {
                                $s.cityOptions.tblOptions = tblOptions;
                                $s.cityOptions.tblOptions.newForm = function () {
                                    $s.cityOptions.tblOptions.Data.Rows.unshift({
                                        ID: 0, ID_Province: $s.RecordID, IsActive: true, Name: null
                                    });
                                }
                            });
                        }
                    });
                }
            });
            
        }

    }])