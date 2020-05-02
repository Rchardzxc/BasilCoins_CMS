angular.module('app')
    .controller('OrbitMaintenanceShiftSchedule', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'ShiftSchedule';
        $s.Schema = {};
        $s.TableSchema = [];
        $s.ShiftTypes = [];
        $s.ShiftScheduleDetailRecord = [];
        $s.MCode = 'OrbitMaintenanceShiftSchedule';
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'WorkingHours', Label: 'Working Hrs.' },
                { Name: 'FirstInLastOut', Label: 'First-In Last-Out' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'FirstInLastOut', Type: 1, ControlType: 'radio', Label: 'First-In Last-Out' }
            ]
        };
        
      
        //// BEGIN --- Load record
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
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController, { MenuCode: $s.MCode }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MCode }, $s.myController);
        }
        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        //// END ----- Load record

        ////Save New - Edit - Delete
        $s.saveForm = function () {
            //if ($s.IsTabsValid('form.shiftschedule')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.Schema.WorkingMinutes = ($s.Schema.WorkingHours * 60);
                $s.Schema.ID_Company = $s.Session('ID_Company');
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            var details = [];
            if ($s.ShiftScheduleDetailsOption.tblOptions.Data.Rows.length > 0)
            {
                Enumerable.From($s.ShiftScheduleDetailsOption.tblOptions.Data.Rows).ForEach(function (x) {
                    x.ID_ShiftSchedule = $s.RecordID || 0;
                    if (new Date(moment(new Date()).format('YYYY-MM-DD') + ' ' + x.EndTime) < new Date(moment(new Date()).format('YYYY-MM-DD') + ' ' + x.StartTime))
                    {
                        var endTime = new date(x.EndTime);
                        x.EndTime = new Date(endTime.setDate(endTime.getDate() + 1));
                    }
                });
                details = $s.ShiftScheduleDetailsOption.tblOptions.Data.Rows;
            }
            $s.Request('SaveForm', { Data: $s.Schema, Details: details, MenuCode: $s.MCode }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                    }
                });
            //}
            
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MCode });
        }

        $s.ShiftScheduleDetailsOption = {
            trigger: function () { },
            tblOptions: {
                Data: { Rows: [] },
                Columns: [
                    //{ Name: 'ID', Label: '#'},
                    { Name: 'StartTime', Label: 'Start Time', ControlType: 'time' },
                    { Name: 'EndTime', Label: 'End Time', ControlType: 'time' },
                    {
                        Name: 'ID_HourType', Label: 'Hour Type', ControlType: 'select', Options:
                        {
                            controller: $s.myController,
                            method: 'LoadLookup',
                            parameter: { MenuCode: $s.MCode,Name:'HourType' }
                        }
                    },
                    { Name: 'Hours', Label: 'Hours', ControlType: 'text' },
                    { Name: 'FirstIn', Label: 'First In', ControlType: 'checkbox' },
                    { Name: 'LastOut', Label: 'Last Out', ControlType: 'checkbox' },
                    { Name: 'WithPay', Label: 'With Pay', ControlType: 'checkbox' },
                    { Name: 'AutoApprove', Label: 'Auto Approve', ControlType: 'checkbox' }
                ],
                Filters: [
                    { Name: 'ID', Type: 9 }
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteRecord', 'ShiftSchedule', tblOptions, { MenuCode: $s.MCode });
                },
                isEditable: true,
                hasOpenIcon: false,
                hasOpenIconClick: function (row) { },
                openForm: function () { }
            },
            Method: 'ExecuteAction',
            Controller: $s.myController,
            Parameter: { ID_ShiftSchedule: $s.RecordID, Menucode: $s.MCode, Action: 'LoadShiftScheduleDetails' },
            isEditable: true,
            hasOpenIcon: false,
            hasOpenIconClick: function (row) { },
            openForm: function () { }
        };

        $s.LoadScheduleDetails = function () {
            $s.Request('LoadSchema', { Table: 'tShiftSchedule_Detail' }, 'Menu').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.TableSchema = ret.Data;
                    $s.ShiftScheduleDetailsOption.tblOptions.TableSchema = ret.Data;
                    $s.ShiftScheduleDetailsOption.tblOptions.HasNew = true;
                    $s.ShiftScheduleDetailsOption.tblOptions.HasEdit = true;
                    $s.ShiftScheduleDetailsOption.tblOptions.HasDelete = true;
                    $s.ShiftScheduleDetailsOption.trigger().then(function (tblOptions) {
                        $s.ShiftScheduleDetailsOption.tblOptions = tblOptions;
                        $s.ShiftScheduleDetailsOption.tblOptions.newForm = function () {
                            var data = $s.PlotDefault({}, ret.Data, 0);
                            $s.ShiftScheduleDetailsOption.tblOptions.Data.Rows.unshift(data);                           
                        }
                        $s.$apply();
                    });


                    //$('.listing-holder').ready(function () {
                    //    $("input[name='StartTime']").on('change', function () {
                    //        console.log("Change is comming");
                    //    });
                    //});
                }
            });
        }

        $s.test = function (e) {
            console.log("wants to make ");
        }


        $s.ValidateField = {
            FieldChangeEvent: function () {
                console.log("change2");
            },
            FieldBlurEvent: function () {
                console.log("blur1");
            }

        }

        $s.ComputeFirstHalfHours = function () {
            $s.Schema.FirstHalfWorkingHours = $s.GetTimeDifference($s.Schema.TimeIn, $s.Schema.FirstHalfTimeOut);
        }

        $s.ComputeSecondHalfHours = function () {
            $s.Schema.SecondHalfWorkingHours = $s.GetTimeDifference($s.Schema.SecondHalfTimeIn, $s.Schema.TimeOut);
        }

        $s.ComputeWorkingHours = function () {
            $s.Schema.WorkingHours = $s.GetTimeDifference($s.Schema.TimeIn, $s.Schema.TimeOut);
            $s.ComputeFirstHalfHours();
            $s.ComputeSecondHalfHours();
        }


        



    }]);