angular.module('app')
    .controller('OrbitWorkScheduleScheduleFile', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.ScheduleFile = {};
        $s.TableSchema = [];
        $s.ScheduleFileDetail = [];
        $s.ScheduleFileDetailTableSchema = [];
        $s.IsDetailsTabClicked = 0;
        $s.Controller = 'ScheduleFile';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'ID_Company', Label: 'Company'},
                { Name: 'StartDate', Label: 'Start Date' },
                { Name: 'DateCreated', Label: 'Filing Date' },
                { Name: 'ApprovalDate', Label: 'Approval Date' },
                { Name: 'IsDefault', Label: 'Default Schedule' },
                { Name: 'IsPosted', Label: 'Posted' },
                { Name: 'IsApplied', Label: 'Applied' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'ID_FilingStatus', Type: 1, ControlType: 'DropDown', Label: 'Filing Status' }
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
                        $s.ScheduleFile = $s.PlotDefault(ret.Data.Form, ret.Data.ScheduleFileTableSchema, $s.RecordID);
                        $s.TableSchema = ret.Data.ScheduleFileTableSchema;
                        //$s.ScheduleFileDetailTableSchema = ret.Data.ScheduleFileDetailTableSchema;
                        $s.ComboBoxList();
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.Controller).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID }, $s.Controller);
        }
        $s.Companies = [];
        $s.FilingStatusList = [];
        $s.ComboBoxList = function () {
            $s.Request('LoadLookup', { Name: 'Company' }, 'GetLookup').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.Companies = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.Request('LoadLookup', { Name: 'FilingStatus' }, 'GetLookup').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.FilingStatusList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        //// END ----- Load record

        ////Save New - Edit - Delete
        $s.SaveForm = function () {

            if ($s.IsTabsValid('form.dailyschedule')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.ScheduleFile, Details: $s.ScheduleFileDetail, DetailsToDelete: $s.DeletedScheduleFileDetails }, $s.Controller).then(function (ret) {
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
            $s.deleteRow('DeleteRecord', $s.Controller)
        }

        /////////////DAILY SCHEDULE DETAILS
        $s.DeletedScheduleFileDetails = [];

        $s.SelectAllScheduleFileDetailRows = false;
        $s.CheckAllScheduleFileDetailRecords = function () {
            $s.SelectAllScheduleFileDetailRows = !$s.SelectAllScheduleFileDetailRows;
            Enumerable.From($s.DeletedScheduleFileDetails).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllScheduleFileDetailRows;
            });
        }

        $s.DeleteScheduleFileDetails = function () {
            var rows = Enumerable
                .From($s.DeletedScheduleFileDetails)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.ScheduleFileDetail.indexOf(rows[y]);
                        if ($s.ScheduleFileDetail[Index].ID > 0) {
                            $s.ScheduleFileDetail.push($s.ScheduleFileDetail[Index].ID);
                        }
                        $s.ScheduleFileDetail.splice(Index, 1);
                    }
                });
            }
        }

        $s.AddDetail = function () {
            var newRecord = {
                ID: 0,
                Code: '',
                Name: '',
                ID_DailySchedule: $s.Schema.ID,
                ID_HourType: null,
                Day: 1,
                StartTime: null,
                StartMinute: null,
                EndMinute: null, // Computed column
                SeqNo: 0,
                IsActive: false,
                Comment: '',
                Hours: null, // Computed column
                EndTime: null, //Computed column
                BreakMinutes: 0,
                FirstIn: false,
                LastOut: false,
                FlexibleMinutes: null, //Computed column
                FlexibleHours: 0,
                WithPay: false,
                AutoApprove: false,
                LBoundStartMinute: null,
                UBoundEndMinute: null
            }
            $s.ScheduleFileDetail.push(newRecord);
        }

        $s.LoadScheduleFileDetails = function () {
            if ($s.IsDetailsTabClicked == 1) return;

            $s.Request('GetScheduleFileDetails', { ID_ScheduleFile: $s.Schema.ID }).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.ScheduleFileDetail = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.IsDetailsTabClicked = 1;
        }

    }]);