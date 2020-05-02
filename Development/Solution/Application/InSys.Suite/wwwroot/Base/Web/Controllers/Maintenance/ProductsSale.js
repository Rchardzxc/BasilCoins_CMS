angular.module('app')
    .controller('MaintenanceProductsSale', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.IsDetailsTabClicked = 0;
        $s.myController = 'ProductsSale';
        $s.MenuCode = 'MaintenanceProductsSale';
        $s.ProductsSaleDetailRecord = [];
        $s.ProductsSaleDetailSchema = [];
        $s.DeletedProductsSaleDetails = [];

        $s.tblOptions = {
            Columns: [
                { Name: 'Code' },
                { Name: 'Name' },
                { Name: 'Discount' },
                { Name: 'StartDate', Label: 'Start Date', Format: 'LL' },
                { Name: 'EndDate', Label: 'End Date', Format: 'LL' },
                { Name: 'StartTime', Label: 'Start Time', Format: 'LT' },
                { Name: 'EndTime', Label: 'End Time', Format: 'LT' },
                { Name: 'IsNeverExpired', Label: 'No Expiry' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9 },
                { Name: 'Name', Type: 9 },
                { Name: 'Discount', Type: 9 },
                { Name: 'StartDate', Type: 9, Label: 'Start Date' },
                { Name: 'EndDate', Type: 9, Label: 'End Date' },
                { Name: 'StartTime', Type: 9, Label: 'Start Time' },
                { Name: 'EndTime', Type: 9, Label: 'End Time' },
                { Name: 'IsNeverExpired', Type: 1, ControlType: 'radio', Label: 'No Expiry' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' }
            ]
        };

        $s.LoadProducts = function (id) {
            var dlgData = {
                Title: "Load Products",
                IsMultiSelect: true,
                EnabledLoadButton: true,
                RemoveIds: $s.ProductsSaleDetailRecord.map(x => x.ID_Products)
            };

            $s.Dialog({
                template: 'ProductList',
                controller: 'dlgProductList',
                size: 'md',
                windowClass: 'select-approver-dlg',
                data: { Data: dlgData }
            }).result.then(function (ret) {
                if (ret != undefined) {
                    for (var j = 0; j < ret.length; j++) {
                        var newRecord = {
                            ID: 0,
                            ID_ProductsSale: $s.Schema.ID,
                            ID_Products: ret[j].ID,
                            Products: ret[j].Name,
                            IsPrimary: true
                        }
                        $s.ProductsSaleDetailRecord.push(newRecord);
                    }
                    $s.CheckAllData($s.ProductsSaleDetailRecord, false);
                }
            });
        }

        $s.CheckAllData = function (Data, CheckedValue) {
            Enumerable.From(Data).ForEach(function (x) {
                x.IsChecked = CheckedValue;
            });
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.TableSchema = ret.Data.Schema;
                        $s.ProductsSaleDetailSchema = ret.Data.DetailSchema;
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
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

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.productssale', $s.ProductsSaleDetailSchema)) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, Detail: $s.ProductsSaleDetailRecord, DetailsToDelete: $s.DeletedProductsSaleDetails, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data.ID }, { reload: true });
                    }
                });
            }
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }

        $s.SelectAllProductsSaleDetailRows = false;
        $s.CheckAllProductsSaleDetailRecords = function () {
            $s.SelectAllProductsSaleDetailRows = !$s.SelectAllProductsSaleDetailRows;
            Enumerable.From($s.ProductsSaleDetailRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllProductsSaleDetailRows;
            });
        }

        $s.deleteProductsSaleDetails = function () {
            var rows = Enumerable
                .From($s.ProductsSaleDetailRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.ProductsSaleDetailRecord.indexOf(rows[y]);
                        if ($s.ProductsSaleDetailRecord[Index].Id > 0) {
                            $s.DeletedProductsSaleDetails.push($s.ProductsSaleDetailRecord[Index].Id);
                        }
                        $s.ProductsSaleDetailRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.GetProductsSaleDetails = function () {
            $s.Request('LoadDetail', { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.$apply();
                } else {
                    $s.ProductsSaleDetailRecord = res.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }])


    .controller('dlgProductList', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.MyController = 'ProductsSale';

        $s.Param = {
            Title: "Product List",
            ID_Company: 1,
            IsMultiSelect: false,
            RemoveIds: [] //This will be used to remove the existing request IDS to avoid duplicate
        }

        $.extend($s.Param, $dlgData.Data);

        $s.tblOptions = {
            Columns: [
                { Name: 'Name', Label: 'Name' },
                { Name: 'SRP', Label: 'SRP' },
                { Name: 'SpecialPrice', Label: 'Special Price' },
                { Name: 'StockQuantity', Label: 'Stock Quantity' }
            ],
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' },
                { Name: 'SRP', Type: 9, Label: 'SRP' },
                { Name: 'SpecialPrice', Type: 9, Label: 'Special Price' },
                { Name: 'StockQuantity', Type: 9, Label: 'Stock Quantity' }
            ],
            HasMultiSelect: $s.Param.IsMultiSelect,
            EnabledLoadButton: $s.Param.EnabledLoadButton
        };

        $s.LoadTable($s.tblOptions, 'ExecuteAction',
            $s.MyController, {
                MethodName: 'LoadProductList',
                MenuCode: 'MaintenanceProductsSale'
            }).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    //Remove Duplicate Ids.
                    for (var i = ret.Data.Rows.length - 1; i >= 0; --i) {
                        var data = $s.Param.RemoveIds.filter(x => { return x == ret.Data.Rows[i].ID; })[0];

                        if (data != undefined && data > 0) {
                            ret.Data.Rows.splice(i, 1);
                        }
                    }

                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });

        $s.LoadSelected = function () {
            //Load/Return all selected/checked products.
            $mi.close($s.tblOptions.Data.Rows.filter(x => { return x.IsChecked == true; }));
        }

        $s.openForm = function (emp) {
            $mi.close(emp)
        }

        $s.close = function () {
            $mi.close();
        }
    }]);