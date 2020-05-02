angular.module('app')
    .controller('PeopleInitialMigration', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.TableSchema = [];
        $s.MenuCode = 'PeopleInitialMigration';

        //Initialize container for data
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'FileName', Label: 'File Name' },
                { Name: 'UserCreatedBy', Label: 'Uploaded By' },
                { Name: 'DateTimeCreated', Label: 'Date/Time Created' },
            ],
            HasNew: true,
            HasDelete: false,
            HasEdit: false,

            Filters: [
                { Name: 'FileName', Type: 9, Label: 'File Name' },
            ],
            Buttons: [
                {
                    Label: 'Download Template',
                    Action: function () {
                        $s.SetSystemStatus('Generating ETF Template', 'loading');
                        $s.Request('ExecuteAction', { MethodName: 'DownloadEtf', MenuCode: $s.MenuCode}, 'InitialMigration').then(function (ret) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                            } else {
                                $s.Download(ret.Data, "Files", "ETF Template", "Application/msexcel");
                                $s.SetSystemStatus('Ready');
                            }
                            $s.$apply();
                        })
                    },
                    Icon: 'fas fa-arrow-circle-down',
                    Show: true,
                    Type: 1
                },
                //{
                //    Label: 'Upload Template',
                //    Action: function () {
                //        $s.SetSystemStatus('Uploading ETF Template', 'loading');
                //        $s.UploadFile('UploadFile', $s.UploadOptions.Parameter, $s.UploadOptions.IsMultiple, $s.UploadOptions.Accept).then(function (ret, s, files) {
                //            if (ret.Type == 2) {
                //                $s.SetSystemStatus(ret.Message, 'error');
                //            } else {
                //                $s.Request('UploadEtf', { MenuCode: $s.MenuCode, FileName_GUID: ret.Data[0].FileName, FileName: ret.Data[0].FileName.split(".CDITI.")[0] }, 'InitialMigration').then(function (ret2) {
                //                    if (ret2.Type == 2) {
                //                        if (ret2.Data == null) {
                //                            $s.SetSystemStatus(ret2.Message, 'error');
                //                        } else if (ret2.Data.FileName_GUID != "") {
                //                            $s.SetSystemStatus(ret2.Message, 'error');
                //                            $s.Download(ret2.Data.FileName_GUID, "Files", ret2.Data.FileName.replace(".xlsx", ""), "Application/msexcel");
                //                        } else {
                //                            $s.SetSystemStatus(ret2.Message, 'error');
                //                        }
                //                    } else {
                //                        console.log(ret2.Data.Sheets)
                //                        $s.SetSystemStatus('Upload Complete');
                //                    }
                //                });
                //            }
                //        });
                //    },
                //    Icon: 'fas fa-arrow-circle-up',
                //    Show: true,
                //    Type: 1
                //}
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
                        $s.UploadOptions = {
                            IsMultiple: false,
                            Accept: '.xls,.xlsx',
                            Method: 'UploadFile',
                            Parameter: { folder: 'ETF/Unprocess' },
                            IsHidden: $s.Schema.ProcessFinish == null,
                            DownloadFolder: 'ETF/Successful'
                        };
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                })
            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', 'InitialMigration', { MenuCode: $s.MenuCode}).then(function (ret) {
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
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: $s.MenuCode }, 'InitialMigration');
        }

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.migration')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, MenuCode: $s.MenuCode }, 'InitialMigration').then(function (ret) {
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
            $s.deleteRow('DeleteRecord', 'InitialMigration', { MenuCode: $s.MenuCode})
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

        
    }]);