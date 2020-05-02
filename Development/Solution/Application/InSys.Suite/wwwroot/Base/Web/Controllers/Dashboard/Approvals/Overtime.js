angular.module('app')
    .controller('IONSDashBoardFilingApplications_ApprovalsOvertime', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
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
                { Name: 'WorkDate', Label: 'WorkDate' },
                { Name: 'StartTime', Label: 'Start Time' },
                { Name: 'EndTime', Label: 'End Time' },
                { Name: 'WorkCredit', Label: 'Work Credit' },
                { Name: 'DatedFiled', Label: 'Dated Filed' },
                { Name: 'ComputedHours', Label: 'Computed Hours' },
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

        $s.unprocessedOptions = {
            trigger: function () { },
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Name' },
                ],
                isEditable: false,
                hasOpenIcon: true,
                hasOpenIconClick: function (row) { },
                openForm: function () { }
            },
            //Method: 'LoadCity',
            //Controller: 'Province',
            Parameter: { ID_Province: $s.RecordID, MenuCode: $s.MenuCode }
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

        var ctx = $('#chartWorkCreditContainer');
        setTimeout(function () {
            $(ctx).empty();
            var canvas = $('<canvas />');
            canvas.attr('id', 'chartWorkCredit');
            ctx.append(canvas);
            setTimeout(function () {
                var chartWorkCredit = new Chart(canvas, {
                    type: 'pie',
                    data: {
                        labels: ['Payroll', 'Offset'],
                        datasets: [{
                            data: [50, 35],
                            backgroundColor: [
                                '#2474a7',
                                '#2d93d2',
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