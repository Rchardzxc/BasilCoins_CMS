angular.module('app')
    .controller('AdministrativeRoles', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });

        $s.MenuPrivileges = $priv.Data;
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.IsShowMenus = false;
        $s.IsShowCompanies = false;
        $s.menusTree = [];
        $s.companiesTree = [];
        $s.selectedNode = null;
        $s.TableSchema = [];


        $s.MenuCode = 'AdministrativeRoles';
        $s.myController = 'Roles';

        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'Name', Label: 'Name' },
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
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
            if ($s.IsTabsValid('form.roles')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema, Menus: $s.menusTree, Companies: $s.companiesTree, MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                    }

                    if ($s.IsTabsValid('form.roles')) {
                        if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                            $s.Prompt('You are not allowed to update this record.');
                            return;
                        }
                    }
                });
            }
        }

        $s.delete = function () {
            $s.deleteRow('DeleteRecord', $s.myController, { MenuCode: $s.MenuCode})
        }

        $s.ShowMenus = function () {

            if ($s.IsShowMenus) return;


            $s.SetSystemStatus('Loading menus', 'loading');
            $s.Request('LoadLookup', { ID: $s.RecordID, MenuCode: $s.MenuCode, LookupName: 'rolesmenu' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Ready');
                    $s.menusTree = $s.RecursiveMenuTree(ret.Data, 'ID_Menus_Parent', 'ID_Menus');
                    $s.IsShowMenus = true;
                }
                $s.$apply();
            });
        }
        $s.ShowCompanies = function () {
            if ($s.IsShowCompanies) return;
            $s.Request('LoadLookup', { ID: $s.RecordID, MenuCode: $s.MenuCode, LookupName: 'rolescompany' }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Ready');
                    $s.companiesTree = ret.Data;
                    $s.IsShowCompanies = true;
                }
                $s.$apply();
            });
        }
        $s.getSelected = function (data) {
            $s.selectedNode = data;
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
        $s.RecursiveMenuTree = function (rawData, parentKey, childKey, pID, xParentKey, xChildKey, xpID) {
            if (pID == undefined) pID = null;
            var tree = [];
            if (xParentKey != undefined && xChildKey != undefined) {
                Enumerable.From(rawData).Where(x => x[parentKey] == pID && x[xParentKey] == xpID).ForEach(function (t) {
                    var cnt = Enumerable.From(rawData).Count(x => x[parentKey] == t[childKey] && x[xParentKey] == t[xChildKey]);
                    if (cnt > 0) {
                        t.Children = $s.RecursiveMenuTree(rawData, parentKey, childKey, t[childKey], xParentKey, xChildKey, t[xChildKey]);
                    } else {
                        t.Children = [];
                    }
                    tree.push(t);
                });
            } else {
                Enumerable.From(rawData).Where(x => x[parentKey] == pID).ForEach(function (t) {
                    var cnt = Enumerable.From(rawData).Count(x => x[parentKey] == t[childKey]);
                    if (cnt > 0) {
                        t.Children = $s.RecursiveMenuTree(rawData, parentKey, childKey, t[childKey]);
                    } else {
                        t.Children = [];
                    }
                    tree.push(t);
                });
            }
            return tree;
        }
}]);