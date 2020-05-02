angular.module('app')
    .controller('RecruitmentManpowerPlanning', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'ManPowerPlan';
        $s.Schema = [];
        $s.DetailSchema = [];
        $s.SchemaDetails = [];
        $s.CurrentSchemaDetails = [];
        $s.PositionList = [];
        $s.DeleteRows = [];
        $s.mppDetails = [];
        $s.TableSchema = [];

        $s.MenuCode = 'RecruitmentManpowerPlanning';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'RefNum', Label: 'Ref#' },
                { Name: 'Name', Label: 'Name' },
                { Name: 'FromDate', Label: 'From' },
                { Name: 'ToDate', Label: 'To' },
                { Name: 'Remarks', Label: 'Remarks' },
                { Name: 'IsActive', Label: 'Is Active' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'RefNum', Label: 'Ref#', Type: 9, ControlType: 'text'},
                { Name: 'Name', Label: 'Name', Type: 9, ControlType: 'text' },
                { Name: 'FromDate', Label: 'From', Type: 12, Value: [null, null], ControlType: 'date' },
                { Name: 'IsActive', Type: 1, ControlType: 'radio', Label: 'Active' }
            ]
        };


        $s.FilterSelectedCompany = {};
        $s.SetSelectedCompany = function () {
            $s.FilterSelectedCompany = {
                Filters: [{ Name: 'ID_Company', Value: $s.Schema.ID_Company, Type: 1 }]
            }
            return $s.FilterSelectedCompany;
        }
        $s.positionLookup = {};

        //Conform with new lookup
        $s.SetLookUp = function () {
            $s.positionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Position' }
                    ],
                    Filters: $s.SetSelectedCompany().Filters
                },
                method: 'LoadLookUp',
                controller: 'GetLookUp',
                parameter: { Name: 'Position' }
            };
        }
        $s.SetLookUp();


        $s.CompanyList = [];
        $s.IsAllCompany = ($s.Session('ID_Company') == 0);
        $s.GetLookUp = function () {
            if ($s.IsAllCompany) {
                $s.Request('LoadLookUp', { Name: 'Company' }, 'GetLookUp').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.CompanyList = ret.Data.Rows;
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.GetLookUp();
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                        $s.TableSchema = ret.Data.Schema;
                        $s.SchemaDetails = ret.Data.SchemaDetails;
                        $s.DetailSchema = ret.Data.DetailSchema;
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

        $s.saveForm = function () {
            var res = Enumerable
                .From($s.SchemaDetails)
                .Select(function (x) {
                    return {
                       ID: x.ID,
                       ID_Designation: x.ID_Designation,
                       ID_ManPowerPlans: x.ID_ManPowerPlans,
                       Plantilla: x.Plantilla,
                       HeadCount: x.HeadCount,
                       ActiveVacancies: x.ActiveVacancies,
                       Remarks: x.Remarks,
                       AdditionalInfo: x.AdditionalInfo,
                       FileName: x.FileName
                    };
                }).ToArray();
            if ($s.IsTabsValid('form.manpowerplanning') && $s.IsTabsValid('form.manpowerplanning', $s.DetailSchema,'details')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, DataDetails: res, DeletedDetails: $s.DeleteRows, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
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
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode })
        }
   
        $s.deleteSchemaDetails = function () {
            var rows =  Enumerable
                    .From($s.SchemaDetails)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x.ID }).ToArray();

            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rows.length; y++) {
                        for (var x = 0; x < $s.SchemaDetails.length; x++) {
                            if ($s.SchemaDetails[x].ID == rows[y]) {
                                $s.DeleteRows.push($s.SchemaDetails[x].ID);
                                $s.SchemaDetails.splice(x, 1);
                            }
                        }
                    }
                });
            }
        }

        $s.newSchemaDetails = function () { $s.AddPosition(); }
        $s.selected = [];
        $s.getPositionName = function (id) {
            var positionCount = $s.PositionList.length;
            var result = "";
            
            for (var x = 0; x < positionCount; x++) {
                if (id == $s.PositionList[x].ID) {
                    result = $s.PositionList[x].Name;
                    break;
                }
            }
            return result;
        }


        $s.GetSelectedCompany = function () {
            return $s.Schema.ID_Company;
        }


        $s.AddPosition = function () {
            $s.Dialog({
                template: 'Designation',
                controller: 'dlgDesignation',
                size: 'lg',
                windowClass: 'select-designation-dlg',
                data: { Data: $s.SchemaDetails, ID_Company : $s.GetSelectedCompany() }
            }).result.then(function (ret) {

                if (ret != undefined) {
                    var newSchemaDetails = [];
                    for (var x = 0; x < ret.length; x++) {
                        newSchemaDetails.ActiveVacancies = 0;
                        newSchemaDetails.AdditionalInfo = null;
                        newSchemaDetails.Excess = 0;
                        newSchemaDetails.FileName = null;
                       
                        newSchemaDetails.HeadCount = 0;
                        newSchemaDetails.ID = 0;
                        newSchemaDetails.ID_ManPowerPlans = $s.Schema.ID;
                        newSchemaDetails.ID_Designation = parseInt(ret[x].ID);
                        newSchemaDetails.OnHold = 0;
                        newSchemaDetails.Plantilla = 0;
                        newSchemaDetails.Position = ret[x].Position;
                        newSchemaDetails.Remarks = null;
                        newSchemaDetails.Replacement = 0;
                        newSchemaDetails.Vacancy = 0;
                        newSchemaDetails.Plantilla = newSchemaDetails.Plantilla == null ? 0 : newSchemaDetails.Plantilla;

                        $s.SchemaDetails.push(newSchemaDetails);
                        newSchemaDetails = [];
                    }
                }
            });
        }

        $s.SelectAllDetailRows = false;
        $s.CheckAllDetails = function () {
            $s.SelectAllDetailRows = !$s.SelectAllDetailRows;
            Enumerable.From($s.SchemaDetails).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllDetailRows;
            });
        }

        $s.UploadOptions = {
            IsMultiple: true,
            Accept: '.doc,.docx,.pdf,.txt,.xls,.xlsx,.JPEG,.PNG,.JPG',
            Method: 'UploadFile',
            Parameter: { folder: 'Files' }
        };

        $s.SelectCompany = function () {
            $s.SchemaDetails.splice(0, $s.SchemaDetails.length);
            $s.SetLookUp();
        }

    }])
    
    .controller('dlgDesignation', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });
        $s.PositionList = [];
        $s.myController = 'Designation'

        $s.ID_Company = $dlgData.ID_Company;

        $s.tblOptions = {
            Data: {
                Rows: [],
                Total: 0
            },
            Columns: [
                { Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' }
            ],
            Filters: [
                { Name: 'ID_Company', Value: $s.ID_Company, Type: 1 }
            ],
            HasDelete: true,
            SortName: 'ID',
            SortDirection: 'ASC',
            CountPerPage: [20, 60, 100],
            Pages: [],
            SelectedCount: 20,
            SelectedPage: 1
        };

        //This is wrong YET, need to update once Position module is available
        $s.LoadTable($s.tblOptions, 'LoadRecord', $s.myController, { MenuCode: $s.MenuCode }).then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message);
            } else {
                var existingPositionLength = $dlgData.Data.length;
                var positionListLength = $s.tblOptions.Data.Rows.length;

                for (var x = 0; x < existingPositionLength; x++) {
                    positionListLength = $s.tblOptions.Data.Rows.length;
                    for (var y = 0; y < positionListLength; y++) {
                        if ($dlgData.Data[x].ID_Designation == $s.tblOptions.Data.Rows[y].ID) {
                            $s.tblOptions.Data.Rows.splice(y, 1);
                            break;
                        }
                    }
                }                
                $s.SetSystemStatus('Ready');
                $s.$apply();  
            }
        });

        $s.Select = function () {
            var rows = $s.getSelected();
            $mi.close(rows);
        }

        $s.getSelected = function () {
            return Enumerable
                .From($s.tblOptions.Data.Rows)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) {
                    //var val = 
                    //var returnVal = x.ID + "-" + $("#" + x.ID).val().toString();
                    return {
                        ID: x.ID,
                        Plantilla: $("#" + x.ID).val(),
                        Position: x.Name
                    };
                }).ToArray();
        }

        $s.close = function () {
            $mi.close();
        }

    }])