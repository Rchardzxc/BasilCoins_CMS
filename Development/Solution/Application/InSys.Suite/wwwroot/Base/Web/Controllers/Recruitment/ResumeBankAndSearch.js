angular.module('app')
    .controller('RecruitmentResumeBank&Search', ['$scope', '$controller', '$priv', '$state', '$timeout', function ($s, $c, $priv, $st, $t) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.IsLoadedInterviews = 0;
        $s.IsLoadedExaminations = 0;
        $s.IsLoadedJobOffer = 0;
        $s.InterviewerRecordCount = 0;
        $s.ExaminationRecordCount = 0;
        $s.Schema = [];
        $s.SchemaAddress = [];
        $s.SchemaCharRef = [];
        $s.SchemaEducational = [];
        $s.SchemaEmployment = [];
        $s.SchemaLicenses = [];
        $s.InterviewersTableSchema = [];
        $s.ExaminersTableSchema = [];
        $s.activeTab = 1;
        
        $s.EducationalAttainmentList = [];
        $s.ApplicationStatusList = [];
        $s.ResumeRows = [];
        $s.AppStatusFilter = null;
        $s.RecruitmentCount = [];
        $s.KioskMRFList = [];
        $s.TableSchema = [];

        $s.myController = 'ResumeBankAndSearch';
        $s.MenuCode = 'RecruitmentResumeBank&Search';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'RefNum', Label: 'Ref#' },
                { Name: 'LastName', Label: 'Last Name' },
                { Name: 'FirstName', Label: 'First Name' },
                { Name: 'MiddleInitial', Label: 'Middle Initial' },
                { Name: 'MRFNumber', Label: 'MRF#' },
                { Name: 'Designation', Label: 'Position Applying for' },
                { Name: 'Status', Label: 'Status' },
                { Name: 'TransDate', Label: 'Date Applied' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            Filters: [
                { Name: 'RefNum', Type: 9, Label: 'Ref#'},
                { Name: 'LastName', Type: 9, Label: 'Last Name' },
                { Name: 'FirstName', Type: 9, Label: 'First Name' },
                { Name: 'MiddleName', Type: 9, Label: 'Middle Name' },
                { Name: 'MRFNumber', Type: 9, Label: 'MRF#' },
                {
                    Name: 'ID_Designation', Label: "Position", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: 'GetLookUp',
                        method: 'LoadLookUp',
                        parameter: { Name: 'Position' },
                        tblOptions: {
                            Columns: [
                                { Name: 'ID', Label: '#' },
                                { Name: 'Name', Label: 'Position' }
                            ],
                            Filters: [
                                { Name: 'Name', Type: 9, ControlType: 'text', Label: 'Name' },
                            ]
                        }
                    }
                }
            ]
        };

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.GetLookUpList();
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                       
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.SchemaAddress = ret.Data.SchemaAddress;
                        $s.PresentAddress = $s.SchemaAddress.filter(x => { return (x.IsPresentAddress == true); } );
                        $s.PermanentAddress = $s.SchemaAddress.filter(x => { return (x.IsPresentAddress == false); });
                        $s.SchemaCharRef = ret.Data.SchemaCharRef.filter(x => { return (x.IsEmploymentReference == false); });
                        $s.SchemaCharRefPrevER = ret.Data.SchemaCharRef.filter(x => { return (x.IsEmploymentReference == true); });

                        $s.SchemaEducational = ret.Data.SchemaEducational;
                        $s.SchemaEmployment = ret.Data.SchemaEmployment;
                        $s.SchemaLicenses = ret.Data.SchemaLicenses;
                        $s.InterviewerRecordCount = ret.Data.InterviewRecordCount;
                        $s.ExaminationRecordCount = ret.Data.ExaminationRecordCount;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
            } else {
                $s.Request('ExecuteAction', { MenuCode: $s.MenuCode, MethodName: 'loadtablookup' }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.ApplicationStatusList = ret.Data.Rows;
                        $s.SetSystemStatus('Ready');
                        Enumerable.From($s.ApplicationStatusList).ForEach(function (a) {
                            $s.ResumeRows.push(angular.copy($s.tblOptions));
                        });
                        $s.hideExcessTab();
                        $s.changeTab(0, $s.ApplicationStatusList[0].ID);
                    }
                    $s.$apply();
                });
            }
        }

        $s.hideExcessTab = function () {
            $s.AllTabs = [];
            $s.ExcessTabs = [];
            angular.forEach($s.ApplicationStatusList, function (row, idx) {
                if (row.Name.length > 10) {
                    row.Width = (row.Name.length * 11.5);
                } else {
                    row.Width = 100;
                }
            });
            $s.AllTabs = angular.copy($s.ApplicationStatusList);
            var ttw = Enumerable.From($s.AllTabs).Select(function (x) { return x.Width }).Sum();
            var lwidth = ($('.list-tab').width());
            var tmp = lwidth - 70;
            while (ttw > lwidth) {
                var toInsert = $s.AllTabs.pop();
                if ($s.ExcessTabs.indexOf(toInsert) == -1)
                    $s.ExcessTabs.unshift(toInsert);
                ttw = Enumerable.From($s.AllTabs).Select(function (x) { return x.Width }).Sum();
            }
            $s.$apply();
        }

        $(window).resize(function () {
            $s.hideExcessTab();
        })

        $s.getRecruitmentCount = function (id) {
            return Enumerable.From($s.RecruitmentCount).Where(function (x) { return x.ID == id }).FirstOrDefault();
        }

        $s.changeTab = function (idx, id) {
            $('.lt-item').removeClass('active');
            $('#lt-' + idx).addClass('active');
            $('.content-tab').removeClass('active');
            $('#ct-' + idx).addClass('active');
            $('.dp-item').removeClass('active');
            $('#dp-lt-' + idx).addClass('active');
            $('#d-lt-' + idx).addClass('active');
            console.log($s.ResumeRows[idx])
            $s.AppStatusFilter = { Name: 'ID_RecruitmentStatus', Value: id, IsVisible: false }; 
            if ($s.ResumeRows[idx].Data != undefined) {
                if ($s.ResumeRows[idx].Data.Rows.length == 0) {
                    $s.LoadTable($s.ResumeRows[idx], 'LoadList', $s.myController, { ID_RecruitmentStatus: id, MenuCode: $s.MenuCode }).then(function (ret) {

                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.SetSystemStatus('Ready');
                            $s.RecruitmentCount = ret.Data.RecruitmentCount;
                            $s.ResumeRows[idx] = angular.copy($s.tblOptions);


                        }
                        $s.$apply();
                    });
                } else {
                    $s.tblOptions = angular.copy($s.ResumeRows[idx]);
                }
            } else {
                $s.LoadTable($s.ResumeRows[idx], 'LoadList', $s.myController, { ID_RecruitmentStatus: id, MenuCode: $s.MenuCode }).then(function (ret) {

                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Ready');
                        $s.RecruitmentCount = ret.Data.RecruitmentCount;
                        $s.ResumeRows[idx] = angular.copy($s.tblOptions);
                    }
                    $s.$apply();
                });
            }
            
            
        }

        $s.loadTab = function (id) {
            if (id == 2 && $s.IsLoadedInterviews != 1) {
                $s.GetInterviewRecords();
                $s.IsLoadedInterviews = 1;
            } else if (id == 3 && $s.IsLoadedExaminations != 1) {
                $s.GetExaminationRecords();
                $s.IsLoadedExaminations = 1;
            } else if (id == 4 && $s.IsLoadedJobOffer != 1) {
                $s.GetJobOfferRecords();
                $s.IsLoadedJobOffer = 1;
            }
        }

        $s.EmployeeWizard = function () {
             return $s.Dialog({
                template: 'EmployeeWizard',
                controller: 'dlgEmployeeWizard',
                size: 'lg',
                windowClass: 'employee-wizard-dlg',
                data: { Schema: $s.Schema }
            });
        }

        $s.TransferToOtherMRF = function () {

            $s.Dialog({
                template: 'ApplicantEndorse',
                controller: 'dlgApplicantEndorse',
                size: 'xs',
                windowClass: 'applicant-endorse-dlg',
                data: { Schema: $s.Schema }
            }).result.then(function (ret) {

                if (ret.Schema.IsEndorsed != $s.Schema.IsEndorsed && ret.Schema.IsEndorsed) {
                    $s.Prompt('Please save this record for official endorsement.');
                    $s.Schema.IsEndorsed = ret.Schema.IsEndorsed;
                }

                $s.Schema.ID_MRF = ret.Schema.ID_MRF;
                $s.Schema.ID_Designation = ret.Schema.ID_Designation;
            });
        }

        $s.Val = function (val) {
            return val || '-';
        }

        $s.getDDValue = function (table, val) {
            return Enumerable.From(table).Where(function (x) { return x.ID == val }).Select(function (x) { return x.Name }).ToArray()[0];
        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, $s.myController);
        }


        $s.UpdateInterviewerStatus = function (e) {
            if (e.ID_ExamAndInterviewStatus < 3)
                e.ID_InterviewerStatus = 3;
            else if (e.ID_ExamAndInterviewStatus == 3)
                e.ID_InterviewerStatus = 1;
        }

        //1 Unprocessed
        //2 Short Listed
        //3 Black Listed
        //4 Failed
        //5 Hired
        //15 Keep for Reference
        //17 For Examination
        //18 For Interview

        $s.ValidateBeforeSave = function () {
            var msg = null;
            var e = $s.Schema.ID_RecruitmentStatus;


            if (e == 5 || e == 18) { //Hired // Interview

                if (($s.InterviewRecords.length == 0 && $s.IsLoadedInterviews > 0) || ($s.InterviewerRecordCount == 0 && $s.IsLoadedInterviews == 0)) //If no Interview Records
                    msg = '"Interviewer" records are required';

            } else if (e == 17) { //Examination
                if (($s.ExaminationRecords.length == 0 && $s.IsLoadedExaminations > 0) || ($s.ExaminationRecordCount == 0 && $s.IsLoadedExaminationRecordCount == 0)) //If no Examination Records
                    msg = '"Examiner" records are required';
            }

            /////Validate Interview Record - CHECK conflict with Schedules
            var InterviewMsg = $s.ValidateInterviewRecords();
            var ExaminationMsg = $s.ValidateExaminationRecords();

            if (InterviewMsg != "") msg = InterviewMsg;
            if (ExaminationMsg != "") msg = ExaminationMsg
            

            console.log(msg);

            if (msg != null) {
                $s.Prompt(msg);
                $s.SetSystemStatus(msg, 'error');
                return false;
            } else return true;

        }

        $s.saveForm = function () {

            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');

            //INTERVIEWERS TAB VALIDATION
            if ($s.InterviewersTableSchema.length > 0)
                if (!$s.IsTabsValid('form.resumeBankAndSearch', $s.InterviewersTableSchema, 'interviews'))return;

            //EXAMINERS TAB VALIDATION
            if ($s.ExaminersTableSchema.length > 0)
                if (!$s.IsTabsValid('form.resumeBankAndSearch', $s.ExaminersTableSchema, 'examination'))return;

            //JOBOFFER TAB VALIDATION
            if ($s.JobOfferTableSchema.length > 0)
                if (!$s.IsTabsValid('form.resumeBankAndSearch', $s.JobOfferTableSchema, 'joboffer'))return;

            //var validatedMessage =

            if (!$s.ValidateBeforeSave()) return;


            console.log($s.ValidateBeforeSave());


            //if (validatedMessage != '') {
            //        $s.Prompt(validatedMessage);
            //        $s.SetSystemStatus(validatedMessage, 'error');
            //        return;
            //}

            //if object is empty set it to null
            //if (angular.equals($s.JobOffer, {})) { $s.JobOffer = null; } else $s.SetJobOffer(); //Set Employee Status Name, will be used for reporting

            if ($s.Schema.ID_RecruitmentStatus == 5) {
                $s.EmployeeWizard().result.then(function (ret) {
                    $s.Request('SaveForm', {
                        Data: $s.Schema, ExaminationData: $s.ExaminationRecords, DeletedExaminationData: $s.DelExaminationRecords,
                        InterviewData: $s.InterviewRecords, DeleteInterviewData: $s.DelInteviewRecords, ForHiringData: $s.ForHiringRecords,
                        DeleteForHiringData: $s.DelForHiringRecords, Employee: ret.tEmployee, Approver: ret.FilingModules, JobOffer: angular.equals($s.JobOffer, {}) ? null : $s.JobOffer,
                        MenuCode: $s.MenuCode
                    }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                            $s.$apply();
                        } else {
                            $s.Prompt('Applicant hired successful.');
                            $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                        }
                    });
                });
            }
            else {
                $s.Request('SaveForm', {
                    Data: $s.Schema, ExaminationData: $s.ExaminationRecords, DeletedExaminationData: $s.DelExaminationRecords,
                    InterviewData: $s.InterviewRecords, DeleteInterviewData: $s.DelInteviewRecords, ForHiringData: $s.ForHiringRecords,
                    DeleteForHiringData: $s.DelForHiringRecords, Employee: null, Approver: null,
                    JobOffer: angular.equals($s.JobOffer, {}) ? null : $s.JobOffer,
                    MenuCode: $s.MenuCode
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

        $s.GetLookUpList = function () {
            $s.Request('LoadLookUp', { Name: 'ApplicationStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ApplicationStatusList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'EducationalAttainment', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.EducationalAttainmentList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            //$s.Request('LoadEndorseLookUp', { Name: 'MRF', MenuCode: $s.MenuCode }, 'GetResumeLookUp').then(function (ret) {
            $s.Request('LoadLookUp', { Name: 'MRF', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.KioskMRFList = ret.Data.Rows;

                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }
        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

        /////////BEGIN EXAMINATION RECORD/////////
        $s.employeeLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Employee' }
                ]
            },
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Employee' },
                //{ Name: 'ID_AssignedEmployee', Type: 1, ControlType: 'text', Label: 'Employee', Options: { controller: 'GetLookUp', method: 'LoadLookUp', parameter: { Name: 'Employee' } } }
            ],
            method: 'LoadLookUp',
            controller: $s.myController,
            parameter: { Name: 'Employee', MenuCode: $s.MenuCode }
        };

        $s.ExaminationRecords = [];
        $s.DelExaminationRecords = [];
        $s.ExaminationList = [];
        $s.ExamAndInterviewStatusList = [];
        $s._ExamAndInterviewStatusList = [];
        $s.ExaminationStatus = [];
        $s.InterviewFindings = [];    

        $s.GetExaminationAndInterviewLookUp = function () {

            $s.Request('LoadLookUp', { Name: 'ExaminationAndInterviewStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s._ExamAndInterviewStatusList = ret.Data.Rows;
                    $s.ExamAndInterviewStatusList = angular.copy($s._ExamAndInterviewStatusList);
                    $s.InterviewFindings = $s.ExamAndInterviewStatusList.filter(x => x.ID < 4);
                    $s.ExaminationStatus = angular.copy($s._ExamAndInterviewStatusList);
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

            $s.Request('LoadLookUp', { Name: 'Examinations', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ExaminationList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.GetExaminationRecords = function () {
            $s.GetExaminationAndInterviewLookUp();
            $s.Request('ExecuteAction', { ID_ApplicationForm: $s.Schema.ID, MenuCode: $s.MenuCode, MethodName: 'getexaminationrecord' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    
                    $s.ExaminationRecords = ret.Data;
                    $s.GetTableSchema('tApplicationFormExamination').then(function (ret) {
                        $s.ExaminersTableSchema = ret.Data;
                    })
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.SelectAllExaminationRows = false;
        $s.CheckAllExaminationRecords = function () {
            $s.SelectAllExaminationRows = !$s.SelectAllExaminationRows;
            Enumerable.From($s.ExaminationRecords).Where(x => x.ID == 0).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllExaminationRows;
            });
        }

        $s.delExaminationRecord = function () {
            var rows = Enumerable
                .From($s.ExaminationRecords)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; } ).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.ExaminationRecords.indexOf(rows[y]);
                        if ($s.ExaminationRecords[Index].ID > 0) {
                            $s.DelExaminationRecords.push($s.ExaminationRecords[Index].ID);
                        }
                        $s.ExaminationRecords.splice(Index, 1);
                    }
                });
            }
        }

        $s.newExaminationRecord = function () {
            var newRecord = {
                ID: 0,
                ID_Exams: 0,
                ID_ExamAndInterviewStatus: 3,
                ID_AssignedEmployee: null,
                ID_ApplicationForm: $s.Schema.ID,
                ScheduledDate: null,
                ExpectedStartTime: null,
                ExpectedEndTime: null,
                Notes: null,
                TotalScore: 0,
                PercentageBased: 0.00,
                ResultDocument: null,
                ResultDocumentGUID: null,
                IsNotify: false
            }
            $s.ExaminationRecords.push(newRecord);
        }
        $s.Today = new Date(new Date().setHours(0, 0, 0, 0));
        /////////END EXAMINATION RECORD/////////

        /////////BEGIN INTERVIEW RECORD/////////
        $s.InterviewRecords = [];
        $s.InterviewStatusRecords = [];
        $s.DelInteviewRecords = [];

        $s.GetInterviewRecords = function () {
            $s.GetExaminationAndInterviewLookUp();
            $s.Request('ExecuteAction', { ID_ApplicationForm: $s.Schema.ID, MenuCode: $s.MenuCode, MethodName: 'getinterviewrecord' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.InterviewRecords = ret.Data;
                    $s.GetTableSchema('tApplicationFormInterviews').then(function (ret) {
                        $s.InterviewersTableSchema = ret.Data;
                    })
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            }); 

            $s.Request('LoadLookUp', { Name: 'InterviewStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.InterviewStatusRecords = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.SelectAllInterviewRows = false;
        $s.CheckAllInterviewRecords = function () {
            $s.SelectAllInterviewRows = !$s.SelectAllInterviewRows;
            Enumerable.From($s.InterviewRecords).Where(x => x.ID == 0).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllInterviewRows;
            });
        }

        $s.delInterviewRecord = function () {
            var rows = Enumerable
                .From($s.InterviewRecords)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray(); //, ID : x.ID

            var errorMsg = "";
            var rowLength = rows.length;

            if (rowLength > 0) {

                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.InterviewRecords.indexOf(rows[y]);

                        if ($s.InterviewRecords[Index].ID_InterviewerStatus == 3) {
                            errorMsg += "Row #" + (Index + 1).toString() + " : Cannot be deleted, status is already Completed.";
                        } else {
                            if ($s.InterviewRecords[Index].ID > 0) {
                                $s.DelInteviewRecords.push($s.InterviewRecords[Index].ID);
                            }
                            $s.InterviewRecords.splice(Index, 1);
                        }
                    }
                    if (errorMsg != "") {
                        $s.SetSystemStatus(errorMsg, 'error');
                        $s.Prompt(errorMsg);
                    }
                });

            }
        }

        $s.newInterviewRecord = function () {
            var newRecord = {
                ID: 0,
                ID_AssignedEmployee: null,
                ID_ExamAndInterviewStatus: 3,
                ID_InterviewerStatus: 1,
                ID_ApplicationForm: $s.Schema.ID,
                ScheduledDate: null,
                ScheduledTime: null,
                Notes: null,
                IsNotify: false,
                Attachment: null
            }
            $s.InterviewRecords.push(newRecord);
        }

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };
        $s.disableDetail = function (row) {
            return (row.ID > 0 ? true : false);
        }
        $s.sendInterviewerNotification = function (row) {
            var hour = row.ScheduledTime.split(':')[0];
            var minute = row.ScheduledTime.split(':')[1].split(' ')[0];
            var format = row.ScheduledTime.split(' ')[1];
            var increment = (format.toLowerCase() == "pm" ? (parseInt(hour) == 12 ? 0 : 12) : 0);
            var interviewer = {
                Title: 'Interview Schedule',
                Content: $s.Schema.FirstName + ' ' + $s.Schema.LastName + ' has an interview with you on ' +
                    moment(new Date(row.ScheduledDate)).format('MMM DD') + ' ' +
                    moment(new Date(new Date().setHours(parseInt(hour) + increment, parseInt(minute)))).format('hh:mm A') + '.',
                ID_Receiver: row.ID_Receiver,
                ID_Sender: $s.Session('ID_User'),
                ID_WebNotificationTypes: 1
            };
            $s.SetSystemStatus('Sending notification...', 'loading');
            $s.Request('SendInterviewerNotification', { data: interviewer, ID: row.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    row.IsNotify = true;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.sendExaminerNotification = function (row) {
            var hour = row.ExpectedStartTime.split(':')[0];
            var minute = row.ExpectedStartTime.split(':')[1].split(' ')[0];
            var format = row.ExpectedStartTime.split(' ')[1];
            var increment = (format.toLowerCase() == "pm" ? (parseInt(hour) == 12 ? 0 : 12) : 0);

            var hour2 = row.ExpectedEndTime.split(':')[0];
            var minute2 = row.ExpectedEndTime.split(':')[1].split(' ')[0];
            var format2 = row.ExpectedEndTime.split(' ')[1];
            var increment2 = (format2.toLowerCase() == "pm" ? (parseInt(hour2) == 12 ? 0 : 12) : 0);

            var examiner = {
                Title: 'Exam Schedule',
                Content: $s.Schema.FirstName + ' ' + $s.Schema.LastName + ' has an exam with you on ' +
                    moment(new Date(row.ScheduledDate)).format('MMM DD') + ' from ' +
                    moment(new Date(new Date().setHours(parseInt(hour) + increment, parseInt(minute)))).format('hh:mm A') + ' to ' +
                    moment(new Date(new Date().setHours(parseInt(hour2) + increment2, parseInt(minute2)))).format('hh:mm A') + '.',
                ID_Receiver: row.ID_Receiver,
                ID_Sender: $s.Session('ID_User'),
                ID_WebNotificationTypes: 2
            };
            $s.SetSystemStatus('Sending notification...', 'loading');
            $s.Request('SendExaminerNotification', { data: examiner, ID: row.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    row.IsNotify = true;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.DisableExaminationAndInterviewButtons = function () {
            var e = $s.Schema.ID_RecruitmentStatus;
            if (e == 3 || e == 4) {
                return true;
            } else {
                return false;
            }
        }

        $s.ConvertTimeformat = function (val) {
            var time = val;
            var hours = Number(time.match(/^(\d+)/)[1]);
            var minutes = Number(time.match(/:(\d+)/)[1]);
            var AMPM = time.match(/\s(.*)$/)[1];
            if (AMPM == "PM" && hours < 12) hours = hours + 12;
            if (AMPM == "AM" && hours == 12) hours = hours - 12;
            var sHours = hours.toString();
            var sMinutes = minutes.toString();
            if (hours < 10) sHours = "0" + sHours;
            if (minutes < 10) sMinutes = "0" + sMinutes;
            return sHours;
        }

        ///////Validate Interview Records
        $s.ValidateInterviewRecords = function () {

            var _InterviewRecords = angular.copy($s.InterviewRecords);
            var _InterviewRecords2 = angular.copy($s.InterviewRecords);
            var _ExaminationRecords = angular.copy($s.ExaminationRecords);

            var TimeInterval = 30; //30 minutes
            var ReturnMsg = "";
            var ValidateConflictStatusRecord = "";
            var X_Index = 0;

            _InterviewRecords.forEach(function (x) {
                X_Index += 1;

                ////Check Conflict on Findings and InterviewStatus
                if (x.ID_InterviewerStatus == 3 && x.ID_ExamAndInterviewStatus != 1 && x.ID_ExamAndInterviewStatus != 2) {
                    ValidateConflictStatusRecord += X_Index.toString() + ", ";
                }

                if (x.ScheduledTime == null || x.ScheduledDate == null) return;
                var X_ConvertedMinutes = $s.ConvertTimeformat(x.ScheduledTime) * 60;
                var X_Minutes = new Date(x.ScheduledDate + ' ' + x.ScheduledTime).getMinutes();
                var X_TotalMinutes = (X_ConvertedMinutes + X_Minutes);

                var E_Index = 0;
                var E_Rows = "";

                _InterviewRecords2.forEach(function (e) {
                    E_Index += 1;
                    if (X_Index == E_Index) return; //Do not validate the record itself
                    if (e.ScheduledDate == null || e.ScheduledTime == null) return; //Date/Time should not null
                    if (x.ScheduledDate != e.ScheduledDate) return; //Records must be the same date

                    var E_ConvertedMinutes = $s.ConvertTimeformat(e.ScheduledTime) * 60;
                    var E_Minutes = new Date(e.ScheduledDate + ' ' + e.ScheduledTime).getMinutes();
                    var E_TotalMinutes = (E_ConvertedMinutes + E_Minutes);

                    var E_DiffMinutes = (X_TotalMinutes - E_TotalMinutes);

                    if ((E_DiffMinutes < TimeInterval) && (E_DiffMinutes > (TimeInterval * -1))) {
                        E_Rows += (E_Index).toString() + ", ";
                    }
                });

                //////Validate to Examination Record
                var EX_Index = 0;
                var EX_Rows = "";
                _ExaminationRecords.forEach(function (ex) {
                    EX_Index += 1;

                    if (ex.ScheduledDate == null || ex.ScheduledDate != x.ScheduledDate) return; //examination record must same date with interview
                    if (ex.ExpectedStartTime == null || ex.ExpectedEndTime == null) return; //examination record must not null

                    //Convert hours into minutes
                    var EX_StartTimeTotalMinutes = $s.ConvertTimeformat(ex.ExpectedStartTime) * 60;
                    var EX_EndTimeTotalMinutes = $s.ConvertTimeformat(ex.ExpectedEndTime) * 60;
                    //Add Converted Hours to Remaining Minutes
                    EX_StartTimeTotalMinutes = new Date(ex.ScheduledDate + ' ' + ex.ExpectedStartTime).getMinutes() + EX_StartTimeTotalMinutes;
                    EX_EndTimeTotalMinutes = new Date(ex.ScheduledDate + ' ' + ex.ExpectedEndTime).getMinutes() + EX_EndTimeTotalMinutes;

                    if (X_TotalMinutes >= EX_StartTimeTotalMinutes && X_TotalMinutes <= EX_EndTimeTotalMinutes)
                        EX_Rows += EX_Index.toString() + ", ";
                });

                if (E_Rows != "") {
                    ReturnMsg += "Interviewers-Row# " + X_Index.toString() + ": Provide at least 30 mins. interval from record# [" + E_Rows.substring(0, E_Rows.length - 2) + "].";
                }

                if (EX_Rows != "") {
                    ReturnMsg += "Interviewers-Row# " + X_Index.toString() + ": Conflict with Examiner record# [" + EX_Rows.substring(0, EX_Rows.length - 2) + "].";
                }

                E_Rows = "";
                EX_Rows = "";
            });

            if (ValidateConflictStatusRecord != "")
                ReturnMsg = "Rows#[" + ValidateConflictStatusRecord.substring(0, ValidateConflictStatusRecord.length - 2) + "]- Findings must be Passed or Failed for Completed status." + ReturnMsg;

            return ReturnMsg;
        }

        /////Validate EXAMINATION RECORDS
        $s.ValidateExaminationRecords = function () {
            var _ExaminationRecords = angular.copy($s.ExaminationRecords);
            var _ExaminationRecords2 = angular.copy($s.ExaminationRecords);

            var ReturnMsg = "";
            var ConflictStartMsg = "";
            var ConflictEndMsg = "";

            var X_Index = 0;
            _ExaminationRecords.forEach(function (x) {
                X_Index += 1;
                if (x.ScheduledDate == null || x.ExpectedStartTime == null || x.ExpectedEndTime == null) return;
                    //Convert hours into minutes
                var X_StartTimeTotalMinutes = $s.ConvertTimeformat(x.ExpectedStartTime) * 60;
                var X_EndTimeTotalMinutes = $s.ConvertTimeformat(x.ExpectedEndTime) * 60;

                //Add Converted Hours to Remaining Minutes
                X_StartTimeTotalMinutes = new Date(x.ScheduledDate + ' ' + x.ExpectedStartTime).getMinutes() + X_StartTimeTotalMinutes;
                X_EndTimeTotalMinutes = new Date(x.ScheduledDate + ' ' + x.ExpectedEndTime).getMinutes() + X_EndTimeTotalMinutes;

                var Smsg = "";
                var Emsg = "";
                var E_Index = 0;
                _ExaminationRecords2.forEach(function (e) {
                    E_Index += 1;
                    if (e.ScheduledDate == null || e.ExpectedStartTime == null || e.ExpectedEndTime == null) return;

                    if (X_Index != E_Index && x.ScheduledDate == e.ScheduledDate) {
                        //Convert hours into minutes
                        var E_StartTimeTotalMinutes = $s.ConvertTimeformat(e.ExpectedStartTime) * 60;
                        var E_EndTimeTotalMinutes = $s.ConvertTimeformat(e.ExpectedEndTime) * 60;
                        //Add Converted Hours to Remaining Minutes
                        E_StartTimeTotalMinutes = new Date(e.ScheduledDate + ' ' + e.ExpectedStartTime).getMinutes() + E_StartTimeTotalMinutes;
                        E_EndTimeTotalMinutes = new Date(e.ScheduledDate + ' ' + e.ExpectedEndTime).getMinutes() + E_EndTimeTotalMinutes;

                        //Check all the rows/records with conflict.
                        if (X_StartTimeTotalMinutes >= E_StartTimeTotalMinutes && X_StartTimeTotalMinutes <= E_EndTimeTotalMinutes)
                            Smsg += E_Index.toString() + ", ";
                        else if (X_EndTimeTotalMinutes >= E_StartTimeTotalMinutes && X_EndTimeTotalMinutes <= E_EndTimeTotalMinutes)
                            Emsg += E_Index.toString() + ", ";
                    }
                    
                });

                if (Smsg != "") ConflictStartMsg += "Examiners-Row# " + X_Index.toString() + ": Start Time is conflict with record# [" + Smsg.substring(0, Smsg.length - 2) + "]."
                
                if (Emsg != "") ConflictEndMsg += "Examiners-Row# " + X_Index.toString() + ": End Time is conflict with record# [" + Emsg.substring(0, Emsg.length - 2) + "]."
                
                Smsg = "";
                Emsg = "";
                
            });

            if (ConflictEndMsg != "" || ConflictStartMsg != "") {
                ReturnMsg = (ConflictStartMsg + ConflictEndMsg);
            }
            return ReturnMsg;
        }

        $s.ViewPDF = function () {
            $s.Dialog({
                template: 'InSysReport',
                controller: ['$scope', '$uibModalInstance', function ($ss, $mi) {
                    $ss.close = function () {
                        $mi.dismiss();
                    }
                    $ss.rptOptions = {
                        Pdf:
                        {
                            Method: 'LoadPDF',
                            Parameter: { ID: $s.Schema.ID, MenuCode: $s.MenuCode },
                            Controller: $s.myController
                        },
                        hasExportXls: false,
                        hasLoadPdf: false
                    };
                    $ss.$watch('rptOptions.reportReady', function (nv, ov) {
                        if (nv == true) {
                            $ss.rptOptions.triggerLoadPdf();
                        }
                    })
                }],
                windowClass: ' insys-report-dlg',
                size: 'lg'
            })
        }

       

        //JOB OFFER Codes
        $s.SetJobOffer = function () {
            if ($s.EmployeeStatusListForJO.filter(x => (x.ID == $s.JobOffer.ID_EmployeeStatus)).length > 1)
                $s.JobOffer.EmployeeStatus = $s.EmployeeStatusListForJO.filter(x => (x.ID == $s.JobOffer.ID_EmployeeStatus))[0].Name;

            if ($s.NatureOfRequestsJO.filter(x => (x.ID == $s.JobOffer.ID_NatureOfRequests)) > 1)
                $s.JobOffer.NatureOfRequests = $s.NatureOfRequestsJO.filter(x => (x.ID == $s.JobOffer.ID_NatureOfRequests))[0].Name;

            if ($s.CostCenterListJO.filter(x => (x.ID == $s.JobOffer.ID_CostCenter)) > 1)
                $s.JobOffer.CostCenter = $s.CostCenterListJO.filter(x => (x.ID == $s.JobOffer.ID_CostCenter))[0].Name;
        }

        $s.JobOffer = {};
        $s._JobOffer = {};
        $s.EmployeeStatusListForJO = [];
        $s.CostCenterListJO = [];
        $s.NatureOfRequestsJO = [];

        $s.GetJobOfferLookUp = function () {
            $s.Request('LoadLookUp', { Name: 'EmployeeStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.EmployeeStatusListForJO = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

            $s.Request('LoadLookUp', { Name: 'CostCenter', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.CostCenterListJO = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

            $s.Request('LoadLookUp', { Name: 'NatureOfRequests', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.NatureOfRequestsJO = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.SetLookupOptions = function () {
            $s.JobClassLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Job Class' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'JobClass', MenuCode: $s.MenuCode }
            };
            $s.DepartmentLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Department' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'Department', MenuCode: $s.MenuCode }
            };
            $s.DivisionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Division' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'Division', MenuCode: $s.MenuCode }
            };
        }
        $s.JobOfferTableSchema = [];
        $s.GetJobOfferRecords = function () {
            $s.Request('ExecuteAction', { ID: $s.Schema.ID, MenuCode: $s.MenuCode, MethodName: 'loadjoboffer' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.JobOffer = ret.Data;
                    $s.JobOffer.ID_ApplicationForm = $s.Schema.ID;
                    $s._JobOffer = angular.copy($s.JobOffer);
                    $s.GetJobOfferLookUp();
                    $s.SetLookupOptions();
                    $s.GetTableSchema('tEmployee_JobOffer').then(function (ret) {
                        $s.JobOfferTableSchema = ret.Data;
                    });
                    $s.SetSystemStatus('Ready');
                }
            });
        }
       
        $s.ComputeAnnualSalary = function () {
            var mult = 13; //13 months
            $s.JobOffer.AnnualizedSalary = ($s.JobOffer.Salary * mult);
        }

        $s.LoadJOReport = function () {

        }
    }])
    .controller('dlgApplicantEndorse', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });

        $s.Schema = $dlgData.Schema;
        $s.myController = 'ResumeBankAndSearch';
        $s.MenuCode = 'RecruitmentResumeBank&Search';

        //////////Create filtering on MRF, based from Designation
        $s.filterMRF = function (e) {
            var _mrfList = angular.copy($s._mrfList);
            var mrfList = Enumerable
                .From(_mrfList)
                .Where(function (x) { return x.ID_Designation == e }).ToArray();
            $s.mrfList = mrfList;

            if (e == 0 || e == null || mrfList.length == 0)
                $s.Schema.ID_MRF = null;
            else $s.Schema.ID_MRF = mrfList[0].ID;
        }

        $s.mrfLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Position' },
                    { Name: 'MRFNumber', Label: 'MRF Number' }
                ]
            },
            method: 'ExecuteAction',
            controller: $s.myController,
            parameter: { MenuCode: $s.MenuCode, MethodName: 'loadmrf' }
        };

        $s.getSchema = function (row) {
            $s.Schema.MRFNumber = row.MRFNumber;
            $s.Schema.ID_MRF = row.ID_MRF;
        }

        $s.close = function () {
            var results = { Schema: $s.Schema };
            $mi.close(results);
        }

        $s.endorse = function () {
            var results = [];
            $s.Schema.IsEndorsed = true;
            results = { Schema: $s.Schema, ExaminationRecords: $s.ExaminationRecords, InterviewRecords: $s.InterviewRecords };
            $mi.close(results);
        }
    }])
    .controller('dlgEmployeeWizard', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });

        $s.Schema = $dlgData.Schema;
        $s.currentEmpTab = 0;
        $s.tEmployee = {};
        $s.tblSchema = [];
        $s.tOrganization = {};
        $s.DateToday = new Date(new Date().setHours(0, 0, 0, 0));

        $s.EmpRecordLoaded = false;
        $s.PayrollLoaded = false;
        $s.OrganizationLoaded = false;

        $s.WeeklyScheduleList = [];
        $s.CostCenterList = [];
        $s.FacultyTypeList = [];
        $s.FacultyInstituteList = [];
        $s.CurrencyList = [];
        $s.PayrollSchemeList = [];
        $s.PayrollFrequencyList = [];
        $s.PaymentModeList = [];
        $s.ParameterList = [];
        $s.LeaveParameterList = [];
        $s.PayrollStatusList = [];
        $s.PayrollClassificationList = [];
        $s.AccountNumberTypeList = [];
        $s.CompanyBankAcctList = [];
        $s.TaxExemptionList = [];
        $s.EmployeeStatusList = [];

        $s.myController = 'ResumeBankAndSearch';
        $s.MenuCode = 'RecruitmentResumeBank&Search';


        $s.init = function () {
            $s.Request('ExecuteAction', { Name: 'tEmployee', MenuCode: $s.MenuCode, MethodName: 'loadschema' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Ready');
                    $s.tEmployee = $s.PlotDefault($s.tEmployee, ret.Data, 0);
                    $s.tblSchema = ret.Data;
                    $s.tEmployee.IsHired = true;
                    //$s.Organization();
                }
            });
            $s.loadTabLookup();
        }

        $s.EmpTab = function (idx) {
            $('.ew-body').scrollTop(0);
            $s.currentEmpTab = idx;
            $('.ew-tab-item').removeClass('active');
            $('.ew-tab-content').removeClass('active');
            $('#ew-tc-' + idx).addClass('active');
            $('#ew-ti-' + idx).addClass('active');

            $s.loadTabLookup();
        }

        $s.Prev = function () {
            $('.ew-body').scrollTop(0);
            if ($s.currentEmpTab > 0) {
                $s.currentEmpTab = $s.currentEmpTab - 1;
                $('.ew-tab-item').removeClass('active');
                $('.ew-tab-content').removeClass('active');
                $('#ew-tc-' + $s.currentEmpTab).addClass('active');
                $('#ew-ti-' + $s.currentEmpTab).addClass('active');
            }
            $s.loadTabLookup();
        }

        $s.Next = function () {
            if ($s.currentEmpTab < 4) {
                $s.currentEmpTab = $s.currentEmpTab + 1;
                $('.ew-tab-item').removeClass('active');
                $('.ew-tab-content').removeClass('active');
                $('#ew-tc-' + $s.currentEmpTab).addClass('active');
                $('#ew-ti-' + $s.currentEmpTab).addClass('active');
            }

            if ($s.currentEmpTab > 3) {
                var data = { tEmployee: $s.tEmployee, FilingModules: $s.FilingModulesSelected };
                $mi.close(data);
                //$s.Request('saveWizard', { Data: $s.tEmployee }, 'ResumeBankAndSearch').then(function (ret) {
                //    if (ret.Type == 2) {
                //        $s.SetSystemStatus(ret.Message, 'error');
                //        $s.$apply();
                //        //$s.close();
                //    } else {
                //        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                //        $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                //        //$s.close();
                //    }
                //});
            }

            $s.loadTabLookup();
        }

        $s.loadTabLookup = function () {
            if ($s.currentEmpTab == 0 && $s.EmpRecordLoaded == false) {
                $s.EmpRecord();
            }
            if ($s.currentEmpTab == 1 && $s.PayrollLoaded == false) {
                $s.Payroll();
            }
            if ($s.currentEmpTab == 3 && $s.OrganizationLoaded == false) {
                
            }
        }

        $s.companyLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Branch' }
                ]
            },
            method: 'LoadLookUp',
            controller: $s.myController,
            parameter: { Name: 'Company', MenuCode: $s.MenuCode }
        };



        $s.branchLookup = {};
        $s.divisionLookup = {};
        $s.departmentLookup = {};
        $s.sectionLookup = {};
        $s.jobClassGroupLookup = {};
        $s.jobClassLookup = {};
        $s.designationLookup = {};

        $s.setBranchLookUp = function () {
            $s.branchLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Company' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgBranch', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_Company }
            };
        }

        $s.setDivisionLookup = function () {
            $s.divisionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Division' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgDivision', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_Branch }
            };
        }

        $s.setDepartmentLookUp = function () {
            $s.departmentLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Department' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgDepartment', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_Division }
            };
        }
        
        $s.setSectionLookup = function () {
            $s.sectionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Section' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgSection', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_Department }
            };
        }

        $s.setJobClassGroupLookup = function () {
            $s.jobClassGroupLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Job Class Group' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgJobClassGroup', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_Section }
            };
        }

        $s.setJobClassLookup = function () {
            $s.jobClassLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Job Class' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgJobClass', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_JobClassGroup }
            };
        }
        
        $s.setDesignationLookup = function () {
            $s.designationLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Designation' }
                    ]
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'OrgDesignation', MenuCode: $s.MenuCode, ID_Parent: $s.tEmployee.ID_JobClass }
            };
        }

        $s.setBranchLookUp();
        $s.setDivisionLookup();
        $s.setDepartmentLookUp();
        $s.setSectionLookup();
        $s.setJobClassGroupLookup();
        $s.setJobClassLookup();
        $s.setDesignationLookup();

        $s.EmpRecord = function () {
            $s.EmpRecordLoaded = true;
            //$s.Request('LoadLookUp', { Name: 'WeeklySchedule', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
            //    if (ret.Type == 2) {
            //        $s.SetSystemStatus(ret.Message, 'error');
            //    } else {
            //        $s.WeeklyScheduleList = ret.Data.Rows;
            //        $s.SetSystemStatus('Ready');
                    
            //    }
            //    $s.$apply();
            //});
            $s.Request('LoadLookUp', { Name: 'CostCenter', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.CostCenterList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'FacultyType', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.FacultyTypeList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'FacultyInstitute', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.FacultyInstituteList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'EmployeeStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.EmployeeStatusList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.Payroll = function () {
            $s.PayrollLoaded = true;
            $s.Request('LoadLookUp', { Name: 'Currency', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.CurrencyList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'PayrollScheme', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.PayrollSchemeList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'PayrollFrequency', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.PayrollFrequencyList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'PaymentMode', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.PaymentModeList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'Parameter', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ParameterList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'LeaveParameter', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.LeaveParameterList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'PayrollStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.PayrollStatusList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'PayrollClassification', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.PayrollClassificationList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'AccountNumberType', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.AccountNumberTypeList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'CompanyBankAcct', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.CompanyBankAcctList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            $s.Request('LoadLookUp', { Name: 'TaxExemption', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.TaxExemptionList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.Organization = function () {
            $s.OrganizationLoaded = true;
            $s.Request('LoadOrganization', { ID: $s.Schema.ID_MRF, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Ready');
                    $s.tOrganization = ret.Data;
                    $s.tEmployee.Company = $s.tOrganization.Company;
                    $s.tEmployee.ID_Company = $s.tOrganization.ID_Company;
                    $s.tEmployee.Branch = $s.tOrganization.Branch;
                    $s.tEmployee.ID_Branch = $s.tOrganization.ID_Branch;
                    $s.tEmployee.Division = $s.tOrganization.Division;
                    $s.tEmployee.ID_Division = $s.tOrganization.ID_Division;
                    $s.tEmployee.Department = $s.tOrganization.Department;
                    $s.tEmployee.ID_Department = $s.tOrganization.ID_Department;
                    $s.tEmployee.Section = $s.tOrganization.Section;
                    $s.tEmployee.ID_Section = $s.tOrganization.ID_Section;
                    $s.tEmployee.JobClassGroup = $s.tOrganization.JobClassGroup;
                    $s.tEmployee.ID_JobClassGroup = $s.tOrganization.ID_JobClassGroup;
                    $s.tEmployee.JobClass = $s.tOrganization.JobClass;
                    $s.tEmployee.ID_JobClass = $s.tOrganization.ID_JobClass;
                    $s.tEmployee.Designation = $s.tOrganization.Name;
                    $s.tEmployee.ID_Designation = $s.tOrganization.ID;
                }
            });
        }
        
        $s.close = function () {
            $mi.dismiss();
        }

        $s.FilingModulesSelected = [];
        $s.CurrentFilingModule = null;

        $s.LoadEmployeeFilingModules = function () {
            if ($s.FilingModulesSelected.length > 0) return;
            $s.SetSystemStatus('Loading employee modules', 'loading');
            $s.Request('ExecuteAction', { ID: $s.RecordID, MenuCode: $s.MenuCode, MethodName: 'loademployeefilingmodules' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.FilingModulesSelected = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.openFilingModules = function () {
            $s.Dialog({
                template: 'FilingModules',
                controller: 'dlgFilingModules',
                size: 'md',
                windowClass: 'filingModules-dlg',
                data: { ID: $s.RecordID, Data: $s.FilingModulesSelected }
            }).result.then(function (ret) {
                if (ret != undefined) {
                    $s.FilingModulesSelected = ret;
                    $s.CurrentFilingModule = null;
                    $s.ApproverSelected = [];
                }
            });
        }


        $s.FormatRateCurrency = function (e) {
            $s.tEmployee[e] = $s.FormatCurrency($s.tEmployee[e]);

        }

        $s.removeModule = function (mod) {
            if ($s.CurrentFilingModule == mod.ID_FilingModules) $s.CurrentFilingModule = null; $s.ApproverSelected = [];
            var idx = Enumerable.From($s.FilingModulesSelected).Select(x => x.ID_FilingModules).IndexOf(mod.ID_FilingModules);
            if ($s.FilingModulesSelected[idx].ID_FilingModules == $s.CurrentFilingModule.ID_FilingModules) {
                $s.CurrentFilingModule = null;
            }
            if ($s.Schema.DeletedModules == undefined) $s.Schema.DeletedModules = [];
            if (mod.ID > 0) $s.Schema.DeletedModules.push(mod.ID);
            $s.FilingModulesSelected.splice(idx, 1);
        }

        $s.LoadApproverSchema = function () {
            if ($s.CurrentFilingModule == null) {
                $s.SetSystemStatus('Please select a Filing Type.', 'warning');
                return;
            }
            if ($s.CurrentFilingModule.Default == undefined) $s.CurrentFilingModule.Default = [];
            if ($s.CurrentFilingModule.Default.length < 5) {
                $s.Request('ExecuteAction', { MenuCode: $s.MenuCode, MethodName: 'loadapproverschema' }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        var sc = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, 0);
                        sc.ID_Level = $s.CurrentFilingModule.Default.length + 1;
                        sc.ID_Approver_Employee_Module = $s.CurrentFilingModule.ID;
                        $s.CurrentFilingModule.Default.push(sc);
                    }
                });
            } else {
                $s.SetSystemStatus('Max approver level reached.', 'warning');
            }
        }

        $s.loadApprovers = function (mod, idx) {
            $s.CurrentFilingModule = mod;
            $('.am-filing-item').removeClass('selected');
            $('#afi-' + idx).addClass('selected');
            $('#afim-' + idx).addClass('selected');

            if (mod.ID == 0) return;
            if (mod.Default != undefined && mod.Default.length > 0) return;
            $s.SetSystemStatus('Loading approvers for ' + mod.Name, 'loading')
            $s.Request('LoadApproverByModule', { ID: mod.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.CurrentFilingModule.Default = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
            })
        }

        $s.AddApprover = function (targetColumn, model) {
            $s.Dialog({
                template: 'EmployeeList',
                controller: 'dlgEmployeeList',
                size: 'md',
                windowClass: 'select-approver-dlg',
                data: { Data: $s.ApproverSelected }
            }).result.then(function (ret) {
                if (ret != undefined) {
                    model[targetColumn] = ret.ID;
                    model[targetColumn.substr(3)] = ret.Name;
                }
            });
        }

        $s.RemoveApprover = function (targetColumn, model) {
            model[targetColumn] = null;
            model[targetColumn.substr(3)] = null;
        }

        $s.RemoveDefaultApprover = function (idx) {
            var deleted = $s.CurrentFilingModule.Default[idx];
            if ($s.CurrentFilingModule.DeletedApprovers == undefined) $s.CurrentFilingModule.DeletedApprovers = [];
            if (deleted.ID > 0) $s.CurrentFilingModule.DeletedApprovers.push(deleted.ID);
            $s.CurrentFilingModule.Default.splice(idx, 1);
        }

        $s.LoadApproverTemplate = function () {
            $s.Dialog({
                template: 'ApproverTemplate',
                controller: 'dlgResumeBankApproverTemplate',
                size: 'md',
                windowClass: 'select-approver-dlg'
            }).result.then(function (ret) {
                if (ret != undefined) {
                    if ($s.FilingModulesSelected.length > 0) {
                        $s.Confirm('Your current setup for the approvers will be cleared and replace with this template. Are you sure you want to proceed?').then(function (rr) {
                            $s.SetSystemStatus('Loading template', 'loading');
                            $s.Request('LoadSelectedApproverTemplate', { ID: ret.ID, ID_Employee: $s.RecordID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                                if (ret.Type == 2) {
                                    $s.SetSystemStatus(ret.Message, 'error');
                                } else {
                                    var oldModules = Enumerable.From($s.FilingModulesSelected).Select(x => x.ID).ToArray();
                                    if ($s.Schema.DeletedModules == undefined) $s.Schema.DeletedModules = [];
                                    $s.Schema.DeletedModules = $s.Schema.DeletedModules.concat(oldModules);
                                    $s.FilingModule1sSelected = ret.Data;
                                    $s.SetSystemStatus('Ready');
                                    $s.CurrentFilingModule = null;
                                }
                                $s.$apply();
                            });
                        });
                    } else {
                        $s.SetSystemStatus('Loading template', 'loading');
                        $s.Request('LoadSelectedApproverTemplate', { ID: ret.ID, ID_Employee: $s.RecordID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                            } else {
                                $s.FilingModulesSelected = ret.Data;
                                $s.SetSystemStatus('Ready');
                                $s.CurrentFilingModule = null;
                            }
                            $s.$apply();
                        });
                    }
                }
            });
        }

        $s.Organization = [];

        $s.Organization.push({ _Id: 1, Id: 'ID_Company', Name: 'Company' });
        $s.Organization.push({ _Id: 2, Id: 'ID_Branch', Name: 'Branch' });
        $s.Organization.push({ _Id: 3, Id: 'ID_Division', Name: 'Division' });
        $s.Organization.push({ _Id: 4, Id: 'ID_Department', Name: 'Department' });
        $s.Organization.push({ _Id: 5, Id: 'ID_Section', Name: 'Section' });
        $s.Organization.push({ _Id: 6, Id: 'ID_JobClassGroup', Name: 'JobClassGroup' });
        $s.Organization.push({ _Id: 7, Id: 'ID_JobClass', Name: 'JobClass' });
        $s.Organization.push({ _Id: 8, Id: 'ID_Designation', Name: 'Designation' });


        $s.EnableOrgField = function (rows, x) {
            var _index = x + 1;
            var Name_Id = "";
            var Name = "";
            while (_index <= $s.Organization.length) {
                var Name_Id = $s.Organization.filter(org => { return org._Id == _index; })[0].Id;
                var Name = $s.Organization.filter(org => { return org._Id == _index; })[0].Name;
                $s.tEmployee[Name_Id] = 0;
                $s.tEmployee[Name] = "";
                _index += 1;
            }

            switch (x) {
                case 1:
                    $s.setBranchLookUp();
                    break;
                case 2: $s.setDivisionLookup();
                    break;
                case 3: $s.setDepartmentLookUp();
                    break;
                case 4: $s.setSectionLookup();
                    break;
                case 5: $s.setJobClassGroupLookup();
                    break;
                case 6: $s.setJobClassLookup();
                    break;
                case 7: $s.setDesignationLookup();
                    break;
            }           
        }


        $s.init();
    }])
    .controller('dlgFilingModules', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });
        $s.FilingModules = [];

        $s.MenuCode = 'AdministrativeApproverMatrix';

        $s.Request('LoadFilingModules', { MenuCode: $s.MenuCode }, 'ApproverMatrix').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message, 'error');
                $s.$apply();
            } else {
                $s.FilingModules = Enumerable.From(ret.Data).Select(x => {
                    return $s.IsExist(x)
                }).ToArray();
            }
        });

        $s.countSelected = function () {
            var a = Enumerable.From($s.FilingModules).Count(x => x.IsChecked);
            return a;
        }

        $s.IsExist = function (mod) {
            var existingData = Enumerable.From($dlgData.Data).Where(x => x.ID_FilingModules == mod.ID).FirstOrDefault();
            var nData = {};
            if (existingData == undefined) {
                nData = { IsChecked: false, ID: 0, ID_FilingModules: mod.ID, ID_Approver: $dlgData.ID, IsActive: true, Name: mod.Name, Default: [] };
            } else {
                nData = { IsChecked: true, ID: existingData.ID || 0, ID_FilingModules: existingData.ID_FilingModules || mod.ID, ID_Approver: existingData.ID_Approver || $dlgData.ID, IsActive: true, Name: mod.Name, Default: existingData.Default || [] };
            }
            return nData;
        }

        $s.load = function () {
            var selected = Enumerable.From($s.FilingModules).Where(x => x.IsChecked == true).ToArray();
            $mi.close(selected);
        }

        $s.close = function () {
            $mi.close();
        }
    }])
    .controller('dlgEmployeeList', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.tblOptions = {
            Columns: [
                { Name: 'Name' }
            ]
        };
        $s.LoadTable($s.tblOptions, 'LoadEmployeeList', 'ApproverMatrix', { MenuCode: 'AdministrativeApproverMatrix' }).then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message);
            } else {
                $s.SetSystemStatus('Ready');
            }
            $s.$apply();
        });

        $s.openForm = function (emp) {
            $mi.close(emp)
        }

        $s.close = function () {
            $mi.close();
        }
    }])
    .controller('dlgResumeBankApproverTemplate', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.tblOptions = {
            Columns: [
                { Name: 'Name' }
            ]
        };

        $s.myController = 'ResumeBankAndSearch';
        $s.MenuCode = 'RecruitmentResumeBank&Search';

        $s.LoadTable($s.tblOptions, 'ExecuteAction', $s.myController, { MenuCode: $s.MenuCode, MethodName: 'loadtemplateapprover' }).then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message);
            } else {
                $s.SetSystemStatus('Ready');
            }
            $s.$apply();
        });

        $s.openForm = function (emp) {
            $mi.close(emp)
        }

        $s.close = function () {
            $mi.close();
        }
    }]);