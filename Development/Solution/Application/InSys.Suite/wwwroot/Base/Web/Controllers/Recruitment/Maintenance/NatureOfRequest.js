angular.module('app')
    .controller('RecruitmentMaintenanceNatureofRequests', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'NatureOfRequest';
        $s.Title = "Nature Of Request";
        $s.MenuName = "Nature Of Request";
        $s.TableSchema = [];

        $s.MenuCode = 'RecruitmentMaintenanceNatureofRequests';

        $s.Schema = {};

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'IsActive', Label: 'Is Active' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' },
            ]
        };

        // BEGIN --- Load record

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
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController, { MenuCode: $s.MenuCode }).then(function (ret) {
                  
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, $s.myController);
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        // END ----- Load record

        //Save New - Edit - Delete
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.natureofrequest')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
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
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }

    }]);