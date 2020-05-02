angular.module('app')
    .controller('PayrollMaintenancePayrollItem', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.tableSchema = [];

        $s.myController = 'PayrollItem';
        $s.MenuCode = 'PayrollMaintenancePayrollItem';


        $s.tblOptions = {
            Columns: [
                { Name: 'RefNum', Label: 'Ref#' },
                { Name: 'Employee', Label: 'Employee' },
                { Name: 'FilingStatus', Label: 'Status', ControlType: 'status' },
                { Name: 'FiledDate', Label: 'Filed Date', ControlType: 'date', IsReadOnly: true, Format: 'MMM, DD YYYY' },
                { Name: 'StartDate', Label: 'Start Date', ControlType: 'date', IsReadOnly: true, Format: 'MMM, DD YYYY' },
                { Name: 'EndDate', Label: 'End Date', ControlType: 'date', IsReadOnly: true, Format: 'MMM, DD YYYY' },
                { Name: 'TotalDays', Label: 'Total Days' },
                { Name: 'TotalDaysWithPay', Label: 'Total Days with Pay' },
                { Name: 'CreatedBy', Label: 'Created By' },
                { Name: 'ModifiedBy', Label: 'Modified By' },
                { Name: 'Reason', Label: 'Reason' },
                { Name: 'IsPosted', Label: 'Is Posted' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: []
        };



        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema
                        $s.SetIfRequired($s.TableSchema);
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
}]);
