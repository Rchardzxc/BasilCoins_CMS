angular.module('app')
    .controller('RecruitmentRecruitmentReport', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.myController = 'RecruitmentReport';

        $s.Schema = [];
        $s.SchemaCount = [];
        $s.ApplicationStatus = [];
        $s.SelectedApplicationStatus = [];

        $s.MenuCode = 'RecruitmentRecruitmentReport';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'MRFNumber', Label: 'Ref#' },
                { Name: 'Designation', Label: 'Position'},
                { Name: 'RequestHeadCount', Label: 'Request Count' },
                { Name: 'ShortList', Label: 'Shortlist' },
                { Name: 'UnProcessed', Label: 'Unprocessed' },
                { Name: 'OnProcess', Label: 'On Process' },
                { Name: 'Hired', Label: 'Hired' },
                { Name: 'Others', Label: 'Others' },
                { Name: 'TotalCount', Label: 'Total Applicant' },
                { Name: 'TimeSpanSinceApproved', Label: 'Time Span (Days)' }
                //,
                //{ Name: 'TotalCount', Label: 'Total Applicant' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            Filters: [
                { Name: 'Designation', Type: 9, Label: 'Position' },
                { Name: 'MRFNumber', Type: 9, Label: 'MRF Number' },
            ]
        };

        $s.GetLookUp = function () {

            $s.Request('LoadResumeLookUp', { Name: 'ApplicationStatus', MenuCode: $s.MenuCode }, 'GetResumeLookUp').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.ApplicationStatus = ret.Data.LookUpData;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.GetLookUp();
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = ret.Data.Rows;
                        $s.SchemaCopy = angular.copy($s.Schema);
                        $s.SchemaSummary = ret.Data.RowCount;
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

        $s.ViewSummary = function () {
            $s.Dialog({
                template: 'MRFStatusSummaryDlg',
                controller: 'dlgMRFStatusSummary',
                size: 'sm',
                windowClass: 'mrfstatussummary-dlg',
                data: { Data: $s.SchemaSummary }
            })
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        // END ----- Load record

        //Save New - Edit - Delete
        $s.saveForm = function () {
            $s.Request('SaveForm', { Data: $s.Schema, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                    $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, { ID: $s.RecordID }, { reload: true });
                }
            });
        }

        $s.delete = function () {
            return;
            //$s.deleteRow('DeleteRecord', $s.myController)
        }

        $s.SelectAllMRFStatusRows = false;
        $s.CheckAllMRFStatusRecords = function () {
            $s.SelectAllMRFStatusRows = !$s.SelectAllMRFStatusRows;
            Enumerable.From($s.Schema).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllMRFStatusRows;
            });
        }

        $s.Move = function () {
         
            console.log($s.Schema);
            var rows = Enumerable
                .From($s.Schema)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x.ID }).ToArray();

            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to update (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rows.length; y++) {
                        for (var x = 0; x < $s.Schema.length; x++) {
                            if ($s.Schema[x].ID == rows[y]) {
                                $s.Schema[x].ID_RecruitmentStatus = $s.SelectedApplicationStatus.ID_Status;
                            }
                        }
                    }
                });
            }
        }
    }])
    .controller('dlgMRFStatusSummary', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {

        $s.SchemaSummary = $dlgData.Data;

        $s.close = function () {
            $mi.close();
        }
    }]);