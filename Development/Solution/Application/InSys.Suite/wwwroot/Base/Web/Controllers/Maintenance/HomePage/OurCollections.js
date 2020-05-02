angular.module('app')
    .controller('MaintenanceHomePageOurCollections', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.SelectedId = 0;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.IsDetailsTabClicked = 0;
        $s.myController = 'OurCollections';
        $s.MenuCode = 'MaintenanceHomePageOurCollections';
        $s.OurCollectionDetailRecord = [];
        $s.OurCollectionDetailSchema = [];
        $s.DeletedOurCollectionDetails = [];

        $s.tblOptions = {
            Columns: [
                { Name: 'Code' },
                { Name: 'Name' },
                { Name: 'StartDate', Label: 'Start Date', Format: 'LL' },
                { Name: 'EndDate', Label: 'End Date', Format: 'LL' },
                { Name: 'IsNeverExpired', Label: 'No Expiry' },
                { Name: 'IsActive', Label: 'Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9 },
                { Name: 'Name', Type: 9 },
                { Name: 'StartDate', Type: 9, Label: 'Start Date' },
                { Name: 'EndDate', Type: 9, Label: 'End Date' },
                { Name: 'IsNeverExpired', Type: 1, ControlType: 'radio', Label: 'No Expiry' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' }
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
        
        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.TableSchema = ret.Data.Schema;
                        $s.OurCollectionDetailSchema = ret.Data.DetailSchema;
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
            if ($s.IsTabsValid('form.ourcollections', $s.OurCollectionDetailSchema)) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, Detail: $s.OurCollectionDetailRecord, DetailsToDelete: $s.DeletedOurCollectionDetails, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
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

        $s.SelectAllOurCollectionDetailRows = false;
        $s.CheckAllOurCollectionDetailRecords = function () {
            $s.SelectAllOurCollectionDetailRows = !$s.SelectAllOurCollectionDetailRows;
            Enumerable.From($s.OurCollectionDetailRecord).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllOurCollectionDetailRows;
            });
        }

        $s.deleteOurCollectionDetails = function () {
            var rows = Enumerable
                .From($s.OurCollectionDetailRecord)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x; }).ToArray();

            rowLength = rows.length;

            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.OurCollectionDetailRecord.indexOf(rows[y]);
                        if ($s.OurCollectionDetailRecord[Index].Id > 0) {
                            $s.DeletedOurCollectionDetails.push($s.OurCollectionDetailRecord[Index].Id);
                        }
                        $s.OurCollectionDetailRecord.splice(Index, 1);
                    }
                });
            }
        }

        $s.newOurCollectionDetails = function () {
            var newRecord = {
                ID: 0,
                ID_Cart: $s.Schema.ID,
                IsPrimary: true
            }
            $s.OurCollectionDetailRecord.push(newRecord);
        }

        $s.GetOurCollectionDetails = function () {
            $s.Request('LoadDetail', { ID: $s.Schema.ID, MenuCode: $s.MenuCode }, $s.myController).then(function (res) {
                if (res.Type == 2) {
                    $s.SetSystemStatus(res.Message, 'error');
                    $s.$apply();
                } else {
                    $s.OurCollectionDetailRecord = res.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }]);