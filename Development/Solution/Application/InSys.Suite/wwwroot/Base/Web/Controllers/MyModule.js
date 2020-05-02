angular.module('app')
    .controller('MyModule', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.TableSchema = [];
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.MenuCode = 'MyModule';

        //Initialize table grid
        $s.tblOptions = {
            Columns: [
                { Name: 'ID', Label: '#' },
                { Name: 'Name', Label: 'Name' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' },
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
                        console.log($s.Schema)
                    }
                    $s.$apply();
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'MyModule', { MenuCode: $s.MenuCode }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, 'MyModule');
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', 'MyModule', { MenuCode: $s.MenuCode })
        }

        $s.genderLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Gender' }
                ]
            },
            method: 'LoadLookup',
            controller: 'MyModule',
            parameter: { LookupName: 'gender', MenuCode: $s.MenuCode }
        };

        $s.saveForm = function () {
            if (!$s.IsTabsValid('mymodule', $s.TableSchema, 'general')) return;
            if (!$s.IsTabsValid('mymodule', $s.dependentsOptions.tblOptions.TableSchema, 'dependents')) return;

            if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                $s.Prompt('You are not allowed to update this record.');
                return;
            }
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.Request('SaveForm', { Data: $s.Schema, MenuCode: $s.MenuCode, Dependents: $s.dependentsOptions.tblOptions.Data.Rows }, 'MyModule').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                    $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
        }

        $s.dependentsOptions = {
            trigger: function () { },
            tblOptions: {
                Data: { Rows: [], Total: 0 },
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'DependentName', Label: 'Dependent', ControlType: 'text' },
                ],
                HasNew: true,
                HasEdit: true,
                HasDelete: true,
                Filters: [
                    { Name: 'DependentName', Type: 9, Label: 'Dependent' },
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteDetail', 'MyModule', tblOptions, { DetailName: 'dependents', MenuCode: $s.MenuCode });
                },
                isEditable: true
            },
            Method: 'LoadDetail',
            Controller: 'MyModule',
            Parameter: { DetailName: 'dependents', ID_MyModule: $s.RecordID, MenuCode: $s.MenuCode }
        };

        $s.loadDependents = function () {
            if ($s.dependentsOptions.tblOptions.Data != undefined)
                if ($s.dependentsOptions.tblOptions.Data.Rows != undefined)
                    if ($s.dependentsOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tMyModule_Dependent' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                } else {
                    $s.dependentsOptions.tblOptions.TableSchema = ret2.Data;
                    $s.dependentsOptions.trigger().then(function (tblOptions) {
                        $s.dependentsOptions.tblOptions = tblOptions;
                        $s.dependentsOptions.tblOptions.newForm = function () {
                            var defaultValue = $s.PlotDefault({}, ret2.Data, 0);
                            defaultValue.ID_MyModule = $s.RecordID;
                            $s.dependentsOptions.tblOptions.Data.Rows.unshift(defaultValue);
                        }
                    });
                }
                $s.$apply();
            });

        }

        $s.Init();

    }]);