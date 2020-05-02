angular.module('app')
    .controller('OrbitMaintenanceLeaveParameter', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.TableSchema = [];
        $s.IsDetailsTabClicked = 0;
        $s.LeaveParameterDetailRecord = [];

        $s.myController = 'LeaveParameter';
        $s.MenuCode = 'OrbitMaintenanceLeaveParameter';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'IsActive', Label: 'Is Active' },
                { Name: 'Comment', Label: 'Comment' },
                { Name: 'CreatedAt', Label: 'Created At' },
                { Name: 'ModifiedAt', Label: 'Modified At' },
                { Name: 'ID_CreatedBy', Label: 'Created By' },
                { Name: 'ID_ModifiedBy', Label: 'Modified By' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Is Active' }
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
        //// END ----- Load record

        ////Save New - Edit - Delete
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.leaveparameter')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, Details: $s.LeaveParameterDetailRecord, DetailsToDelete: $s.DeletedLeaveParameterDetails, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
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
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode });
        }

        /////////////DAILY SCHEDULE DETAILS
        $s.DeletedLeaveParameterDetails = [];
        $s.HourTypes = [];

        $s.SelectAllLeaveParameterDetailRows = false;
        $s.CheckAllLeaveParameterDetailRecords = function () {
            $s.SelectAllLeaveParameterDetailRows = !$s.SelectAllLeaveParameterDetailRows;
            Enumerable.From($s.LeaveParameterDetailRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllLeaveParameterDetailRows;
            });
        }

        $s.delLeaveParameterDetails = function () {
            var rows = Enumerable
                .From($s.LeaveParameterDetailRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.LeaveParameterDetailRecord.indexOf(rows[y]);
                        if ($s.LeaveParameterDetailRecord[Index].ID > 0) {
                            $s.DeletedLeaveParameterDetails.push($s.LeaveParameterDetailRecord[Index].ID);
                        }
                        $s.LeaveParameterDetailRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.newLeaveParameterDetails = function () {
            var newRecord = {
                ID: 0,
                ID_LeavePayrollItem: null,
                ID_LeaveAccrualType: null,
                ID_LeaveParameterItemReferenceDate: null,
                ID_LeaveParameter: $s.Schema.ID,
                SeqNo: null,
                ID_AccrualOption: null,
                AccrualDay: 1,
                InitialValue: 5,
                IsActive: true,
                Comment: ''
            }
            $s.LeaveParameterDetailRecord.push(newRecord);
        }


        $s.LeaveAccrualType = [];
        $s.LeaveParameterItemReferenceDate = [];
        $s.AccrualOption = [];
        $s.LeavePayrollItem = [];

        $s.LoadLeaveParameterItems = function () {
            if ($s.IsDetailsTabClicked == 1) return;

            //LookUp
            $s.Request('LoadLookup', { Name: 'LeaveAccrualType', MenuCode: $s.MenuCode }, $s.myController ).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.LeaveAccrualType = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.Request('LoadLookup', { Name: 'LeaveParameterItemReferenceDate', MenuCode: $s.MenuCode }, $s.myController ).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.LeaveParameterItemReferenceDate = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.Request('LoadLookup', { Name: 'AccrualOption', MenuCode: $s.MenuCode }, $s.myController ).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.AccrualOption = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.Request('LoadLookup', { Name: 'LeavePayrollItem', MenuCode: $s.MenuCode }, $s.myController ).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.LeavePayrollItem = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
            
            $s.Request('GetLeaveParameterDetails', { ID_LeaveParameter: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.LeaveParameterDetailRecord = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
            });
            $s.IsDetailsTabClicked = 1;
        }

    }]);