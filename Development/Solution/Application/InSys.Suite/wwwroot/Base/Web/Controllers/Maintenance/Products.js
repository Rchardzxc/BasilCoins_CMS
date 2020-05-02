angular.module('app')
    .controller('MaintenanceProducts', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.IsDetailsTabClicked = 0;
        $s.myController = 'Products';
        $s.MenuCode = 'MaintenanceProducts';
        $s.ProductDetailRecord = [];
        $s.ProductDetailSchema = [];
        $s.DeletedProductDetails = [];
        
        $s.tblOptions = {
            Columns: [
                { Name: 'Code' },
                { Name: 'Name' },
                { Name: 'Category', Label: 'Category' },
                { Name: 'ProductBrand', Label: 'Brand' },
                { Name: 'SRP' },
                { Name: 'SpecialPrice', Label: 'Special Price' },
                { Name: 'StockQuantity', Label: 'Stock Quantity' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9 },
                { Name: 'Name', Type: 9 },
                { Name: 'Category', Type: 9 },
                { Name: 'ProductBrand', Type: 9, Label: 'Brand' },
                { Name: 'SRP', Type: 9 },
                { Name: 'SpecialPrice', Type: 9, Label: 'Special Price' },
                { Name: 'StockQuantity', Type: 9, Label: 'Stock Quantity' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' },
            ]
        };
        
        $s.categoryLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'Code', Label: 'Code' },
                    { Name: 'Name', Label: 'Name' }
                ],
                Filters: [
                    { Name: 'Code', Type: 9, Label: 'Code' },
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ]
            },
            method: 'LoadLookup',
            controller: $s.myController,
            parameter: { MenuCode: $s.MenuCode, LookupName: 'category' }
        };

        $s.productbrandLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'Code', Label: 'Code' },
                    { Name: 'Name', Label: 'Name' }
                ],
                Filters: [
                    { Name: 'Code', Type: 9, Label: 'Code' },
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ]
            },
            method: 'LoadLookup',
            controller: $s.myController,
            parameter: { MenuCode: $s.MenuCode, LookupName: 'productbrand' }
        };

        $s.colorLookUp = {
            tblOptions: {
                Columns: [
                    { Name: 'Code', Label: 'Code' },
                    { Name: 'Name', Label: 'Name' }
                ],
                Filters: [
                    { Name: 'Code', Type: 9, Label: 'Code' },
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ]
            },
            method: 'LoadLookup',
            controller: $s.myController,
            parameter: { MenuCode: $s.MenuCode, LookupName: 'color' }
        };
        
        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.TableSchema = ret.Data.Schema;
                        $s.ProductDetailSchema = ret.Data.DetailSchema;
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
            if ($s.IsTabsValid('form.products', $s.ProductDetailSchema)) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, Detail: $s.ProductDetailRecord, DetailsToDelete: $s.DeletedProductDetails, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
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
        
        $s.SelectAllProductDetailRows = false;
        $s.CheckAllProductDetailRecords = function () {
            $s.SelectAllProductDetailRows = !$s.SelectAllProductDetailRows;
            Enumerable.From($s.ProductDetailRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllProductDetailRows;
            });
        }

        $s.deleteProductDetails = function () {
            var rows = Enumerable
                .From($s.ProductDetailRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.ProductDetailRecord.indexOf(rows[y]);
                        if ($s.ProductDetailRecord[Index].Id > 0) {
                            $s.DeletedProductDetails.push($s.ProductDetailRecord[Index].Id);
                        }
                        $s.ProductDetailRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.newProductDetails = function () {
            var newRecord = {
                ID: 0,
                ID_Product: $s.Schema.ID,
                ImageFile: null,
                ID_Color: null,
                IsPrimary: true
            }
            $s.ProductDetailRecord.push(newRecord);
        }

        $s.GetProductDetails = function () {
            $s.Request('LoadDetail', { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.$apply();
                } else {
                    $s.ProductDetailRecord = res.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }]);