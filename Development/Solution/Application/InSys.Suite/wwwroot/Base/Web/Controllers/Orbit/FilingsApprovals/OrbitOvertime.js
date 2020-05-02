angular.module('app')
    .controller('OrbitFilings&ApprovalsOvertime', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.WorkCreditList = [];
        $s.ApprovalHistory = [];

        $s.myController = 'OrbitOvertime';
        $s.MenuCode = 'OrbitFilings&ApprovalsOvertime';
        $s.FormName = "'form.overtime'";
        $s.DisplayFilingStatus = "";
        $s.FSSummary = {
            Filed: 10,
            Disapproved: 20,
            Approved: 30
        }

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' }, 
                { Name: 'ReferenceNo', Label: 'Ref#' },
                { Name: 'Employee', Label: 'Employee' },
                { Name: 'FilingStatus', Label: 'Status', ControlType: 'status' },
                { Name: 'WorkDate', Label: 'Work Date', ControlType: 'date', IsReadOnly: true, Format: 'MMM, DD YYYY' },
                { Name: 'StartTime', Label: 'Start Time', ControlType: 'date', IsReadOnly: true, Format: 'MMM, DD YYYY hh:mm A' },
                { Name: 'EndTime', Label: 'End Time', ControlType: 'date', IsReadOnly: true, Format: 'MMM, DD YYYY hh:mm A' },
                { Name: 'ComputedHours', Label: 'Computed hrs' },
                { Name: 'ConsideredHours', Label: 'Considered hrs' },
                { Name: 'CreatedBy', Label: 'Created By' },
                { Name: 'ModifiedBy', Label: 'Modified By' },
                { Name: 'ApprovedBy', Label: 'Approved By' },
                { Name: 'Reason', Label: 'Reason' },
                { Name: 'IsPosted', Label: 'Is Posted' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: []
        };

        //// BEGIN --- Load record

        //Load Lookup
        $s.LoadLookup = function () {
            var _method = 'LoadLookup';
            var _controller = 'Common';
            $s.Request(_method, { LookupName: "WorkCredit" }, _controller).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('ready');
                    $s.WorkCreditList = ret.Data.Rows;
                }
                $s.$apply();
            });
        }
        
        //Since there's no StartDate/EndDate dates, then it will get the value from StartTime/EndTime
        $s.FormatTimeAndDate = function () {
            $s.Schema.StartDate = moment(new Date($s.Schema.StartTime)).format("MM/DD/YYYY");
            $s.Schema.EndDate = moment(new Date($s.Schema.EndTime)).format("MM/DD/YYYY");
        }
        
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadLookup();
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema
                        $s.FormatTimeAndDate();
                        $s.SetIfRequired($s.TableSchema);
                        $s.AfterFormInit();
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

        $s.BeforeSave = function () {
            //Place all action/validations before saving of records
            //In validation, if not valid then return false in order not to proceed on saving.
            //Validate if tabs are valid otherwise do not proceed on saving

            //Validate StartTime, should be greater than End Time
            var _StartTime = new Date($s.Schema.StartDate + " " + $s.Schema.StartTime);
            var _EndTime = new Date($s.Schema.EndDate + " " + $s.Schema.EndTime);

            if (!$s.IsTabsValid($s.FormName))
                return false;
            else if ($s.Schema.ComputedHours <= 0) {
                $s.SetSystemStatus('Zero Computed Hours is not allowed.', 'error');
                return false;
            } else { return true; }
        }


        ////Save New - Edit - Delete
        $s.saveForm = function () {
            //Validate Before Save
            if (!$s.BeforeSave()) return;


            //Validate record if filing status is approved and disapproved, and changes happened then set to Filed.
            //if ($s.Schema.ID_FilingStatus != 1) {
            //    $s.Schema.ID_FilingStatus = 1;
                //$s.Confirm("Record will be set into 'Filed' status. Do you wish to continue?", "Validate").then(function () {
                //    $s.Schema.ID_FilingStatus = 1;
                //});
            //}

            $s.Schema.ID_FilingStatus = $s.Schema.ID_FilingStatus != 1 ? 1 : $s.Schema.ID_FilingStatus;


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
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode });
        }

        $s.SetEndDateValue = function () {

            var _SDate = angular.copy($s.Schema.StartDate);
            var _EDate = angular.copy($s.Schema.EndDate);

            if ($s.IsFirstDateGreaterThan(_SDate, _EDate)) {
                $s.Schema.EndDate = $s.Schema.StartDate;
                $s.ComputeHours();
            } else {
                $s.ComputeHours();
            }
        }

        $s.ComputeHours = function () {
            var _StartDate = angular.copy($s.Schema.WorkDate);
            var _EndDate = angular.copy($s.Schema.WorkDate);
            var _StartTime = angular.copy($s.Schema.StartTime);
            var _EndTime = angular.copy($s.Schema.EndTime);

            if (_StartTime == undefined || _EndTime == undefined) return;

            
            var _startTime = new Date(moment(new Date(_StartDate)).format("MM/DD/YYYY") + ' ' + _StartTime);
            var _endTime = new Date(moment(new Date(_EndDate)).format("MM/DD/YYYY") + ' ' + _EndTime);


            if ($s.IsFirstDateGreaterThan(_startTime, _endTime)) {
                _endTime = new Date(moment(_endTime).add(1, 'days'));
            }
            
            var _ComputedHours = $s.FormatNumber(Math.abs(_startTime - _endTime) / 36e5, 3);
            
            if (_ComputedHours >= 0)
                $s.Schema.ComputedHours = _ComputedHours;
        }
        
        $s.employeelookup = $s.GetStandardLookUp("Employee");

        //Click Post
        $s.SavePost = function () {
            $s.Request("Post", { MenuCode: $s.MenuCode, Data: $s.Schema, ID: $s.Schema.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Successfully Posted.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
                    $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
        }

        //Click UnPost
        $s.UnPost = function () {
            $s.Request("UnPost", { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2)
                    $s.SetSystemStatus(ret.Message, 'error');
                else {
                    $s.SetSystemStatus('Successfully Unposted.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
                    $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
        }

        //Click Approve
        $s.Approve = function () {
            $s.Confirm("Are you sure to approve?", "Confirmation").then(function () {
                $s.Request("Approve", { Data: $s.Schema, MenuCode: $s.MenuCode, ApproverComment: "" }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Successfully Approved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                    }
                });
            });
        }

        $s.Disapprove = function () {
            $s.Confirm("Are you sure to disapprove?", "Confirmation").then(function () {
                $s.Request("Disapprove", { Data: $s.Schema, MenuCode: $s.MenuCode, ApproverComment: ""}, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Successfully Disapprove.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                    }
                });
            });
        }

        $s.FilingStatusClass = "form-control";
        $s.AfterFormInit = function () {
            //Please place all your action after loading of listview or form view
            //This will be loaded after loading all Schema data.
            $s.SetDisableForm($s.Schema.IsPosted);
            //Set filling status caption
            $s.DisplayFilingStatus = $s.FilingStatusList.filter(x => x.ID == $s.Schema.ID_FilingStatus)[0].Name;
            switch ($s.Schema.ID_FilingStatus) {
                case 2:
                    $s.FilingStatusClass = 'form-control-approved-status'
                    break;
                case 3:
                    $s.FilingStatusClass = 'form-control-disapproved-status'
                    break;
                default:
                    $s.FilingStatusClass = "form-control";
            }
            //'': Schema.ID_FilingStatus == 2, : Schema.ID_FilingStatus == 3, 'form-control': Schema.ID_FilingStatus == 1
        }

        $s.ChangeStartDate = function () {
            $s.Schema.StartDate = moment(new Date($s.Schema.WorkDate)).format("MM/DD/YYYY");;
            $s.Schema.EndDate = moment(new Date($s.Schema.WorkDate)).format("MM/DD/YYYY");
        }

        $s.GetApprovalHistory = function () {
            $s.Request("LoadApprovalHistory", { MenuCode: $s.MenuCode, ID: $s.Schema.ID }, "Common").then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ApprovalHistory = ret.Data;
                    $s.SetSystemStatus('Ready')
                }
                $s.$apply();
            });
        }

        

}]);