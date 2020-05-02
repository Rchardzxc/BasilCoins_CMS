angular.module('app')
    .controller('PeopleMaintenanceMovementType', ['$scope', '$controller', '$priv', '$state', '$rootScope', function ($s, $c, $priv, $st, $rs) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.MenuCode = 'PeopleMaintenanceMovementType';
        $s.myController = 'MovementType'

        $s.IsMovementDetailClicked = false;
        //Initialize container for data
        $s.tblOptions = {
            Columns: [
                { Name: 'Name', Label: 'Name' },
                { Name: 'IsActive', Label: 'Is Active' },
                { Name: 'CreatedAt', Label: 'Created At', ControlType: 'date', IsReadOnly: true },
                { Name: 'CreatedBy', Label: 'Created By' },
                { Name: 'ModifiedAt', Label: 'Modified At', ControlType: 'date', IsReadOnly: true },
                { Name: 'ModifiedBy', Label: 'Modified By' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Name', Type: 9, Label: 'Name' }
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
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController, { MenuCode: $s.MenuCode }).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }

        }

        $s.MovementDetailsFieldsLookUp = [];
        $s._MovementDetailsFieldsLookUp = [];
        $s.MovementTypeFields = [];

        $s.LoadMovementTypeFields = function () {
            $s.Request("LoadDetail", { MenuCode: $s.MenuCode, ID: $s.Schema.ID }, $s.myController).then(function (ret) {
                if (ret.Type == 2)
                    $s.SetSystemStatus(ret.Message);
                else {

                    console.log("Details");
                    $s.MovementTypeFields = ret.Data;
                    $s.SetSystemStatus('Ready');
                }
            });
        }

        $s.LoadMovementDetailsFields = function () {
            $s.Request("LoadLookup", { LookupName: 'employeemovementfields', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2)
                    $s.SetSystemStatus(ret.Message, 'error');
                else {

                    console.log("Load Lookup");
                    console.log(ret);
                    $s.MovementDetailsFieldsLookUp = ret.Data.Rows;
                    $s._MovementDetailsFieldsLookUp = angular.copy($s.MovementDetailsFieldsLookUp);
                }
            });
        }

        $s.LoadDetails = function () {
            if (!$s.IsMovementDetailClicked) {
                $s.LoadMovementTypeFields();
                $s.LoadMovementDetailsFields();
                $s.IsMovementDetailClicked = true;
            }
        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, $s.myController);
        }

        $s.CheckAllMovementFields = function () { }

        $s.BeforeSave = function () {
           if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                $s.Prompt('You are not allowed to update this record.');
                return false;
            } else return true;
        }
        $s.AfterSave = function () { }

        $s.saveForm = function () {



            if (!$s.BeforeSave()) return;        
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.Request('SaveForm', {
                Data: $s.Schema,
                Detail: $s.MovementTypeFields,
                DeletedRecord: $s.DelMovementTypeRecords,
                MenuCode: $s.MenuCode
            }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                    $s.AfterSave();
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
                    $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                }
            });   
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }
        
        $s.SelectAllMovementFields = false;
        $s.CheckAllMovementFields = function () {
            $s.SelectAllMovementFields = !$s.SelectAllMovementFields;
            Enumerable.From($s.MovementTypeFields).Where(x => x.ID == 0).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllMovementFields;
            });
        }


        $s.DelMovementTypeRecords = [];
        $s.delMovementTypeRecord = function () {
            var rows = Enumerable
                .From($s.MovementTypeFields)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray(); //, ID : x.ID
            var rowLength = rows.length;
            
            if (rowLength > 0) {
                $s.Confirm('Are you sure you want to delete (' + rowLength + (rowLength > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rowLength; y++) {
                        var Index = $s.MovementTypeFields.indexOf(rows[y]);
                        
                        if ($s.MovementTypeFields[Index].Id > 0) {
                            $s.DelMovementTypeRecords.push($s.MovementTypeFields[Index].Id);
                        }
                        $s.MovementTypeFields.splice(Index, 1);
                    }
                });
            }
        }


        $s.SetFieldRecord = function (e) {
            //Set other data from Movement Field Details based on filtered results.
            var LookUpRecord = $s.MovementDetailsFieldsLookUp.filter(x => x.FieldName == e)[0];
            $s.MovementTypeFields.filter((x => x.FieldName == e))[0].DataType = LookUpRecord.DataType;
            $s.MovementTypeFields.filter((x => x.FieldName == e))[0].DisplayName = LookUpRecord.DisplayName;
            //Validate Double Details
        }

        $s.newMovementTypeRecord = function () {
            var newMoveField = {
                Id: 0,
                Id_MovementType: $s.Schema.ID,
                DataType: null,
                FieldName: null,
                DisplayName: null
            };

            $s.MovementTypeFields.push(newMoveField);
        }


        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

}]);