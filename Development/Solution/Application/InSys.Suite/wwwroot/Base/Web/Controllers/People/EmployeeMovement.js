angular.module('app')
    .controller('PeopleEmployeeMovement', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.myController = 'EmployeeMovement';
        $s.MenuCode = 'PeopleEmployeeMovement';
        $s.FormName = "'form.employeemovement'";
        $s.DisableLookUpField = false;
        
        $s.IsDetailedClicked = false;
        
        $s.tblOptions = {
            Columns: [
               // { Name: 'ID', Label: '#' },
                { Name: 'RefNumber', Label: 'Ref#' },
                { Name: 'EmployeeName', Label: 'Employee Name' },
                { Name: 'MovementType', Label: 'Movement Type' },
                { Name: 'EffectivityDate', Label: 'Effectivity Date', ControlType: 'date', IsReadOnly: true },
                { Name: 'FilingStatus', Label: 'Status', ControlType: 'status' },
                { Name: 'ApprovedDate', Label: 'Approved Date', ControlType: 'date', IsReadOnly: true },
                { Name: 'Remarks', Label: 'Remarks' },
                { Name: 'Posted', Label: 'Posted' },
                { Name: 'CreatedAt', Label: 'Created At', ControlType: 'date', IsReadOnly: true },
                { Name: 'CreatedBy', Label: 'Created By' },
                { Name: 'ModifiedAt', Label: 'Modified At', ControlType: 'date', IsReadOnly: true },
                { Name: 'ModifiedBy', Label: 'Modified By' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                //{ Name: 'ID', Type: 1, Label: '#', ControlType: 'int' },
                { Name: 'RefNumber', Type: 9, Label: 'Ref#' },
                { Name: 'EmployeeName', Type: 9, Label: 'Employee' },
                {
                    Name: 'ID_MovementType', Type: 1, ControlType: 'select', Label: 'Movement Type',
                    Options: {
                        controller: $s.myController, method: 'LoadLookup',
                        parameter: { LookupName: 'movementtype', MenuCode: $s.MenuCode }
                    }
                },
                {
                    Name: 'ID_FilingStatus', Type: 1, ControlType: 'select', Label: 'Filing Status',
                    Options: {
                        controller: $s.myController, method: 'LoadLookup',
                        parameter: { LookupName: 'filingstatus', MenuCode: $s.MenuCode }
                    }
                },
                { Name: 'EffectivityDate', Type: 12, Label: 'Effectivity Date', ControlType: 'date', Value: [null, null] },
            ]
        };

        //Setting Up LookUp
        $s.MovementTypes = [];
        $s.LoadMovementTypes = function () {
            $s.Request('LoadLookUp', { LookUpName: "movementtype", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2)
                    $s.SetSystemStatus(ret.Message, 'error');
                else {
                    $s.MovementTypes = ret.Data.Rows;
                }
            });
        }
        
        $s.employeeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Employee' }
                ]
            },
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Employee' }
            ],
            method: 'LoadLookUp',
            controller: $s.myController,
            parameter: { LookUpName: 'Employee', MenuCode: $s.MenuCode }
        }

        $s.BeforeFormInit = function () {
            //Load Lookup
            $s.LoadMovementTypes();
        }

        $s.DisplayFilingStatus = "";
        $s.FilingStatusClass = "form-control";
        $s.AfterFormInit = function () {
            //Please place all your action after loading of listview or form view
            //This will be loaded after loading all Schema data.
            $s.SetDisableForm($s.Schema.Posted);
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

        $s.BeforeSave = function () {
            //Place all action/validations before saving of records
            //In validation, if not valid then return false in order not to proceed on saving.
            //Validate if tabs are valid otherwise do not proceed on saving
            if (!$s.IsTabsValid($s.FormName)) return false
            else if (!$s.IsTabsValid($s.FormName, {}, 'detail')) return false;
            else if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                $s.Prompt('You are not allowed to update this record.'); return false;
            } else if (!$s.MovementDetailsHasRecord()) {
                $s.Prompt("'Details' tab is required at least one record.");
                return false;
            } else { return true; }
        }

        $s.AfterSave = function () {
            //Place all action before saving of records
        }
        
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.BeforeFormInit();
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        if ($s.RecordID > 0) $s.LoadEmployeeMovementRecords();
                        $s.AfterFormInit();
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController, { MenuCode: $s.MenuCode }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, $s.myController);
        }

        $s.saveForm = function () {
            //Before Saving Action, Mostly all validations
            if (!$s.BeforeSave()) return;

            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.Request('SaveForm', { Data: $s.Schema, Detail: $s.EmployeeMovementDetails, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
                    $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
            $s.AfterSave();
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }

        $s.newEmployeeMovementRecord = function () {
            console.log("new employee movement record");
        }

        $s.delEmployeeMovementRecord = function () {
            console.log("delete employee movemenet record");
        }

        //Change Movement Type field.
        $s.SetReloadingOfEMF = function () {
            //Check if there are records.
            if ($s.MovementDetailsHasRecord()) { //Validate if already clicked, it means that record details has already data.
                $s.Prompt("Record on 'Details tab' will be discarded.");
            }
            $s.IsDetailedClicked = false;
            $s.EmployeeMovementDetails = [];
        }

        //Check Employee Movement Details if there are records.
        $s.MovementDetailsHasRecord = function () {
            var record = $s.EmployeeMovementDetails.filter(x => { return (x.NewValue != "" && x.NewValue != null); });
            if (record.length > 0)
                return true;
            else return false;
        }
        
        $s.EmployeeMovementDetails = [];
        //Click Details Tab
        $s.LoadEmployeeMovementRecords = function () {
            if ($s.IsDetailedClicked) return; //Avoid reloading employee movement records.
            $s.EmployeeMovementDetails = [];
            if ($s.Schema.ID_MovementType == 0 || $s.Schema.ID_MovementType == null || $s.Schema.ID_MovementType == undefined) return;
            $s.Request("LoadDetail", { MenuCode: $s.MenuCode, Schema: $s.Schema }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, "error");
                } else {
                    $s.EmployeeMovementDetails = ret.Data.Form;
                    $s.IsDetailedClicked = true;
                }
            });
        }

        $s.selectAllEmployeeMovementRows = false;
        $s.CheckAllEmployeeMovementRecords = function () {
        }
  
        //Click Post
        $s.Post = function () {
            $s.Request("Post", { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2)
                    $s.SetSystemStatus(ret.Message, 'error');
                else {
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
            $s.Request("Approve", { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Successfully Approved.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
                    $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
        }

        $s.setLookUp = function (e) {
            return {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Name' }
                    ]
                },
                Filters: [
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ],
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { LookUpName: e, MenuCode: $s.MenuCode }
            };
        }

        $s.dataLookUp = {
            accountnumbertype: $s.setLookUp('accountnumbertype'),
            branch: $s.setLookUp('branch'),
            company: $s.setLookUp('company'),
            currency: $s.setLookUp('currency'),
            department: $s.setLookUp('department'),
            designation: $s.setLookUp('designation'),
            division: $s.setLookUp('division'),
            jobclass: $s.setLookUp('jobclass'),
            jobclassgroup: $s.setLookUp('jobclassgroup'),
            leaveparameter: $s.setLookUp('leaveparameter'),
            parameter: $s.setLookUp('parameter'),
            paymentmode: $s.setLookUp('paymentmode'),
            payrollclassification: $s.setLookUp('payrollclassification'),
            payrollfrequency: $s.setLookUp('payrollfrequency'),
            payrollscheme: $s.setLookUp('payrollscheme'),
            payrollstatus: $s.setLookUp('payrollstatus'),
            section: $s.setLookUp('section'),
            bloodtype: $s.setLookUp('bloodtype'),
            citizenship: $s.setLookUp('citizenship'),
            civilstatus: $s.setLookUp('civilstatus'),
            designation: $s.setLookUp('designation'),
            gender: $s.setLookUp('gender'),
            nationality: $s.setLookUp('nationality'),
            religion: $s.setLookUp('religion'),
            sssstatus: $s.setLookUp('sssstatus'),
            employeestatus: $s.setLookUp('employeestatus')
        };

        //////INIT EVENT
        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

    }]);