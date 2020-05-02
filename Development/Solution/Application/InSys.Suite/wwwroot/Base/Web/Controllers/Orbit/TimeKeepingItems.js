angular.module('app')
    .controller('OrbitTimeKeepingItems', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;

        $s.IsProcessed = true;

        //$s.myController = 'TimeKeepingItems';
        //$s.MenuCode = 'OrbitTimeKeepingItems';

        $s.myController = 'CostCenter'; // For Presentation Only
        $s.MenuCode = 'PeopleOrganizationCostCenter'; // For Presentation Only

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'StartDate', Label: 'Start Date' },
                { Name: 'EndDate', Label: 'End Date' },
                { Name: 'PayFrequency', Label: 'Pay Frequency' },
                { Name: 'PostingDate', Label: 'Posting Date' },
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
                    { Name: 'Name', Label: 'Name'},
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

    }]);