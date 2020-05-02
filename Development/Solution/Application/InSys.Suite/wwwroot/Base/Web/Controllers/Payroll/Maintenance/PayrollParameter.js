angular.module('app')
    .controller('PayrollMaintenancePayrollParameter', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.TableSchema = {};
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.MenuCode = 'PayrollMaintenancePayrollParameter';
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'DaysPerYear', Label: 'Days Per Year' },
                { Name: 'HoursPerDay', Label: 'Hours Per Day' },
                { Name: 'MonthsPerYear', Label: 'Months Per Year' },
                { Name: 'TaxComputation', Label: 'Tax Computation' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' },
            ]
        };
        
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        //Enumerable.From($s.TableSchema).ForEach(function (data)
                        //{
                        //    if (data.ColumnName === "ID_Company") {
                        //        data.DefaultValue = $s.Session('ID_Company');
                        //    }
                        //});
                        console.log($s.TableSchema);
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'PayrollParameter', { MenuCode: $s.MenuCode }).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, 'PayrollParameter');
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', 'PayrollParameter', { MenuCode: $s.MenuCode })
        }

        $s.taxComputationLookUp = {
            tblOptions: {
                Columns: [
                    {Name: 'ID', Label: '#'},
                    { Name: 'Name', Label: 'Tax Computation' }
                ]
            },
            method: 'LoadLookup',
            controller: 'PayrollParameter',
            parameter: { LookupName: 'taxComputation', MenuCode: $s.MenuCode }
        };

        $s.CompanyLookUp = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Company' }
                ]
            },
            method: 'LoadLookup',
            controller: 'PayrollParameter',
            parameter: { LookupName: 'Company', MenuCode: $s.MenuCode }
        };

        $s.PayrollItemLookUp = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Payroll Item' }
                ]
            },
            method: 'LoadLookup',
            controller: 'PayrollParameter',
            parameter: { LookupName: 'PayrollItem', MenuCode: $s.MenuCode }
        };

        $s.saveForm = function () {
            if (!$s.IsTabsValid('form.payrollparameter', $s.TableSchema, 'general')) return;
            if (!$s.IsTabsValid('form.payrollparameter', $s.payrollItemRateOptions.TableSchema, 'payrollitemrate')) return;

            if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                $.Prompt('You are not allowed to update this record.');
                return;
            }
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.Request('SaveForm', { Data: $s.Schema, MenuCode: $s.MenuCode, PayrollItemRate: $s.payrollItemRateOptions.tblOptions.Data.Rows }, 'PayrollParameter').then(function (ret) {
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
        $s.payrollItemRateOptions = {
            trigger: function () { },
            tblOptions: {
                Data: { Rows: [], Total: 0 },
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'ID_PayrollItem', Label: 'Payroll Item', ControlType: 'lookup', Options: $s.PayrollItemLookUp, DisplayContainer: 'PayrollItem' },
                    { Name: 'Rate', Label:'Rate', ControlType:'decimal'},
                    { Name: 'Amt', Label: 'Amount', ControlType: 'decimal' },
                    { Name: 'IsActive', Label: 'Active', ControlType: 'checkbox' }
                ],
                HasNew: true,
                HasEdit: true,
                HasDelete: true,
                //Filters: [
                //    { Name: 'PayrollItem', Type: 9, Label: 'Payroll Item', ControlType: 'lookup', Options: $s.PayrollItemLookUp, DisplayContainer: 'PayrollItem' }
                //],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteDetail', 'PayrollParameter', tblOptions, { DetailName: 'payrollItemRate', MenuCode: $s.MenuCode });
                },
                isEditable: true
            },
            Method: 'LoadDetail',
            Controller: 'PayrollParameter',
            Parameter: { DetailName: 'payrollItemRate', ID_Parameter: $s.RecordID, MenuCode: $s.MenuCode }
        };

        $s.loadPayrollItemRate = function () {
            if ($s.payrollItemRateOptions.tblOptions.Data != undefined)
                if ($s.payrollItemRateOptions.tblOptions.Data.Rows != undefined)
                    if ($s.payrollItemRateOptions.tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tPayrollItemRate' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                } else {
                    $s.payrollItemRateOptions.tblOptions.TableSchema = ret2.Data;
                    $s.payrollItemRateOptions.trigger().then(function (tblOptions) {
                        $s.payrollItemRateOptions.tblOptions = tblOptions;
                        $s.payrollItemRateOptions.tblOptions.newForm = function () {
                            var defaultValue = $s.PlotDefault({}, ret2.Data, 0);
                            defaultValue.ID_Parameter = $s.RecordID;
                            $s.payrollItemRateOptions.tblOptions.Data.Rows.unshift(defaultValue);
                        }
                        $s.$apply();
                    });
                }
                $s.$apply();
            });
        }

        $s.Init();
    }]);