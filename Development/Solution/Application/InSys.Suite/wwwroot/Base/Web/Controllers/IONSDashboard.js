angular.module('app')
    .controller('IONSDashboard', ['$scope', '$controller', function ($s, $c) {
        $c('insysTableController', { $scope: $s });

        $s.myController = 'DashBoard';
        $s.MenuCode = 'IONSDashboard';
        $s.LeaveItem = [];
        $s.Announcements = [];
        $s.UpcomingEvent = [];
        $s.TimeKeepingSummary = [];
        $s.FilingTypes = [];
        $s.IsLimit = true;


        $s.ShowToggle = function () {
            $s.IsLimit = !$s.IsLimit;
        }

        $s.GetDescriptiveHourName = function (e) {
            //This function is applicable only for minutes.
            var Minute = e;
            var Hours = Minute >= 60 ? Math.trunc(Minute / 60) : 0;
            var RemainingMinutes = Minute % 60;

            var HourName = Hours > 0 ? Hours.toString() + ' hr(s)' : '';
            var MinuteName = RemainingMinutes > 0 ? RemainingMinutes.toString() + ' min(s)' : '';

            if (HourName != '' && MinuteName != '')
                return HourName + ' and ' + MinuteName;
            else
                return HourName + MinuteName;
        }

        $s.LoadInitialDashBoard = function () {
            return $s.Request('LoadForm', { MenuCode: $s.MenuCode }, $s.myController);
        }

        $s.init = function () {
            $s.LoadInitialDashBoard().then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message);
                } else {
                    $s.Announcements = ret.Data.AnnouncementData;
                    $s.TimeKeepingSummary = ret.Data.TimeSummaryData;
                    $s.LeaveItem = ret.Data.LeaveDetailsData;
                    $s.UpcomingEvent = ret.Data.UpcommingEventData;

                    //format timekeeping summary
                    $s.TimeKeepingSummary[0].Absent = $s.TimeKeepingSummary[0].Absent + ' day(s)'
                    $s.TimeKeepingSummary[0].Tardy = $s.GetDescriptiveHourName($s.TimeKeepingSummary[0].Tardy);
                    $s.TimeKeepingSummary[0].Undertime = $s.GetDescriptiveHourName($s.TimeKeepingSummary[0].Undertime);
                    $s.TimeKeepingSummary[0].Overtime = $s.GetDescriptiveHourName($s.TimeKeepingSummary[0].Overtime);

                    $s.$apply();
                }
            });
            $s.InitializeOptions();
        }

        $s.deleteRow = function (selectedRow) {
            // = data.Data.Rows.filter(x => { return x.IsChecked == true; });
            var Ids = Enumerable.From(selectedRow.Data.Rows).Where(function (x) { return x.IsChecked == true }).Select(function (x) { return (x.ID == undefined ? x.Id : x.ID); }).ToArray();
            //Ready To Delete
        }

        $s.newFormAction = function (controller, template, param) {
            $s.FileApplication(controller, template, param);
        }

        $s.openFormAction = function (controller, template, row) {// function (controller, template, param) {
            var Id = row.ID == undefined ? row.Id : row.ID;
            $s.FileApplication(controller, template, Id);
        }

        $s.FileApplication = function (controller, template, param) {
            //Just Change template and controller depending on the current filing type the user are working with.
            //Create your own controller per filing type
            //Leave //Overtime //OfficialBusiness //MissedLog //COS
            $s.Dialog({
                template: template,//'COS',
                controller: controller,//'dlgLeave',
                size: 'lg',
                windowClass: 'fileApplication-dlg',
                data: param
            })
        }

        $s.ViewPayslip = function () {
            //Load InSys Report here
        }

        //FILING APPLICATIONS {
        $s.FilingOptions = [];
        $s.InitializeOptions = function () {
            
            $s.FilingOptions['leave'] = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'RefNum', Label: 'Reference No' },
                        { Name: 'FiledDate', Label: 'Date Filed', Format: 'LL' },
                        { Name: 'LeavePayrollItem', Label: 'Leave Item' },
                        { Name: 'StartDate', Label: 'From', Format: 'LL' },
                        { Name: 'EndDate', Label: 'To', Format: 'LL' },
                        { Name: 'Status', Label: 'Status' },
                        { Name: 'IsPosted', Label: 'Posted' }
                    ],
                    Filters: [
                        { Name: 'RefNum', Type: 1, Value: null },
                        { Name: 'StartDate', Type: 1, Value: null },
                        { Name: 'EndDate', Type: 1, Value: null },
                        { Name: 'ID_FilingStatus', Type: 1, Value: null },
                        { Name: 'IsPosted', Type: 1, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteRecord', 'Leave', tblOptions, { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsLeave' });
                    },
                    isEditable: false,
                    hasOpenIcon: true,
                    hasOpenIconClick: function (row) {
                        $s.LoadForm(row.Id, $s.FilingOptions['leave']).then(function (res) {
                            $s.openForm('leave', res.Data);
                        });
                    },
                    openForm: function (row) {
                        $s.LoadForm(row.Id, $s.FilingOptions['leave']).then(function (res) {
                            $s.openForm('leave', res.Data);
                        });
                    }
                },
                Method: 'LoadList',
                Controller: 'Leave',
                Parameter: { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsLeave' }
            };

            $s.FilingOptions['ot'] = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ReferenceNo', Label: 'Reference No.' },
                        { Name: 'WorkDate', Label: 'Date', Format: 'LL' },
                        { Name: 'StartTime', Label: 'From', Format: 'LT' },
                        { Name: 'EndTime', Label: 'To', Format: 'LT' },
                        { Name: 'ComputedHours', Label: 'Total Hours' },
                        { Name: 'ID_FilingStatus', Label: 'Status' },
                        { Name: 'IsPosted', Label: 'Posted' }
                    ],
                    Filters: [
                        { Name: 'ReferenceNo', Type: 1, Value: null },
                        { Name: 'WorkDate', Type: 1, Value: null },
                        { Name: 'ID_FilingStatus', Type: 1, Value: null },
                        { Name: 'IsPosted', Type: 1, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteRecord', 'Overtime', tblOptions, { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsOvertime' });
                    },
                    isEditable: false,
                    hasOpenIcon: true,
                    hasOpenIconClick: function (row) { $s.LoadForm(row.ID, $s.FilingOptions['ot']).then(function (res) { $s.openForm('ot', res.Data); }); },
                    openForm: function (row) { $s.LoadForm(row.ID, $s.FilingOptions['ot']).then(function (res) { $s.openForm('ot', res.Data); }); }
                },
                Method: 'LoadList',
                Controller: 'Overtime',
                Parameter: { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsOvertime' }
            };

            $s.FilingOptions['ob'] = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'ReferenceNo', Label: 'Reference No.' },
                        { Name: 'FiledDate', Label: 'Date Filed', Format: 'LL' },
                        { Name: 'StartTime', Label: 'Time In', Format: 'LT' },
                        { Name: 'EndTime', Label: 'Time Out', Format: 'LT' },
                        { Name: 'FilingStatus', Label: 'Status' },
                        { Name: 'IsPosted', Label: 'Posted' }
                    ],
                    Filters: [
                        { Name: 'ReferenceNo', Type: 1, Value: null },
                        { Name: 'WorkDate', Type: 1, Value: null },
                        { Name: 'ID_FilingStatus', Type: 1, Value: null },
                        { Name: 'IsPosted', Type: 1, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteRecord', 'OB', tblOptions, { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsOB' });
                    },
                    isEditable: false,
                    hasOpenIcon: true,
                    hasOpenIconClick: function (row) { $s.LoadForm(row.ID, $s.FilingOptions['ob']).then(function (res) { $s.openForm('ob', res.Data); }); },
                    openForm: function (row) { $s.LoadForm(row.ID, $s.FilingOptions['ob']).then(function (res) { $s.openForm('ob', res.Data); }); }
                    
                },
                Method: 'LoadList',
                Controller: 'OB',
                Parameter: { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsOB' }
            };

            $s.FilingOptions['ml'] = {
                trigger: function () { },
                tblOptions: {
                    Data: { Rows: [] },
                    Columns: [
                        { Name: 'RefNum', Label: 'Reference No.' },
                        { Name: 'WorkDate', Label: 'Date', Format: 'LL' },
                        { Name: 'FilingStatus', Label: 'Status' },
                        { Name: 'IsPosted', Label: 'Posted' }
                    ],
                    Filters: [
                        { Name: 'RefNum', Type: 1, Value: null },
                        { Name: 'WorkDate', Type: 1, Value: null },
                        { Name: 'ID_FilingStatus', Type: 1, Value: null },
                        { Name: 'IsPosted', Type: 1, Value: null }
                    ],
                    deleteRow: function (tblOptions) {
                        $s.deleteDetailRow('DeleteRecord', 'MissedLog', tblOptions, { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsMissedLog' });
                    },
                    isEditable: false,
                    hasOpenIcon: true,
                    hasOpenIconClick: function (row) {
                        $s.LoadForm(row.ID, $s.FilingOptions['ml']).then(function (res) {
                           
                            $s.openForm('ml', res.Data);
                        });
                    },
                    openForm: function (row) {
                        $s.LoadForm(row.ID, $s.FilingOptions['ml']).then(function (res) {
                         
                            $s.openForm('ml', res.Data);
                        });
                    }
                },
                Method: 'LoadList',
                Controller: 'MissedLog',
                Parameter: { MenuCode: 'IONSDashBoardFilingApplications_ApplicationsMissedLog' }
            };
            
            $s.FilingOptions['cos'] = {
            trigger: function () { },
            tblOptions: {
                Data: { Rows: [] },
                Columns: [
                    { Name: 'ReferenceNo', Label: 'Reference No' },
                    { Name: 'FiledDate', Label: 'Date Filed', Format: 'LL' },
                    { Name: 'StartDate', Label: 'From', Format: 'LT' },
                    { Name: 'EndDate', Label: 'To', Format: 'LT' },
                    { Name: 'FilingStatus', Label: 'Status' },
                    { Name: 'IsPosted', Label: 'Posted' }
                ],
                Filters: [
                    { Name: 'ReferenceNo', Type: 1, Value: null },
                    { Name: 'StartDate', Type: 1, Value: null },
                    { Name: 'EndDate', Type: 1, Value: null },
                    { Name: 'ID_FilingStatus', Type: 1, Value: null },
                    { Name: 'IsPosted', Type: 1, Value: null }
                ],
                deleteRow: function (tblOptions) {

                    $s.deleteDetailRow('DeleteRecord', 'ChangeOfSchedule', tblOptions, { MenuCode: 'IONSDashBoardFilingApplications_ApprovalsChangeOfSchedule' });
                },
                isEditable: false,
                hasOpenIcon: true,
                hasOpenIconClick: function (row) { $s.LoadForm(row.ID, $s.FilingOptions['cos']).then(function (res) { $s.openForm('cos', res.Data); }); },
                openForm: function (row) { $s.LoadForm(row.ID, $s.FilingOptions['cos']).then(function (res) { $s.openForm('cos', res.Data); }); }
            },
            Method: 'LoadList',
            Controller: 'ChangeOfSchedule',
            Parameter: { MenuCode: 'IONSDashBoardFilingApplications_ApprovalsChangeOfSchedule' }
        };
        }
        $s.loadTabSchema = function (ft, table) {
            if ($s.FilingOptions[ft].tblOptions.Data != undefined)
                if ($s.FilingOptions[ft].tblOptions.Data.Rows != undefined)
                    if ($s.FilingOptions[ft].tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: table }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.FilingOptions[ft].tblOptions.TableSchema = ret2.Data;
                    $s.FilingOptions[ft].tblOptions.HasNew = true;
                    $s.FilingOptions[ft].tblOptions.HasEdit = true;
                    $s.FilingOptions[ft].tblOptions.HasDelete = true;
                    $s.FilingOptions[ft].trigger().then(function (tblOptions) {
                        $s.FilingOptions[ft].tblOptions = tblOptions;
                        $s.FilingOptions[ft].tblOptions.newForm = function () {
                            var data = $s.PlotDefault({}, ret2.Data, 0);
                            
                            $s.LoadForm(data.ID, $s.FilingOptions[ft]).then(function (res) { $s.openForm(ft, res.Data); });
                        }
                    });
                }
            });
        }
        $s.LoadForm = function (id, option) {
           
            return $s.Request('LoadForm', { ID: id, MenuCode: option.Parameter.MenuCode }, option.Controller);
        }
        $s.openForm = function (ft, data) {

            
            var template;
            var controller;
            switch (ft) {
                case 'leave':
                    template = 'IONS_Leave';
                    controller = 'leaveDlgCtrl';
                    break;
                case 'ot':
                    template = 'IONS_Overtime';
                    controller = 'overtimeDlgCtrl';
                    break;
                case 'ob':
                    template = 'IONS_OfficialBusiness';
                    controller = 'officialBusinessDlgCtrl';
                    break;
                case 'ml':
                    template = 'IONS_MissedLog';
                    controller = 'missedLogDlgCtrl';
                    break;
                case 'cos':
                    template = 'IONS_COS';
                    controller = 'changeOfScheduleDlgCtrl';
                    break;
                default:
                    console.log('Invalid filing type.');
            }

            
            var dd = $s.Dialog({
                template: template,
                controller: controller,
                size: 'lg',
                windowClass: 'fileApplication-dlg',
                data: { data, Controller: controller }
            });
            dd.result.then(function () {
                $s.Request($s.FilingOptions[ft].Method, { MenuCode: $s.FilingOptions[ft].Parameter.MenuCode }, $s.FilingOptions[ft].Controller).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message);
                    } else {

                        $s.FilingOptions[ft].tblOptions.Data.Rows = ret.Data.Rows;
                        $s.FilingOptions[ft].tblOptions.Data.Rows.reverse();
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();

                });
            });
        }
        //} FILING APPLICATIONS
        $s.init();
    }])
    .controller('leaveDlgCtrl', ['$scope', '$controller', '$uibModalInstance', 'dData', '$state', function ($s, $c, $mi, $dlgData, $st) {
        $c('BaseController', { $scope: $s });
        $c('insysTableController', { $scope: $s });
        $s.Data = {};
        $s.Details = [];
        $s.ApprovalHistories = [];
        $s.MenuCode = 'IONSDashBoardFilingApplications_ApplicationsLeave';
        $s.ApiController = 'Leave';
        $s.PayrollItems = [];
        $s.FilingStatus = [];
        $s.TableSchema = [];
        $s.LeaveDetails = [];
        $s.RecordID = 0
        
       
        $s.UploadOptions = {
            IsMultiple: false,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };



        
        $s.Init = function () {

            console.log('data', $dlgData);;
            $s.Data = $dlgData.data.Form;//$dlgData.Options.Data.Rows;

            if (!$s.Validate.Details($dlgData.data)) return

            $s.LeaveDayType = $dlgData.data.LeaveDayType.Data.Rows;
            $s.PayrollItems = $dlgData.data.PayrollItems.Data.Rows;
            $s.LeaveCredit = $dlgData.data.LeaveCredit.Data.Rows;
            $s.RestDays = Enumerable.From($dlgData.data.EmployeeRestDay.Data.Rows).Select(x => x.ID_WeekDay).ToArray();
            
            $s.TableSchema = $dlgData.data.Schema;
            $s.TableSchemaDetail = $dlgData.data.SchemaDetail;
            $s.RecordID = $s.Data.Id;
            $s.LoadApprovalHistory();
            $s.LoadLeaveDetails();
            $s.Data.ID_LeavePayrollItem = $s.Data.ID_LeavePayrollItem ? $s.Data.ID_LeavePayrollItem : Enumerable.From($s.PayrollItems).Select(x => x.ID).FirstOrDefault();
            
            $s.RestDays = Enumerable.From($dlgData.data.EmployeeRestDay.Data.Rows).Select(x => x.ID_WeekDay).ToArray();
            $s.MaternityDays = 105;
            $s.ID_MaternityLeave = 62;
            $s.LeaveCreditBalance = 0;

       
            if ($s.Data.IsPosted)
                $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'leaveForm', true);
            
        }

        $s.GetID_PayrollItem = function (_code) {

            return Enumerable.From($s.PayrollItems).Where(x => x.Code == _code).Select(x => x.ID).FirstOrDefault();
        }

        $s.Save = function (post) {

            if (!$s.Validate.DateRange()) return;
            if (!$s.IsTabsValid('leaveForm', $s.TableSchema.concat($s.TableSchemaDetail),0,true)) return;

            if (post == 1) {

                $s.Confirm('Are you sure you want to Post?').then(function () {

                    $s.Data.IsPosted = true;
                    $s.Request('SaveForm', { Data: $s.Data, Detail: $s.LeaveDetails, Post: $s.varPost, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                        if (res.Type == 1) {

                            $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'leaveForm', true);
                            $s.Prompt('Successfuly Posted.', 'Leave Filing').then(function () {
                                $s.Data.RefNum = res.Data.RefNo;
                                $s.Data.Id = res.Data.ID;
                                $s.Data.IsPosted = 1;

                            });// $mi.close();
                            
                        } else {
                            if (res.Type == 7) {
                                $s.Prompt(res.Message, 'Leave Filing', 'warning');
                            } else {
                                $s.Prompt('An error occured. Please contact your administrator','Error');
                            }

                            return;
                        }
                    });

                });
            } else {
                $s.Data.IsPosted = 0;
                $s.Request('SaveForm', { Data: $s.Data, Detail: $s.LeaveDetails, Post: $s.varPost, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                  
                    if (res.Type == 1) {
                        
                        $s.Prompt(res.Message, 'Leave Filing').then(function () {
                            $s.Data.RefNum = res.Data.RefNo;
                            $s.Data.Id = res.Data.ID;
                        });

                    } else {
                        if (res.Type == 7) {
                            $s.Prompt(res.Message, 'Leave Filing', 'warning');
                        } else {
                            $s.Prompt('An error occured. Please contact your administrator', 'Error');
                        }

                        return;
                    }

                    $s.$apply();
                });
            }

           
        }

        $s.Close = function () {
            $mi.close();
        }

        $s.LoadApprovalHistory = function () {
            $s.Request('LoadApprovalHistory', { Data: $s.Data, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.Prompt('An error occured. Please contact your administrator', 'Error');
                    $s.$apply();
                } else {
                    $s.ApprovalHistories = res.Data;
                }
            });
        }

        $s.LeaveDetailOption = {

            trigger: function () { },
            tblOptions: {
                Data: { Rows: [], Total: 0 },
                Columns: [
                    //{ Name: 'ID', Label: '#' },
                    { Name: 'WorkDate', Label: 'Date', ControlType: 'date', IsReadOnly: true, Format: 'ddd, MMM DD YYYY' },
                    {
                        Name: 'ID_LeaveDayType', Label: 'Duration', ControlType: 'select', Options: //($s.Data.IsPosted ? false : true)
                        {
                            controller: 'Leave',
                            method: 'LoadLookup',
                            parameter: { MenuCode: $s.MenuCode, Name: 'leaveDayType' }
                        },
                        cascade: function (a, b, c) { $s.Validate.IfWithPay() }
                    },
                    { Name: 'IsPaid', Label: 'With Pay', IsReadOnly: true, ControlType: 'checkbox' },
                    { Name: 'Reasons', Label: 'Comment', ControlType: 'text'},
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteDetail', 'TestModule', tblOptions, { DetailName: 'dependents', MenuCode: $s.MenuCode });
                },
                isEditable: true
            },
            Method: 'LoadDetail',
            Controller: 'Leave',
            Parameter: { DetailName: 'LeaveDetail', ID_Leave: $s.RecordID, Menucode: $s.MenuCode },
            isEditable: true,
            hasOpenIcon: false,
            hasOpenIconClick: function (row) { },
            openForm: function () { }


        };

        $s.LoadLeaveDetails = function () {


            if ($s.RecordID == 0) {

                $s.GererateLeaveDetail();
            } else {

                $s.Request('LoadDetail', { DetailName: 'LeaveDetail', ID_Leave: $s.RecordID, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {


                    $s.LeaveDetails = res.Data.Rows;

                    $s.$apply();

                });
                

                if ($s.Data.ID_LeavePayrollItem == $s.GetID_PayrollItem('ML')) {
                    $('input[name="EndDate"]').prop("disabled", true);
                }
            }

            //if (!$s.Validate.TableOptions($s.LeaveDetailOption.tblOptions)) return;

            //$s.Request('LoadSchema', { Table: 'tLeave_Detail' }, 'Menu').then(function (ret2) {

            //    $s.LeaveDetailOption.Parameter.ID_Leave = $s.RecordID;

            //    if (ret2.Type == 2) {
            //        $s.SetSystemStatus(ret2.Message, 'error');
            //    } else {
            //        $s.LeaveDetailOption.tblOptions.TableSchema = ret2.Data;
            //        $s.LeaveDetailOption.trigger().then(function (tblOptions) {

            //            $s.LeaveDetailOption.tblOptions = tblOptions;

            //            if ($s.RecordID == 0 && $s.Data.StartDate && $s.Data.EndDate) {
            //                $s.GererateLeaveDetail();
            //            } else {
            //                if ($s.Data.ID_LeavePayrollItem == $s.ID_MaternityLeave) {
            //                    $('input[name="EndDate"]').prop("disabled", true);
            //                }
            //            }

            //            $s.$apply();
            //        });
            //    }

                
            //    $s.$apply();

            //});
        }

        $s.ChangeLeaveType = function () {

            if ($s.Data.ID_LeavePayrollItem == $s.GetID_PayrollItem('ML')) {
                $s.Data.EndDate = moment(new Date($s.Data.StartDate)).add($s.MaternityDays, 'days');
                $s.Data.TotalDays = $s.MaternityDays;
                $s.Data.TotalDaysWithPay = $s.MaternityDays;
                $s.LeaveDetailOption.tblOptions.Data.Rows = [];
                $('input[name="EndDate"]').prop("disabled", true);

            } else if ($s.Data.ID_LeavePayrollItem == $s.GetID_PayrollItem('BL')) {
                $s.Data.EndDate = $s.Data.StartDate;
                $('input[name="EndDate"]').prop("disabled", true);
                $('select[name="ID_LeaveDayType"]').prop("disabled", true);
            }else {

                if ($s.LeaveDetailOption.tblOptions.Data.Rows.length <= 0) {
                    $('input[name="EndDate"]').prop("disabled", false);
                    $s.Data.EndDate = $s.Data.StartDate;
                }
              
                $s.GererateLeaveDetail();
            }
        }

        $s.ChangeDate = function () {
            
            if ($s.Data.ID_LeavePayrollItem == $s.GetID_PayrollItem('ML')) {
                
                clearTimeout($s.timeOut);
               
                $s.timeOut = setTimeout(function () {
                    $s.Data.EndDate = moment(new Date($s.Data.StartDate)).add($s.MaternityDays, 'days');
                    $s.$apply();
                },500)
              
              
            }  else {

                
                if ($s.Data.ID_LeavePayrollItem == $s.GetID_PayrollItem('BL')) {
                    clearTimeout($s.timeOut);

                    $s.timeOut = setTimeout(function () {
                        $s.Data.EndDate = moment(new Date($s.Data.StartDate));
                        $s.GererateLeaveDetail();
                        $s.$apply();
                    }, 500)
                } else {
                    $s.GererateLeaveDetail();
                }

                
            }
            
        }
        
        $s.GererateLeaveDetail = function () {



            $s.LeaveDetails = [];
            if ($s.Data.ID_LeavePayrollItem == $s.GetID_PayrollItem('ML')) return;
            
            var dateRange = $s.GenerateDateRange(new Date($s.Data.StartDate), new Date($s.Data.EndDate),30);


            if (!dateRange) {
                $s.isValidDateRange = false;
                return;
            } else {
                $s.isValidDateRange = true;
            }
           
            Enumerable.From(dateRange).ForEach(function (e) {
                
                if ($s.Validate.RestDay(e)) return;


                $s.LeaveDetails.push({
                    ID_Leave : $s.Data.Id,
                    ID_LeaveDayType : 1,
                    WorkDate: moment(e).format('L'),
                    ID_CreatedBy : $s.Data.ID_Employee,
                    ID_ModifiedBy : $s.Data.ID_Employee,
                    ModifiedAt : moment(),
                    CreatedAt : moment()
                });

                //var defaultValue = $s.PlotDefault({}, $s.TableSchema, 0);

                //defaultValue.ID_Leave = $s.Data.Id;
                //defaultValue.ID_LeaveDayType = 1;
                //defaultValue.WorkDate = moment(e).format('L');
                //defaultValue.ID_CreatedBy = $s.Data.ID_Employee;
                //defaultValue.ID_ModifiedBy = $s.Data.ID_Employee;
                //defaultValue.ModifiedAt = moment();
                //defaultValue.CreatedAt = moment();

                //$s.LeaveDetailOption.tblOptions.Data.Rows.unshift(defaultValue);


                $s.Validate.IfWithPay();
            });
            
        }

        $s.UnPost = function () {

            $s.Confirm('Are you sure you want to Unpost?').then(function () {

                $s.Request('UnPost', { ID_Leave: $s.RecordID, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {

                    if (res.Type == 1) {
                        $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'leaveForm', false);
                        $s.Data.IsPosted = false;
                    } else {

                        $s.Prompt('An error occured. Please contact your administrator', 'Error');
                    }

                    $s.$apply();
                });


            })
            
        }

        //Validations
        $s.Validate = {
            TableOptions: function (e) {
                if (!e.Data || !e.Data.Rows || e.Data.Rows > 0) return false;
                else return true;
            },
            IfWithPay: function () {

                //var _Credit = Enumerable.From($s.LeaveCredit).Where(x => x.ID_LeavePayrollItem == $s.Data.ID_LeavePayrollItem).FirstOrDefault().EndBalance; 

                var _balance = $s.Data.ID_LeavePayrollItem == 27 ? 2 : 3 //_Credit != undefined || _Credit != null || _Credit != 0 ? _Credit : 0
                $s.Data.TotalDays = 0;
                $s.Data.TotalDaysWithPay = 0;
                Enumerable.From(Enumerable.From($s.LeaveDetails).ToArray()).ForEach(function (e) {

                    if (parseFloat(_balance) >= 0.5 && e.ID_LeaveDayType) {

                        if (e.ID_LeaveDayType == 1 && _balance == 0.5) {
                            e.Comment = 'Only Half day has with pay, not enough leave credit.';
                        }
                        else {
                          
                            e.Comment = null;
                        }
                        
                        if (e.ID_LeaveDayType == 2 || e.ID_LeaveDayType == 3) {

                            _balance -= 0.5;
                            $s.Data.TotalDaysWithPay += 0.5;
                            $s.Data.TotalDays += 0.5;

                        } else {
                            _balance -= 1;
                            $s.Data.TotalDaysWithPay += 1;
                            $s.Data.TotalDays += 1;
                        }

                        e.IsPaid = true;

                    } else {

                        e.Comment = null;
                        if (e.ID_LeaveDayType == 2 || e.ID_LeaveDayType == 3) {
                            $s.Data.TotalDays += 0.5;
                        } else {
                            $s.Data.TotalDays += 1;
                        }

                        e.IsPaid = false;
                    }

                });

            },
            RestDay: function (date) {
                return $s.RestDays.includes(parseInt(moment(date).isoWeekday()));
            },
            Details: function (_details) {
            
                Enumerable.From(_details).ForEach(function (x) {

                    if ([
                        'PayrollItems',
                        'FilingStatus',
                        'LeaveCredit',
                        'EmployeeRestDay',
                        'LeaveDayType',
                     ].includes(x.Key)) {

                        if (x.Value.Type != 1) {
                            $s.Prompt(x.Value.Message, x.Key, 'error');
                            return false;
                        }
                    }

                })
                return true
            },
            DateRange: function () {

                if (!$s.isValidDateRange)
                    $s.Prompt('Date range is too long', 'Start Date - End Date', 'warning');


                return $s.isValidDateRange 
            }
        }

    }])

    .controller('overtimeDlgCtrl', ['$scope', '$controller', '$uibModalInstance', 'dData', '$state', function ($s, $c, $mi, $dlgData, $st) {
        $c('BaseController', { $scope: $s });
        $s.MenuCode = 'IONSDashBoardFilingApplications_ApplicationsOvertime';
        $s.ApiController = 'Overtime';
        $s.Data = {};
        $s.TableSchema = [];
        $s.Details = [];
        $s.ApprovalHistories = [];
        $s.WorkCredits = [];
        $s.Init = function () {

            console.log('overtime', $dlgData.data);
            $s.Data = $dlgData.data.Form;


            $s.TableSchema = $dlgData.data.Schema;
            $s.WorkCredits = $dlgData.data.WorkCredits.Data.Rows;

            $s.LoadApprovalHistories();

            if ($s.Data.IsPosted)
                $s.DisableEnableFields($s.TableSchema, 'overtimeFrm', true);

        }
        $s.Save = function (post) {
            if (!$s.IsTabsValid('overtimeFrm', $s.TableSchema)) return
            
            $s.Request('SaveForm', { Data: $s.Data, Post: post, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                if (res.Type == 2) {

                   
                    $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');

                    $s.$apply();
                    return;
                }
                else {
                    if (post) {

                        $s.DisableEnableFields($s.TableSchema, 'overtimeFrm', true);
                        $s.Data.IsPosted = post
                        $s.Prompt('Save and Post successful!', 'Success');
                    } else {
                        $s.Prompt('Save successful!', 'Success');
                    }


                    $s.Data.ID = res.Data.ID;
                    $s.Data.ReferenceNo = res.Data.ReferenceNo;
                    $s.Data.ComputedHours = res.Data.ComputedHours;
                    $s.Data.ConsideredHours = res.Data.ConsideredHours;

                    //$s.Data = res.Data;
                }
            });


        }
        $s.Cancel = function () {



        }

        $s.Unpost = function () {
            $s.Confirm('Do you wish to continue? Once unpost, Filing will return to 1st approver.', 'Warning', 'warning').then(function () {
                $s.Request('ExecuteAction', { Action: { Name: 'UnpostOvertime', Parameter: { ID: $s.Data.ID } }, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    if (res.Type == 2) {
                        $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                        $s.$apply();
                        return;
                    }
                    else {
                        $s.DisableEnableFields($s.TableSchema, 'overtimeFrm', false);
                        $s.Prompt('Unpost successful!', 'Success');

                        $s.Data.ID = res.Data.ID;
                        $s.Data.ReferenceNo = res.Data.ReferenceNo;
                        $s.Data.ComputedHours = res.Data.ComputedHours;
                        $s.Data.ConsideredHours = res.Data.ConsideredHours;
                       
                        $s.$apply();
                    }
                });
            });
        }

        $s.Post = function () {
            $s.Confirm('Do you wish to continue?', 'Confirm').then(function () {
                $s.Request('ExecuteAction', { Action: { Name: 'PostOvertime', Parameter: { ID: $s.Data.ID } }, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    if (res.Type == 2) {
                        $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                        $s.$apply();
                        return;
                    }
                    else {
                        $s.Data = res.Data;
                        $s.$apply();
                    }
                });
            });
        }

        $s.LoadApprovalHistories = function () {
            $s.Request('LoadApprovalHistory', { Data: $s.Data, MenuCode: $s.MenuCode }, $s.ApiController, false).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                    $s.$apply();
                    return;
                } else {
                    $s.ApprovalHistories = res.Data;
                }
            });
        }
        $s.Close = function () {
            $mi.close();
        }
    }])

    .controller('officialBusinessDlgCtrl', ['$scope', '$controller', '$uibModalInstance', 'dData', '$state', function ($s, $c, $mi, $dlgData, $st ) {
        $c('BaseController', { $scope: $s });
        $s.MenuCode = 'IONSDashBoardFilingApplications_ApplicationsOB';
        $s.ApiController = 'OB';
        $s.Data = {};
        $s.TableSchema = [];
        $s.Details = [];
        $s.ApprovalHistories = [];

        $s.Init = function () {
            console.log('hey', $dlgData.data.Form);
            $s.Data = $dlgData.data.Form;
            $s.TableSchema = $dlgData.data.Schema;
            $s.LoadApprovalHistories();

            if ($s.Data.IsPosted)
                $s.DisableEnableFields($s.TableSchema, 'obFrm', true);
        }

        $s.clientLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Client' }
                ]
            },
            method: 'LoadLookup',
            controller: 'OB',
            parameter: { LookupName: 'client', MenuCode: $s.MenuCode }
        };

        $s.projectLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Project' }
                ]
            },
            method: 'LoadLookup',
            controller: 'OB',
            parameter: { LookupName: 'project', MenuCode: $s.MenuCode }
        };

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        $s.Save = function (post) {
            if (!$s.IsTabsValid('obFrm', $s.TableSchema, 0, true)) return;

            $s.Request('SaveForm', { Data: $s.Data, Post: post, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                if (res.Type == 2) {
                    $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                    $s.$apply();
                    return;
                }
                if (res.Type == 7) {
                    $s.Prompt(res.Message, 'OB Filing', 'warning');
                    $s.$apply();
                    return;
                }
                else {
                    if (post) {

                        $s.DisableEnableFields($s.TableSchema, 'obFrm', true);
                        $s.Prompt('Save and Post successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.Data.ReferenceNo = res.Data.ReferenceNo
                        $s.Data.ID = res.Data.ID
                        $s.$apply();
                    } else {
                        $s.Prompt('Save successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.Data.ReferenceNo = res.Data.ReferenceNo
                        $s.Data.ID = res.Data.ID
                        $s.$apply();
                    }
                }
            });
        }

        $s.Close = function () {
            $mi.close();
        }

        $s.Unpost = function () {
            $s.Confirm('Do you wish to continue? Once unpost, Filing will return to 1st approver.', 'Warning', 'warning').then(function () {
                $s.Request('ExecuteAction', { Action: { Name: 'Unpost', Parameter: { ID: $s.Data.ID } }, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    if (res.Type == 2) {
                        $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                        $s.$apply();
                        return;
                    }
                    else {
                        $s.DisableEnableFields($s.TableSchema, 'obFrm', false);
                        $s.Prompt('Unpost successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.$apply();
                    }
                });
            });
        }

        $s.Post = function () {
            $s.Confirm('Do you wish to continue?', 'Confirm').then(function () {
                $s.Request('ExecuteAction', { Action: { Name: 'Post', Parameter: { ID: $s.Data.ID } }, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    if (res.Type == 2) {
                        $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                        $s.$apply();
                        return;
                    }
                    else {

                       
                        $s.Prompt('Post successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.$apply();
                    }
                });
            });
        }

        $s.LoadApprovalHistories = function () {
            $s.Request('LoadApprovalHistory', { Data: $s.Data, MenuCode: $s.MenuCode }, $s.ApiController,false).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.Prompt('An error occured. Please contact your Administrator', 'Error','error');
                    $s.$apply();
                    return;
                } else {
                    $s.ApprovalHistories = res.Data;
                }
            });
        }
    }])

    .controller('missedLogDlgCtrl', ['$scope', '$controller', '$uibModalInstance', 'dData', function ($s, $c, $mi, $dlgData, ) {
        $c('BaseController', { $scope: $s });
        $c('insysTableController', { $scope: $s });
        $s.Data = {};
        $s.Details = [];
        $s.ApprovalHistories = [];
        $s.MenuCode = 'IONSDashBoardFilingApplications_ApplicationsMissedLog';
        $s.ApiController = 'MissedLog';
        console.log('$dlgData', $dlgData);
        $s.UploadOptions = {
            IsMultiple: false,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };



        $s.Init = function () {

            $s.Data = $dlgData.data.Form;
            $s.TableSchema = $dlgData.data.Schema;
            $s.TableSchemaDetail = Enumerable.From($dlgData.data.SchemaDetail).Where(x => x != undefined).ToArray();
            
            $s.Details = []
            $s.LoadMissedLogDetail();

            //$s.LoadApprovalHistory();
            
            $s.LogType = $dlgData.data.LogType.Data.Rows;


            if ($s.Data.IsPosted)
                $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'missedLogForm', true);

        }
        $s.Save = function (post) {
            
            if (!$s.IsTabsValid('missedLogForm', $s.TableSchema.concat($s.TableSchemaDetail), 0, true)) return;
            
            if (post == 1) {
                $s.Confirm('Are you sure you want to Post?').then(function () {

                    $s.Data.IsPosted = 1;
                    $s.Request('SaveForm', { Data: $s.Data, Detail: $s.Details, Post: $s.varPost, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                        if (res.Type == 1) {

                            $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'missedLogForm', true);
                            $s.Prompt(res.Message, 'Missed Log Filing');

                        } else {
                            if (res.Type == 7) {
                                $s.Prompt(res.Message, 'Missed Log Filing', 'warning');
                            } else {
                                $s.Prompt(res.Message, 'Missed Log Filing', 'error');
                            }

                            return;
                        }
                    });

                });
            } else {
                $s.Data.IsPosted = 0;
                $s.Request('SaveForm', { Data: $s.Data, Detail: $s.Details, Post: $s.varPost, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    
                    console.log('res',res);
                    if (res.Type == 1) {

                        $s.Prompt(res.Message, 'Missed Log Filing').then(function () {
                            $s.Data.RefNum = res.Data.RefNo;
                            $s.Data.Id = res.Data.ID;
                        });

                    } else {
                        if (res.Type == 7) {
                            $s.Prompt(res.Message, 'Missed Log Filing', 'warning');
                        } else {
                            $s.Prompt(res.Message, 'Missed Log Filing', 'error');
                        }

                        return;
                    }

                    $s.$apply();
                });
            }

        

                
            
        }

        $s.UnPost = function () {

            $s.Confirm('Are you sure you want to Unpost?').then(function () {

                $s.Request('UnPost', { ID_MissedLog: $s.Data.ID, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {

                    if (res.Type == 1) {
                        $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'missedLogForm', false);
                        $s.Data.IsPosted = false;
                    }

                    $s.$apply();
                });


            })
           

        }

        $s.Close = function () {
            $mi.close();
        }


        $s.LoadMissedLogDetail = function () {

            if ($s.Data.ID == 0) {

                $s.Details.push({
                    ID_Company: $s.Data.ID_Company,
                    ID_MissedLog: $s.Data.ID,
                    LogTime: null,
                    ID_MissedLogType: 1,
                    CreatedAt: $s.Data.WorkDate,
                    ModifiedAt: $s.Data.FiledDate,
                    ID_CreatedBy: $s.Data.ID_Employee,
                    FilingStatus: 'Filed'
                });
                
            } else {

                $s.Request('LoadDetail', { DetailName: 'MissedLogDetail', ID_MissedLog: $s.Data.ID, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {

                    console.log('res', res);

                    $s.Details = res.Data.Rows;
                    $s.TableSchema.push(res.Data.Schema)

                    $s.$apply();

                });




            }

        }







        //$s.LoadApprovalHistory = function () {
        //    $s.Request('LoadApprovalHistory', { Data: $s.Data, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
        //        if (res.Type == 2) {
        //            $s.SetSystemStatus(res.Message, 'error');
        //            $s.Prompt('An error occured. Please contact your administrator', 'Error');
        //            $s.$apply();
        //        } else {
        //            $s.ApprovalHistories = res.Data;
        //        }
        //    });
        //}
    }])

    .controller('changeOfScheduleDlgCtrl', ['$scope', '$controller', '$uibModalInstance', 'dData', function ($s, $c, $mi, $dlgData) {
        $c('BaseController', { $scope: $s });
        $s.MenuCode = 'IONSDashBoardFilingApplications_ApplicationsChangeOfSchedule';
        $s.ApiController = 'ChangeOfSchedule';
        $s.Data = {};
        $s.TableSchema = [];
        $s.TableSchemaDetail = [];
        $s.ApprovalHistories = [];
        $s.EmpWeeklySchedule = [];
        $s.ScheduleList = [];
        $s.COSDetailOption = [];
        $s.AffectedDates = [];

        $s.Init = function () {
            $s.Data = $dlgData.data.Form;
            $s.TableSchema = $dlgData.data.Schema;
            $s.TableSchemaDetail = $dlgData.data.SchemaDetail;
            $s.EmpWeeklySchedule = $dlgData.data.EmpWeeklySchedule;
            $s.RecordID = $s.Data.ID;
            $s.LoadCOSDetails();
            $s.LoadApprovalHistories();

            $s.Request('LoadLookUp', { MenuCode: $s.MenuCode, LookupName: 'shiftschedule' }, 'ChangeOfSchedule').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ScheduleList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

            if ($s.Data.IsPosted)
                $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'cosFrm', true);
        }
        
        $s.LoadCOSDetails = function () {
            if ($s.COSDetailOption != undefined)
                if ($s.COSDetailOption != undefined)
                    if ($s.COSDetailOption.length > 0)
                        return;
            
            $s.Request('LoadDetail', { DetailName: 'cosdetail', ID_ChangeOfSchedule: $s.RecordID, Menucode: $s.MenuCode }, 'ChangeOfSchedule').then(function (res) {
                $s.COSDetailOption = res;
                $s.$apply();
            });
        }

        $s.GererateCOSDetail = function () {
            var dateRange = $s.GenerateDateRange(new Date($s.Data.StartDate), new Date($s.Data.EndDate));
            $s.COSDetailOption.Data.Rows = [];
            $s.AffectedDates = [];

            Enumerable.From(dateRange).ForEach(function (e) {
                $s.Request('LoadDetail', { DetailName: 'checkApprovedCOS', WorkDate: moment(e).format('L'), Menucode: $s.MenuCode }, 'ChangeOfSchedule').then(function (res) {
                    if (res.Type == 2) {
                        $s.SetSystemStatus(res.Message, 'error');
                        $s.Prompt('An error occured. Please contact your administrator', 'Error');
                    } else {
                        let EmpOldShiftSched = $s.EmpWeeklySchedule.find(o => o.ShiftWeekday == (moment(e).day() + 1));
                        $s.COSDetailOption.Data.Rows.push({
                            ID_ShiftSchedule_Old: EmpOldShiftSched.ID_ShiftSchedule,
                            ShiftSchedule_Old: EmpOldShiftSched.ShiftSchedule,
                            ID_ShiftSchedule_New: $s.ScheduleList[0].ID,
                            ShiftSchedule_New: $s.ScheduleList[0].Name,
                            ID_ChangeOfSchedule: $s.Data.ID,
                            Old: $s.Data.ID,
                            WorkDate: moment(e),
                            ID_CreatedBy: $s.Data.ID_Employee,
                            ID_ModifiedBy: $s.Data.ID_Employee,
                            ModifiedAt: moment(),
                            CreatedAt: moment()
                        });
                        if (res.Data.Total != 0) {
                            $s.AffectedDates.push({ Name: moment(res.Data.Rows[0].WorkDate).format('MMM. DD, YYYY - ddd.') });
                        }
                    }
                    $s.$apply();
                });
            });
        }

        $s.LoadApprovalHistories = function () {
            $s.Request('LoadApprovalHistory', { Data: $s.Data, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.Prompt('An error occured. Please contact your administrator', 'Error');
                    $s.$apply();
                } else {
                    $s.ApprovalHistories = res.Data;
                }
            });
        }

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        $s.Save = function (post) {
            if ($s.AffectedDates.length != 0) {
                $s.Prompt('Some dates are already filed in other COS filing, below is the ff.', 'COS Filing', 'warning', $s.AffectedDates);
                return;
            }

            if (!$s.IsTabsValid('cosFrm', $s.TableSchema.concat($s.TableSchemaDetail), 0, true)) return;

            $s.Request('SaveForm', { Data: $s.Data, Detail: $s.COSDetailOption.Data.Rows, Post: post, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                if (res.Type == 2) {
                    $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                    $s.$apply();
                    return;
                }
                if (res.Type == 7) {
                    $s.Prompt(res.Message, 'COS Filing', 'warning');
                    $s.$apply();
                    return;
                }
                else {
                    if (post) {
                        $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'cosFrm', true);
                        $s.Prompt('Save and Post successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.Data.ReferenceNo = res.Data.ReferenceNo
                        $s.Data.ID = res.Data.ID
                        $s.$apply();
                    } else {
                        $s.Prompt('Save successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.Data.ReferenceNo = res.Data.ReferenceNo
                        $s.Data.ID = res.Data.ID
                        $s.$apply();
                    }
                }
            });
        }

        $s.Close = function () {
            $mi.close();
        }

        $s.Unpost = function () {
            $s.Confirm('Do you wish to continue? Once unpost, Filing will return to 1st approver.', 'Warning', 'warning').then(function () {
                $s.Request('ExecuteAction', { Action: { Name: 'Unpost', Parameter: { ID: $s.Data.ID } }, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    if (res.Type == 2) {
                        $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                        $s.$apply();
                        return;
                    }
                    else {
                        $s.DisableEnableFields($s.TableSchema.concat($s.TableSchemaDetail), 'cosFrm', false);
                        $s.Prompt('Unpost successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.$apply();
                    }
                });
            });
        }

        $s.Post = function () {
            $s.Confirm('Do you wish to continue?', 'Confirm').then(function () {
                $s.Request('ExecuteAction', { Action: { Name: 'Post', Parameter: { ID: $s.Data.ID } }, MenuCode: $s.MenuCode }, $s.ApiController).then(function (res) {
                    if (res.Type == 2) {
                        $s.Prompt('An error occured. Please contact your Administrator', 'Error', 'error');
                        $s.$apply();
                        return;
                    }
                    else {
                        $s.Prompt('Post successful!', 'Success');
                        $s.Data.IsPosted = res.Data.IsPosted;
                        $s.$apply();
                        return;
                    }
                });
            });
        }
    }]);