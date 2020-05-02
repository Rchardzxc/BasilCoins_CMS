angular.module('app')
    .controller('IONSDashBoardFilingApplications_ApprovalsLeave', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;

        //$s.myController = 'Leave';
        //$s.MenuCode = 'DashboardApprovalsLeave';

        $s.myController = 'CostCenter'; // For Presentation Only
        $s.MenuCode = 'PeopleOrganizationCostCenter'; // For Presentation Only

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Employee', Label: 'Employee' },
                { Name: 'LeaveItem', Label: 'Leave Item' },
                { Name: 'StartDate', Label: 'Start Date' },
                { Name: 'EndDate', Label: 'End Date' },
                { Name: 'Days', Label: 'Days' },
                { Name: 'DateFiled', Label: 'Date Filed' },
                { Name: 'Reason', Label: 'Reason' },
                { Name: 'Status', Label: 'Status' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' },
            ]
        };

        $s.companyOrgOptions = {
            Data: { Rows: [] },
            trigger: function () { },
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    {
                        Name: 'ID_OrgType', Label: 'Organizational Type', ControlType: 'lookup',
                        Options: {
                            tblOptions: {
                                Columns: [
                                    { Name: 'ID', Label: '#' },
                                    { Name: 'Name', Label: 'Organizational Type' }
                                ],
                                Filters: [
                                    { Name: 'Name', Type: 9 }
                                ]
                            },
                            method: 'LoadLookup',
                            controller: 'Company',
                            parameter: { Name: 'OrgType', MenuCode: $s.MenuCode },
                        },
                        DisplayContainer: 'OrgType'
                    },
                    { Name: 'SeqNo', Label: 'Sequence No.', ControlType: 'number' },
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteCompanyOrgLevel', 'Company', tblOptions);
                },
            },
            Method: 'LoadCompanyOrg',
            Controller: 'Company',
            isEditable: true,
            Parameter: { ID: ($s.RecordID || $s.Session('ID_Company')), MenuCode: $s.MenuCode }
        };

        //Leave Type Pie Chart

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {

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

        var ctx = $('#chartLeaveTypeContainer');
        setTimeout(function () {
            $(ctx).empty();
            var canvas = $('<canvas />');
            canvas.attr('id', 'chartLeaveType');
            ctx.append(canvas);
            setTimeout(function () {
                var chartLeaveType = new Chart(canvas, {
                    type: 'pie',
                    data: {
                        labels: ['VL', 'SL', 'PL'],
                        datasets: [{
                            data: [50, 35, 15],
                            backgroundColor: [
                                '#2474a7',
                                '#2d93d2',
                                '#57a8db',
                            ],
                        }]
                    },
                    options: {
                        responsive: true
                    }
                });

            }, 100);
        }, 1090);
        

    }]);