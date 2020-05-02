angular.module('app')
    .controller('PeopleOrganizationOrgMasterList', ['$scope', '$controller','$state', function ($s, $c,$st) {
        $c('insysTableController', { $scope: $s });
        $s.OrgTypes = [];
        $s.MenuCode = 'PeopleOrganizationOrgMasterList';
        $s.TableSchema = [];
        $s.Init = function () {
            $s.Request('LoadOrgType', { MenuCode: $s.MenuCode }, 'OrgMasterList').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'Error');
                    $s.$apply();
                } else {
                    $s.OrgTypes = ret.Data;
                    Enumerable.From($s.OrgTypes).ForEach(function (a) {
                        $s.tblOptions.Parameter.ID_OrgType = a.ID;
                        $s.MasterListOptions.push(angular.copy($s.tblOptions));
                        
                    });
                    if ($s.OrgTypes.length > 0)
                    $s.loadMasterList(0, $s.OrgTypes[0]);
                }
            });
            
        }
        $s.TempData = [];
        $s.loadMasterList = function (idx,data) {
            $s.ActiveTab = data;
            $('.orgMasterList').removeClass('show active');
            $('#OrgMasterList-' + idx).addClass('show active');
            if ($s.MasterListOptions[idx].tblOptions.Data != undefined)
                if ($s.MasterListOptions[idx].tblOptions.Data.Rows != undefined)
                    if ($s.MasterListOptions[idx].tblOptions.Data.Rows.length > 0)
                        return;
            $s.Request('LoadSchema', { Table: 'tOrgMasterList' }, 'Menu').then(function (ret2) {
                if (ret2.Type == 2) {
                    $s.SetSystemStatus(ret2.Message, 'error');
                    $s.$apply();
                } else {
                    $s.TableSchema = ret2.Data;
                    $s.MasterListOptions[idx].tblOptions.TableSchema = ret2.Data;
                    $s.MasterListOptions[idx].tblOptions.HasNew = true;
                    $s.MasterListOptions[idx].tblOptions.HasEdit = true;
                    $s.MasterListOptions[idx].tblOptions.HasDelete = true;
                    $s.MasterListOptions[idx].trigger().then(function (tblOptions) {
                        $s.MasterListOptions[idx].tblOptions = tblOptions;
                        $s.MasterListOptions[idx].tblOptions.newForm = function () {
                            var data = $s.PlotDefault({}, ret2.Data, 0);
                            data.ID_OrgType = $s.ActiveTab.ID;
                            $s.MasterListOptions[idx].tblOptions.Data.Rows.unshift(data);
                        }
                    });
                }
            });
            //For Comparing when data has been changed.
            Enumerable.From($s.MasterListOptions[idx].tblOptions.Data.Rows).ForEach(function (x) {
                $s.TempData.push(x);
            });
        }
        $s.ActiveTab = {};
        $s.MasterListOptions = [];

        $s.tblOptions = {
            trigger: function () { },
            tblOptions: {
                Data: { Rows: [] },
                Columns: [
                    //{ Name: 'ID', Label: '#' },
                    { Name: 'Code', Label: 'Code',ControlType: 'text' },
                    { Name: 'Name', Label: 'Name',ControlType: 'text' },
                    { Name: 'IsActive', Label: 'Active', ControlType: 'checkbox' }
                ],
                Filters: [
                    { Name: 'Name', Type: 9, Value: null }
                ],
                deleteRow: function (tblOptions) {
                    $s.deleteDetailRow('DeleteRecord', 'OrgMasterList', tblOptions, { MenuCode: $s.MenuCode });
                },
                isEditable: true,
                hasOpenIcon: false,
                hasOpenIconClick: function (row) { },
                openForm: function () { }
            },
            Method: 'LoadOrgMasterList',
            Controller: 'OrgMasterList',
            Parameter: { ID_OrgType: 0, Menucode: $s.Menucode },
            isEditable: true,
            hasOpenIcon: false,
            hasOpenIconClick: function (row) { },
            openForm: function () { }
        };
        $s.saveForm = function () {
            if ($s.IsTabsValid('form.orgMasterList')) {
                var data = [];
                Enumerable.From($s.MasterListOptions).ForEach(function (x) {
                    Enumerable.From(x.tblOptions.Data.Rows).ForEach(function (y) {
                        data.push(y);
                    });
                });

                if (data.length > 0) {
                    $s.Request('SaveForm', { Data: data, MenuCode: $s.MenuCode }, 'OrgMasterList').then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                            $s.$apply();
                        } else {
                            $s.Prompt('Save successful!', 'Success');
                            $s.SetDirtyFormToFalse($s.form);
$st.go($st.current.name, {}, { reload: true });
                        }
                    });
                }
            }
        }
        $s.SetSystemStatus('Loading Organization Types','Loading');
        $s.Init();
        $s.SetSystemStatus('Ready');
    }])