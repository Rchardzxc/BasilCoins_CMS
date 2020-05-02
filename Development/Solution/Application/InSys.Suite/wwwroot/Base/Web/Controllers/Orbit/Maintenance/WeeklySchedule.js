angular.module('app')
    .controller('OrbitMaintenanceWeeklySchedule', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'WeeklySchedule';
        $s.Schema = {};
        $s.TableSchema = [];
        $s.ShiftTypes = [];
        $s.IsDetailsTabClicked = 0;
        $s.DailyScheduleDetailRecord = [];
        $s.MenuCode = 'OrbitMaintenanceWeeklySchedule';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'Monday', Label: 'Monday' },
                { Name: 'Tuesday', Label: 'Tuesday' },
                { Name: 'Wednesday', Label: 'Wednesday' },
                { Name: 'Thursday', Label: 'Thursday' },
                { Name: 'Friday', Label: 'Friday' },
                { Name: 'Saturday', Label: 'Saturday' },
                { Name: 'Sunday', Label: 'Sunday' },
                { Name: 'IsActive', Label: 'Is Active' },
                { Name: 'Comment', Label: 'Comment' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Is Active' },
                { Name: 'Comment', Type: 9, Label: 'Comment' },
            ]
        };

        $s.GetLookUp = function () {
            //ShiftTypes
            $s.Request('LoadLookUp', { Name: 'ShiftType' }, 'GetLookUp').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ShiftTypes = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

        }


        //// BEGIN --- Load record
        $s.Init = function () {
            if ($s.RecordID != '') {
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
            }
            else
            {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController, { MenuCode: $s.MenuCode }).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        
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
            if (!$s.IsTabsValid('form.weeklyschedule', $s.TableSchema, 'general')) return;

            if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                $.Prompt('You are not allowed to update this record.');
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

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }


        $s.dailyschedulelookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Schedule' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'WeeklySchedule',
            parameter: { LookupName: 'DailySchedule', MenuCode: $s.MenuCode }
        };

    }]);
//$s.taxComputationLookUp = {
//    tblOptions: {
//        Columns: [
//            { Name: 'ID', Label: '#' },
//            { Name: 'Name', Label: 'Tax Computation' }
//        ]
//    },
//    method: 'LoadLookup',
//    controller: 'PayrollParameter',
//    parameter: { LookupName: 'taxComputation', MenuCode: $s.MenuCode }
//};