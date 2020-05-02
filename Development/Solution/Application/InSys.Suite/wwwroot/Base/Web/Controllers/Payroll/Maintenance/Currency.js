angular.module('app')
    .controller('PayrollMaintenanceCurrency', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'Currency';
        $s.IsDetailsTabClicked = 0;

        $s.Schema = {};
        $s.TableSchema = [];

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'Comment', Label: 'Comment' },
                { Name: 'IsActive', Label: 'Is Active' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'Comment', Type: 9, Label: 'Comment' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' }
            ]
        };

        // BEGIN --- Load record

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID }, $s.myController);
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        // END ----- Load record

        //Save New - Edit - Delete
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.currency')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, CurrencyRates: $s.CurrencyRatesRecord, DeletedCurrencyRate: $s.DeletedCurrencyRates }, $s.myController).then(function (ret) {
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


        /////////////////////////CURRENCY RATES///////////////////////////

        $s.DeletedCurrencyRates = [];
        $s.CurrencyRatesRecord = [];

        $s.SelectAllCurrencyRatesRows = false;
        $s.CheckAllDailyScheduleDetailRecords = function () {
            $s.SelectAllCurrencyRatesRows = !$s.SelectAllCurrencyRatesRows;
            Enumerable.From($s.CurrencyRatesRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllCurrencyRatesRows;
            });
        }

        $s.delCurrencyRates = function () {
            var rows = Enumerable
                .From($s.CurrencyRatesRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.CurrencyRatesRecord.indexOf(rows[y]);
                        if ($s.CurrencyRatesRecord[Index].ID > 0) {
                            $s.DeletedCurrencyRates.push($s.CurrencyRatesRecord[Index].ID);
                        }
                        $s.CurrencyRatesRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.newCurrencyRates = function () {
            var newRecord = {
                ID: 0,
                Date: new Date(),
                Rate: 0,
                ID_Currency: $s.Schema.ID,
                IsActive: false,
                Comment: ''
            }
            $s.CurrencyRatesRecord.push(newRecord);
        }

        $s.LoadCurrencyRates = function () {
            if ($s.IsDetailsTabClicked == 1) return;
            $s.Request('GetCurrencyRateDetails', { ID_Currency: $s.Schema.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.CurrencyRatesRecord = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
            });

            $s.IsDetailsTabClicked = 1;
        }

    }]);