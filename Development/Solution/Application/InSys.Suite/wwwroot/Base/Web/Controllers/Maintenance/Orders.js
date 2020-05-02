angular.module('app')
    .controller('MaintenanceOrders', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.IsDetailsTabClicked = 0;
        $s.myController = 'Orders';
        $s.MenuCode = 'MaintenanceOrders';
        $s.OrderDetailRecord = [];
        $s.OrderDetailSchema = [];
        $s.DeletedOrderDetails = [];

        $s.tblOptions = {
            Columns: [
                { Name: 'OrderReference', Label: 'Order Reference' },
                { Name: 'Customer' },
                { Name: 'CustomAddress', Label: 'Address' },
                { Name: 'DeliveryCharge', Label: 'Delivery Charge' },
                { Name: 'SubTotal', Label: 'SubTotal' },
                { Name: 'Total', Label: 'Total' },
                { Name: 'PaymentMethod', Label: 'Payment Method' },
                { Name: 'PaymentReference', Label: 'Payment Reference' },
                { Name: 'FilingStatus', Label: 'Filing Status' },
                { Name: 'IsOpen', Label: 'Open' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'OrderReference', Type: 9, Label: 'Order Reference' },
                { Name: 'Customer', Type: 9 },
                { Name: 'CustomAddress', Type: 9, Label: 'Address' },
                { Name: 'DeliveryCharge', Type: 9, Label: 'Delivery Charge' },
                { Name: 'SubTotal', Type: 9, Label: 'SubTotal' },
                { Name: 'Total', Type: 9, Label: 'Total' },
                { Name: 'PaymentMethod', Type: 9, Label: 'Payment Method' },
                { Name: 'PaymentReference', Type: 9, Label: 'Payment Reference' },
                { Name: 'FilingStatus', Type: 9, Label: 'Filing Status' },
                { Name: 'IsOpen', Type: 1, ControlType: 'radio', Label: 'Open' }
            ]
        };

        $s.customerLookup = {
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
            parameter: { MenuCode: $s.MenuCode, LookupName: 'customer' }
        };

        $s.paymentmethodLookup = {
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
            parameter: { MenuCode: $s.MenuCode, LookupName: 'paymentmethod' }
        };

        $s.filingstatusLookup = {
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
            parameter: { MenuCode: $s.MenuCode, LookupName: 'filingstatus' }
        };

        $s.productLookup = {
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
            parameter: { MenuCode: $s.MenuCode, LookupName: 'product' }
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
                        $s.OrderDetailSchema = ret.Data.DetailSchema;
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
            if ($s.IsTabsValid('form.orders', $s.OrderDetailSchema)) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, Detail: $s.OrderDetailRecord, DetailsToDelete: $s.DeletedOrderDetails, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
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

        $s.SelectAllOrderDetailRows = false;
        $s.CheckAllOrderDetailRecords = function () {
            $s.SelectAllOrderDetailRows = !$s.SelectAllOrderDetailRows;
            Enumerable.From($s.OrderDetailRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllOrderDetailRows;
            });
        }

        $s.deleteOrderDetails = function () {
            var rows = Enumerable
                .From($s.OrderDetailRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.OrderDetailRecord.indexOf(rows[y]);
                        if ($s.OrderDetailRecord[Index].Id > 0) {
                            $s.DeletedOrderDetails.push($s.OrderDetailRecord[Index].Id);
                        }
                        $s.OrderDetailRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.newOrderDetails = function () {
            var newRecord = {
                ID: 0,
                ID_Cart: $s.Schema.ID,
                IsPrimary: true
            }
            $s.OrderDetailRecord.push(newRecord);
        }

        $s.GetOrderDetails = function () {
            $s.Request('LoadDetail', { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.$apply();
                } else {
                    $s.OrderDetailRecord = res.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }]);