angular.module('app')
    .controller('TrainingTrainingSummary', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.ApprovalRecordID = 0;
        $s.myController = 'TrainingSummary';

        $s.IsEligibleTraineesTabClicked = 0;
        $s.IsTrainingApplicantsTabClicked = 0;
        $s.IsApprovalHistoryTabClicked = 0;
        $s.EligibleTabClicked = 1;
        $s.Approver = {
            Comment : "" 
        }

        //FOR TABS
        $s.TabsCount = [0, 0];
        $s.Schema = {};
        $s.TableSchema = [];
        $s.ApprovalSchema = {};

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'ReferenceNumber', Label: 'Ref#' },
                { Name: 'Title', Label: 'Title' },
                { Name: 'Location', Label: 'Location' },
                { Name: 'CoveredDate', Label: 'Date' },
                { Name: 'StartAt', Label: 'Start At' },
                { Name: 'Status', Label: 'Status' },
                { Name: 'Posted', Label: 'Posted'},
                { Name: 'ApplicationClosed', Label: 'Application Closed' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'ReferenceNumber', Type: 9, Label: 'Ref#' },
                { Name: 'Title', Type: 9, Label: 'Title' },
                { Name: 'Posted', Type: 1, ControlType: 'radio', Label: 'Posted' },
            ]
        };

        $s.FilingStatusList = [];
        $s.PostStatusList = [];
        $s.CompanyList = [];
        $s.IsAllCompany = ($s.Session('ID_Company') == 0);
        $s.GetLookUp = function () {    
            
            $s.Request('LoadLookUp', { Name: 'FilingStatus' }, 'GetLookUp').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.FilingStatusList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.Request('LoadLookUp', { Name: 'TrainingPostStatus' }, 'GetLookUp').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.PostStatusList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });

            if ($s.IsAllCompany) {
                $s.Request('LoadLookUp', { Name: 'Company' }, 'GetLookUp').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.CompanyList = ret.Data.Rows;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }
                
        // BEGIN --- Load record
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.GetLookUp();
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.SetReportParams();
                        $s.SetSystemStatus('Ready');
                        console.log(ret.Data.Form, 123)
                    }
                    $s.$apply();
                })
                
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message);
                    } else {
                        $s.SetSystemStatus('Ready');
                        $s.TabsCount = [ret.Data.Total, ret.Data.ApprovalCount];
                        $s.InitTabs();
                    }
                    $s.$apply();
                });
            }
        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID }, $s.myController);
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        // END ----- Load record

        $s.DataAdjustment = function () {

            //Date Time Correction
            $s.Schema.StartTime = $s.ConcatDateAndTime($s.Schema.StartDate, $s.Schema.StartTime);
            console.log($s.Schema.StartTime)
            $s.Schema.EndTime = $s.Schema.EndTime != null && $s.Schema.EndTime != "" ? $s.ConcatDateAndTime($s.Schema.EndDate, $s.Schema.EndTime) : null;
        }

        $s.ConcatDateAndTime = function (date, time) {
            return date + ' ' + time;
        }

        //Save New - Edit - Delete
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.trainingsummary')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.DataAdjustment();
                $s.Request('SaveForm', {
                    Data: $s.Schema,
                    EligibleTrainees: $s.EligibleTraineesRecord,
                    DeletedEligibleTrainees: $s.DeletedEligibleTraineesRecord,
                    EmailTrainingSummary: $s.NewTrainingSummary
                }, $s.myController).then(function (ret) {
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
            $s.deleteRow('DeleteRecord', $s.myController)
        }

        $s.postTraining = function () {
            var ForApprovalNotifMsg = $s.ForApprovalNotifMsg();

            if ($s.Schema.Posted == true) return;

            $s.Request('PostRecord', {
                data: $s.Schema,
                ForApprovalNotifMsg: ForApprovalNotifMsg
            }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Posted.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
        }
        
        $s.cancelTraining = function () {
            if ($s.Schema.Posted == false) return;
            $s.Request('CancelRecord', { ID: $s.RecordID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Cancelled.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });
        }
                     
        //Look-Up
        $s.employeeLookup = {};
        $s.employeeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Employee' }
                ],
                Filters: [
                    { Name: 'Name', Type: 9, Label: 'Employee' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Employee' }
        };

        $s.departmentlookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Department' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Department' }
        };

        $s.designationlookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Designation' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'GetLookUp',
            parameter: { Name: 'Designation' }
        };
        
        $s._EligibleTraineesRecord = [];
        $s.EligibleTraineesRecord = [];
        $s.DeletedEligibleTraineesRecord = [];
        
        $s.LoadEligibleTrainees = function () {
            if ($s.IsEligibleTraineesTabClicked == 1) return;
            $s.Request('GetEligibleTrainee', { ID_Training: $s.Schema.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message);
                } else {

                    $s._EligibleTraineesRecord = ret.Data.Rows;
                    $s.EligibleTraineesRecord = angular.copy($s._EligibleTraineesRecord);
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.IsEligibleTraineesTabClicked = 1;
        }

        $s.DepartmentType = function (e) {
            if (e.ID_Type == 1)
                return true;
            else
                return false;
        }

        $s.DesignationType = function (e) {
            if (e.ID_Type == 2)
                return true;
            else
                return false;
        }

        $s.EmployeeType = function (e) {
            if (e.ID_Type == 3)
                return true;
            else return false;
        }

        $s.SetClickedTab = function (e) {
            $s.EligibleTabClicked = e
        }


        //Training Applicants Status
        $s.UpdateStatus = function () {

            if ($s.TrainingApplicantsRecord.length == 0) {
                $s.Prompt('No record found.');
                return;
            }

            $s.Request('UpdateTrainingApplicants', {
                TrainingApplicantsRecord: $s.TrainingApplicantsRecord
            }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                }
            });
        }


        //////////// NEW Eligible Trainee //////////////
        $s.newEligibleTrainees = function () {
            var NewRecord = {
                ID: 0,
                ID_Training: $s.Schema.ID,
                ID_Type: $s.EligibleTabClicked,
                ID_Value: null,
                Name: null
            }
            $s.EligibleTraineesRecord.push(NewRecord);
        }

        //////////// DELETE TRAINEES //////////////

        $s.SelectAllEligibleDepartmentRows = false;
        $s.CheckAllEligibleDepartmentRecords = function () {
            $s.SelectAllEligibleDepartmentRows = !$s.SelectAllEligibleDepartmentRows;
            Enumerable.From($s.EligibleTraineesRecord).ForEach(function (rowDetails) {
                if (rowDetails.ID_Type == 1) rowDetails.IsChecked = $s.SelectAllEligibleDepartmentRows;
            });
        }

        $s.SelectAllEligibleDesignationRows = false;
        $s.CheckAllEligibleDesignationRecords = function () {
            $s.SelectAllEligibleDesignationRows = !$s.SelectAllEligibleDesignationRows;
            Enumerable.From($s.EligibleTraineesRecord).ForEach(function (rowDetails) {
                if (rowDetails.ID_Type == 2) rowDetails.IsChecked = $s.SelectAllEligibleDesignationRows;
            });
        }

        $s.SelectAllEligibleEmployeeRows = false;
        $s.CheckAllEligibleEmployeeRecords = function () {
            $s.SelectAllEligibleEmployeeRows = !$s.SelectAllEligibleEmployeeRows;
            Enumerable.From($s.EligibleTraineesRecord).ForEach(function (rowDetails) {
                if (rowDetails.ID_Type == 3) rowDetails.IsChecked = $s.SelectAllEligibleEmployeeRows;
            });
        }

        $s.deleteEligibleTrainiees = function () {
            var rows = Enumerable
                .From($s.EligibleTraineesRecord)
                .Where(function (x) { return x.IsChecked == true && x.ID_Type == $s.EligibleTabClicked })
                .Select(function (x) { return x; }).ToArray();

           var rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.EligibleTraineesRecord.indexOf(rows[y]);
                        if ($s.EligibleTraineesRecord[Index].ID > 0) {
                            $s.DeletedEligibleTraineesRecord.push($s.EligibleTraineesRecord[Index].ID);
                        }
                        $s.EligibleTraineesRecord.splice(Index, 1);
                    }
                });
            }
        }

        /////////// VIEW TRAINEES ////////////////
        $s.TrainingApplicantsRecord = [];
        $s.FilingStatus = [];
        $s.LoadTrainingApplicants = function () {
            if ($s.IsTrainingApplicantsTabClicked == 1) return;

            $s.Request('LoadLookUp', { Name: "FilingStatus" }, 'GetLookUp').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message);
                } else {
                    $s.FilingStatus = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

            $s.Request('GetTrainingApplicant', { ID_Training: $s.Schema.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message);
                } else {
                    $s.TrainingApplicantsRecord = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');             }
                $s.$apply();
            });
            $s.IsTrainingApplicantsTabClicked = 1;
        }

        $s.CheckAllTrainingApplicantRecords = function () {
            return;
        }

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        ///APPROVAL OF TRAINING
        //tabs
        $s.Tabs = [];
        $s.CurrentTab = 0;
        $s.InitTabs = function () {
            //applicationTab
            $s.Tabs.push(angular.copy($s.tblOptions));
            //approvalTab
            var approvalTab = {
                Columns: [
                    { Name: 'ReferenceNumber', Label: 'Reference #' },
                    { Name: 'Title', Label: 'Title' },
                    { Name: 'StartDate', Label: 'Start Date' },
                    { Name: 'EndDate', Label: 'End Date' },
                    { Name: 'Location', Label: 'Location' },
                    { Name: 'CourseRating', Label: 'Course Rating' },
                    { Name: 'Status', Label: 'Status' },
                ],
                HasNew: $s.MenuPrivileges.HasNew,
                HasDelete: $s.MenuPrivileges.HasDelete,
                Filters: [
                ],
                openForm: function (row) {
                    $st.go($st.current.name + '.ApprovalForm', { ID: row.ID, ID_Employee: row.ID_Employee }, { reload: true })
                }
            };
            $s.Tabs.push(approvalTab);
        }

        $s.changeTab = function (idx) {
            $('.lt-item').removeClass('active');
            $('#lt-' + idx).addClass('active');
            $('.content-tab').removeClass('active');
            $('#ct-' + idx).addClass('active');
            $('.dp-item').removeClass('active');
            $('#dp-lt-' + idx).addClass('active');
            $('#d-lt-' + idx).addClass('active');
            $s.CurrentTab = idx;
            $s.LoadTable($s.Tabs[idx], (idx == 1 ? 'LoadTrainingSummaryApprovals' : 'LoadList'), $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Ready');
                    $s.Tabs[idx] = angular.copy($s.tblOptions);
                    $s.TabsCount[idx] = ret.Data.Total;
                }
                $s.$apply();
            });
        }
        
        $s.Approve = function () {
            var _SendStatusToEmployee = $s.SendStatusToEmployeeNotifMsg(1);
            var ForApprovalNotifMsg = $s.ForApprovalNotifMsg();
            $s.Request('Approve', {
                data: $s.Schema,
                ApproverComment: $s.Approver.Comment,
                SendStatusToEmployee: _SendStatusToEmployee,
                ForApprovalNotifMsg: ForApprovalNotifMsg
            }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $st.go($st.$current.parent.name, { ID: '' }, { reload: true });
                }
                $s.$apply();
            })
        }

        $s.Disapprove = function () {
            var _SendStatusToEmployee = $s.SendStatusToEmployeeNotifMsg(2);
            var ForApprovalNotifMsg = $s.ForApprovalNotifMsg();
        
            $s.Request('Disapprove', {
                data: $s.Schema,
                ApproverComment: $s.Approver.Comment,
                SendStatusToEmployee: _SendStatusToEmployee,
                ForApprovalNotifMsg: ForApprovalNotifMsg
            }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $st.go($st.$current.parent.name, { ID: '' }, { reload: true });
                }
                $s.$apply();
            })
        }

        $s.ApprovalHistory = [];
        $s.GetApprovalHistory = function (ReferenceID, TargetEmployee) {
            if ($s.ApprovalHistory.length == 0)
                $s.Request('LoadApprovalHistory', { ID: ReferenceID, Employee: TargetEmployee }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.ApprovalHistory = ret.Data;
                    }
                    $s.$apply();
                });
        }

        //APPROVE / DISAPPROVE - Email notifications messages
        $s.ForApprovalNotifMsg = function () {
            if ($s.Schema.ReferenceNumber != null) {
                return {
                    Title: 'For Approval',
                    Content: 'Training "' + $s.Schema.ReferenceNumber + '" is for approval.',
                    ID_Receiver: 0, //Send to next approver
                    ID_Sender: $s.Session('ID_User'),
                    ID_WebNotificationTypes: null,
                    ReferenceID: $s.Schema.ID
                }
            } else {
                console.log("No Training reference number is being created.");
            }
        };  

        //Update the employee about the status of Training
        $s.SendStatusToEmployeeNotifMsg = function (e) {
            var status = e == 1 ? 'Approved' : 'Disapproved';
            return {
                Title: 'Training Status',
                Content: 'Training "' + $s.Schema.ReferenceNumber + '-' + $s.Schema.Title + '" has been ' + status + ' by ',
                ID_Receiver: $s.Schema.ID_Employee, //Send to next approver
                ID_Sender: $s.Session('ID_User'),
                ID_WebNotificationTypes: null,
                ReferenceID: $s.Schema.ID
            }
        };

        //$s.UploadOptions = {
        //    IsMultiple: true,
        //    Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
        //    Method: 'UploadFile',
        //    Parameter: { folder: 'Files' }
        //};
        $s.rptOptions = [];
        $s.SetReportParams = function () {
            $s.rptOptions = {
                Pdf:
                {
                    Method: 'LoadTrainingBondReport',
                    Parameter: { ID: $s.Schema.ID, Source: 'TrainingSummary'},
                    Controller: $s.myController
                },
                Xls:
                {
                    Method: 'LoadExcelTrainingBondReport',
                    Parameter: { ID: $s.Schema.ID, Source: 'TrainingSummary' },
                    Controller: $s.myController
                }
            }
        }

    }]);