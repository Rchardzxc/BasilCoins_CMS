angular.module('app')
    .controller('OrbitMaintenanceLeaveType', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.TableSchema = [];

        $s.myController = 'LeaveType';
        $s.MenuCode = "OrbitMaintenanceLeaveType";

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'IsActive', Label: 'Is Active' },
                { Name: 'Remarks', Label: 'Remarks' },
                { Name: 'ShowInIONS', Label: 'Show In IONS' },

            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Is Active' },
                { Name: 'ShowInIONS', Type: 1, ControlType: 'radio', Label: 'Show IONS' }
            ]
        };


        $s.LeaveApplicableGender = [];

        $s.LeaveTypeLookUp = function () {
            //LookUp
            $s.Request('LoadLookup', { Name: 'LeaveApplicableGender', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.LeaveApplicableGender = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        //// BEGIN --- Load record
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LeaveTypeLookUp();
                $s.LoadForm().then(function (ret) {

                    console.log(ret);
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

                    console.log("WWWWWWWWWWWWWW");
                    console.log(ret);

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
        //// END ----- Load record

        ////Save New - Edit - Delete
        $s.saveForm = function () {
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

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode} );
        }

    }]);