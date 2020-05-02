angular.module('app')
    .controller('RecruitmentManpowerRequisitionForm', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = [];
        $s.DepartmentList = [];
        $s.RecruitmentList = [];
        $s.EducationalAttainmentList = [];
        $s.BranchList = [];
        $s.EmployeeStatusList = [];
        $s.NatureOfRequestList = [];
        $s.PositionList = [];
        $s.JobLevelList = [];
        $s.GetPositionDetails = [];
        $s.ReplacementReasonsList = [];
        $s.ReplacementPersonList = [];
        $s.ApprovalHistory = [];
        $s.CompanyList = [];
        $s.TableSchema = [];

        $s.myController = 'MRF';
        $s.MenuCode = 'RecruitmentManpowerRequisitionForm';

        $s.IsAllCompany = ($s.Session('ID_Company') == 0);
    
        $s.GetLookUpData = function () {
            $s.Request('LoadLookup', { Name: 'RecruitmentStatus', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.RecruitmentList = ret.Data.Rows;
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

            $s.Request('LoadLookUp', { Name: 'NatureOfRequests2', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.NatureOfRequestList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
            if ($s.IsAllCompany) {
                $s.Request('LoadLookUp', { Name: 'Company', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.CompanyList = ret.Data.Rows;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }

            //$s.GetCompanyBasedLookUp();
        }
        
        $s.getManPowerPlanDetails = function (_ID_Designation, _FiledDate) {

            $s.GetPositionDetails = {
                Position: "", Plantilla: 0, HeadCount: 0, ID_Designation : 0, Vacancy : 0
            };

            _ID_Designation = _ID_Designation == null ? 0 : _ID_Designation;

            var paramFiledDate = new Date(_FiledDate);
            var paramYear = paramFiledDate.getFullYear();

            if (_FiledDate != null && paramYear < 3000) {
                $s.Request('ExecuteAction', { Data: { ID_Designation: _ID_Designation, FiledDate: _FiledDate, MenuCode: $s.MenuCode, MethodName: 'GetManPowerPlanDetail' } }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        if (ret.Data != null) {
                            $s.GetPositionDetails = ret.Data;
                        }
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }
            
        $s.tblOptions = {
            Columns: [
                //{ Name: 'MRFNumber', Label: 'MRF#' },
                { Name: 'Position', Label: 'Position' },
                { Name: 'RequestHeadCount', Label: 'Requested Headcount' },
                { Name: 'RequestedBy', Label: 'Requested By' },
                { Name: 'ApprovedBy', Label: 'Approved By' },
                { Name: 'RecruitmentStatus', Label: 'Status', ControlType: 'status', ReadOnly: false},
                { Name: 'TimeSpanSinceApproved', Label: 'Time Span (Days)' }
                
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'MRFNumber', Type: 1, ControlType: 'text', Label: 'MRF #' },
                //{ Name: 'RequestedDate', Value: [null, null], Type: 12, ControlType: 'date', Label: 'Requested Date' },
                {
                    Name: 'ID_Designation', Label: "Position", Type: 1, ControlType: 'lookup',
                    Options: {
                        controller: $s.myController,
                        method: 'LoadLookUp',
                        parameter: { Name: 'Designation', MenuCode: $s.MenuCode },
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
                },
                { Name: 'ID_RecruitmentStatus', Type: 1, ControlType: 'select', Label: 'Status', Options: { controller: $s.myController, method: 'LoadLookUp', parameter: { Name: 'RecruitmentStatus', MenuCode: $s.MenuCode } } }
                //{ Name: 'EndDate', Value: [null, null], Type: 12, ControlType: 'date', Label: 'Closing Date' },
                //{ Name: 'Posted', Type: 1, ControlType: 'radio', Label: 'Posted' },
                //{ Name: 'ID_NatureOfRequests', Type: 1, ControlType: 'select', Label: 'Nature Of Requests', Options: { controller: 'GetLookUp', method: 'LoadLookUp', parameter: { Name: 'NatureOfRequests' } } },
                //{ Name: 'test', Value: [null, null], Type: 12, ControlType: 'time', Label: 'test time' }
            ]
        };

        // BEGIN --- Load record
        $s.TabsCount = [];
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.GetLookUpData();
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.getManPowerPlanDetails($s.Schema.ID_Designation, $s.Schema.FiledDate);
                        $s.LoadReplacementResources($s.Schema.ID_NatureOfRequests);
                        $s.Schema.ApprovedBy = ret.Data.EmployeeNames.ApprovedBy;
                        $s.Schema.PersonToBeReplaced = ret.Data.EmployeeNames.PersonToBeReplaced;
                        $s.Schema.RequestedBy = ret.Data.EmployeeNames.RequestedBy;
                        $s.SetLookUp();
                        ////REPORTINGS
                        $s.SetReportParams();
                        //Format Salary
                        $s.FormatMinSalary();
                        $s.FormatMaxSalary();
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
                        $s.InitTabs();
                        $s.TabsCount.push($s.tblOptions.Data.Total);
                        $s.TabsCount.push(ret.Data.ForApproval);
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

        $s.ValidateBeforeSave = function () {
            var ReturnMsg = '';

            if ($s.Schema.MinSalary > $s.Schema.MaxSalary) {
                ReturnMsg = 'Minimum Salary is greater than Maximum Salary';
            }
            return ReturnMsg;
        }


        //Save New - Edit - Delete
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.mrf')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                var validatedMessage = $s.ValidateBeforeSave();
                if (validatedMessage != '') {
                    $s.Prompt(validatedMessage);
                    $s.SetSystemStatus(validatedMessage, 'error');
                    return;
                }

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

        $s.postMRF = function () {
            if ($s.Schema.Posted) return;
            $s.Request('Post', { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Successfully Posted.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
                $s.$apply();
            });
        }

        $s.cancelMRF = function () {            
            
            if (ID_RecruitmentStatus = 11 && !$s.Schema.Posted) return;

            $s.Request('Cancel', { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Successfully Cancelled.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
                $s.$apply();
            });
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }

        $s.GetReplacementLookUp = function () {
            if ($s.ReplacementReasonsList != null) {
                $s.Request('LoadLookUp', { Name: 'ReplacementReasons', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.ReplacementReasonsList = ret.Data.Rows;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                    });
            }
        }

        $s.FilterSelectedCompany = {};
        $s.SetSelectedCompany = function () {
            $s.FilterSelectedCompany = {
                Filters: [
                    { Name: 'ID_Company', Value: $s.Schema.ID_Company, Type: 1 }
                ]
            }
            return $s.FilterSelectedCompany;
        }

        $s.SelectCompany = function () {

            $s.Schema.ID_Designation = null;
            $s.Schema.Designation = '';
            $s.Schema.ID_JobClass = null;
            $s.Schema.JobClass = '';
            $s.Schema.ID_RequestedBy = null;
            $s.Schema.RequestedBy = '';
            $s.SetLookUp();
        }

        $s.employeeLookup = {};
        $s.jobClassLookup = {};
        $s.desginationLookup = {};

        $s.SetLookUp = function () {
            $s.employeeLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Employee' }
                    ],
                    Filters: [
                        { Name: 'Name', Type: 9, Label: 'Employee' },
                        $s.SetSelectedCompany().Filters[0]
                    ] 
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'Employee', MenuCode: $s.MenuCode }
            };

            $s.jobClassLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Job Class' }
                    ],
                    Filters: $s.SetSelectedCompany().Filters
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'JobClass', MenuCode: $s.MenuCode }
            };

            $s.desginationLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Designation' }
                        //, { Name: 'Department', Label: 'Assigned Department' },
                        //{ Name: 'EmployeeName', Label: 'Department Head' }
                    ],
                    Filters: $s.SetSelectedCompany().Filters
                },
                method: 'LoadLookUp',
                controller: $s.myController,
                parameter: { Name: 'MRFDesignation', MenuCode: $s.MenuCode }
            };
        }
        

        $s.LoadReplacementResources = function (e) {
            if (e == 2) {
                $s.GetReplacementLookUp();
            }
        }

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        $s.IsAttachmentRequired = function (e) {

            var filteredRes = $s.NatureOfRequestList.filter(x => (x.ID == e));
            if (filteredRes.length > 0)
                return filteredRes[0].IsRequiredMRFAttachment;
            else return false;
        }

        $s.ComputeTotalHeadCount = function () {
            
            if ($s.Schema.MaleCount == undefined) { $s.Schema.MaleCount = 0; }
            if ($s.Schema.FemaleCount == undefined) { $s.Schema.FemaleCount = 0; }
            if ($s.Schema.NoGenderPreference == undefined) { $s.Schema.NoGenderPreference = 0; }

            $s.Schema.RequestHeadCount = ($s.Schema.MaleCount + $s.Schema.FemaleCount + $s.Schema.NoGenderPreference);
            
        }

        $s.JobPosting = function (status) {
            var PromptMsg = (status == 1 ? ' Post ' : ' Cancel ') + 'Successful';

            if (!$s.Schema.Posted) return;

            $s.Request('ExecuteAction', { ID: $s.Schema.ID, Status: status, MenuCode: $s.MenuCode, MethodName: 'JobPosting' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.Prompt(PromptMsg);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
                $s.$apply();
            });
        }
        ///APPROVAL OF TRAINING
        //tabs
        $s.Tabs = [];
        $s.AlreadyLoadedTab = [];
        $s.CurrentTab = 0;
        $s.InitTabs = function () {
            //applicationTab
            $s.Tabs.push(angular.copy($s.tblOptions));
            //approvalTab
            var approvalTab = {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'MRFNumber', Label: 'MRF#' },
                    { Name: 'Designation', Label: 'Designation' },
                    { Name: 'NatureOfRequest', Label: 'Nature Of Request' },
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
            $s.LoadTable($s.Tabs[idx], (idx == 1 ? 'LoadApproval' : 'LoadList'), $s.myController, { MenuCode: $s.MenuCode } ).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Ready');
                    $s.Tabs[idx] = angular.copy($s.tblOptions);
                }
                $s.$apply();
            });
        }

        $s.FormatMinSalary = function () {
            $s.Schema.MinSalary = $s.FormatCurrency($s.Schema.MinSalary);
        }

        $s.FormatMaxSalary = function () {
            $s.Schema.MaxSalary = $s.FormatCurrency($s.Schema.MaxSalary);
        }
       
        $s.SetRecruitmentStatus = function (e) {
        //Status = 1 - Approve, 2 - Disapprove, 3 - On Hold
            
            if ((e == 2 || e == 3) && ($s.Schema.ApproverComment.length == 0)) {
                $s.Prompt('Approver Comment is required');
                return;
            }

            if (e > 3) {
                $s.Prompt('Wrong recruitment status value');
                return;
            }

            $s.Request('SetStatus', {
                    data: $s.Schema,
                    RecruitmentStatusType: e,
                    ApproverComment: $s.Schema.ApproverComment,
                    ForApprovalNotifMsg: $s.ForApprovalNotifMsg(),
                    SendStatusToEmployee: $s.SendStatusToEmployeeNotifMsg(e),
                    MenuCode: $s.MenuCode
            }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $st.go($st.$current.parent.name, { ID: '' }, { reload: true });
                }
                $s.$apply();
                });
        }



        //Approve / Disapprove - Email notifications messages
        $s.ForApprovalNotifMsg = function () {
            return {
                Title: 'For Approval',
                Content: 'MRF "' + $s.Schema.MRFNumber + '" is for approval.',
                ID_Receiver: 0, //Send to next approver
                ID_Sender: $s.Session('ID_User'),
                ID_WebNotificationTypes: null,
                ReferenceID: $s.Schema.ID
            }
        }

        //Update the employee about the status of MRF
        $s.SendStatusToEmployeeNotifMsg = function (e) {
            var status = '';

            switch (e) {
                case (1): status = "has been Approved"; break;
                case (2): status = "has been Disapproved"; break;
                default: status = "is On-hold"; break;
            }

            return {
                Title: 'MRF Status',
                Content: 'MRF "' + $s.Schema.MRFNumber + '-' + $s.Schema.Designation + '" ' + status + ' by ',
                ID_Receiver: $s.Schema.ID_RequestedBy, //Send to next approver
                ID_Sender: $s.Session('ID_User'),
                ID_WebNotificationTypes: null,
                ReferenceID: $s.Schema.ID
            }
        }

        $s.rptOptions = [];
        $s.SetReportParams = function () {
            $s.rptOptions = {
                Pdf:
                {
                    Method: 'LoadPDF',
                    Parameter: { ID: $s.Schema.ID, MenuCode: $s.MenuCode,  },
                    Controller: $s.myController
                },
                Xls:
                {
                    Method: 'LoadExcel',
                    Parameter: { ID: $s.Schema.ID, MenuCode: $s.MenuCode },
                    Controller: $s.myController
                }
            }
        }

        $s.GetApprovalHistory = function (ReferenceID, TargetEmployee) {
            if ($s.ApprovalHistory.length == 0)
                $s.Request('LoadHistory', { ID: ReferenceID, Employee: TargetEmployee, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.ApprovalHistory = ret.Data;
                    }
                    $s.$apply();
                });
        }

    }]);