angular.module('app')
    .controller('Index', ['$scope', '$controller', '$state','$rootScope', function ($s, $c, $st,$rs) {
        $c('BaseController', { $scope: $s });
        $c('MenuController', { $scope: $s });
        $c('insysForm', { $scope: $s });

        $s.ShowMenuSearch = false;
        $s.ShowNotif = false;
        $s.searchKeyword = null;
        $s.menuFound = [];
        $s.ShowMsg = false;

        $s.IsLogOff = false; //This flag will be used to check if user click the logoff.
        $s.loadTheme();

        $(document).on('mouseup', function (ee) {
            if ($(ee.target).closest(".notification").length === 0) {
                if ($(ee.target).attr('id') != "notif-bell")
                $s.ShowNotif = false;
            }
            if ($(ee.target).closest(".menu-search-result").length === 0) {
                $s.ShowMenuSearch = false;
            }
            $s.$apply();
        });
        
        $s.Companies = []; 
        $s.ID_Employee = null;

        $s.LoadBrowserSession = function () {
            return $s.Request('BrowserSession', {}, 'Index').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.UserRow(ret.Data);
                    $s.ID_Employee = ret.Data.ID_Employee;
                }
            });
        }
        $s.LoadCompany = function () {
            return $s.Request('LoadCompany', {}, 'Index').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.Companies = ret.Data;
                    $s.SetSystemStatus('Ready');
                    $s.CtrlReady = true;
                    $s.$apply();
                }
            });
        }
        $s.RestartSession = function (id) {
            $s.Request('ServerSession', { ID_User: $s.Session('ID_User'), ID_Company: id }, 'Login').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else if (ret.Type == 1) {
                    window.location = "#!";
                    location.reload(true);
                }
            });
        }

        $rs.$on('$stateChangeStart',
            function (event, toState, toParams, fromState, fromParams) {
                //event.preventDefault();
                //if ($s.form != {}) {
                //    for (var formObject in $s.form) {
                //        if ($s.form[formObject] != undefined)
                //            if ($s.form[formObject].$dirty) {
                //                event.preventDefault();
                //                $s.Confirm('You have changes to your form, do you want to proceed?').then(function (answer) {
                //                    $s.Goto(toState, toParams);
                //                });
                //            }
                //    }
                //}
                if (parseInt($s.Session('ID_Roles')) != 1 && toState.name === 'PeopleOrganizationCompany') {
                    event.preventDefault();
                    $s.Goto('PeopleOrganizationCompany.Form', { ID: $s.Session('ID_Company') });
                }
            });
        window.addEventListener('beforeunload', (event) => {
            event.preventDefault();
            if ($s.form != {}) {
                for (var formObject in $s.form) {
                    if ($s.form[formObject] != undefined)
                        if ($s.form[formObject].$dirty) {
                            event.returnValue = '';
                        }
                }
            }
        });

        //This function is used, for unsaved changes in a form.
        $rs.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
            //event.preventDefault();
            if (!$s.IsLogOff) { //No need to ask user for changes if LogOff
                if ($s.form != {}) {
                    for (var formObject in $s.form) {
                        if ($s.form[formObject] != undefined)
                            if ($s.form[formObject].$dirty) {
                                var _toState = toState;
                                var _toParams = toParams;
                                event.preventDefault();
                                $s.Confirm('Your changes have not been saved, do you want to leave?').then(function (answer) {
                                    $s.form[formObject].$dirty = false;
                                    $s.GoToState(_toState, _toParams);
                                });
                            }
                    }
                }
            }
            if (parseInt($s.Session('ID_Roles')) != 1 && toState.name === 'PeopleOrganizationCompany') {
                event.preventDefault();
                $s.GoToState('PeopleOrganizationCompany.Form', { ID: $s.Session('ID_Company') });
            }
        });

        window.addEventListener('beforeunload', (event) => {
            event.preventDefault();
            if ($s.form != {}) {
                for (var formObject in $s.form) {
                    if ($s.form[formObject] != undefined)
                        if ($s.form[formObject].$dirty) {
                            event.returnValue = '';
                        }
                }
            }
        });


        $s.findMenu = function (e) {
            $s.searchKeyword = $(e.currentTarget).val();
            if ($s.searchKeyword != '' && $s.searchKeyword.length > 2) {
                $s.menuFound = Enumerable.From($s.bareTree).Where(function (x) { return x.Name.toLowerCase().indexOf($s.searchKeyword.toLowerCase()) > -1
                }).Select(function (x) { return { ID: x.ID, Name: x.Name, ID_Menus: x.ID_Menus, Children: x.Children, Code: x.Code } }).ToArray();
                angular.forEach($s.menuFound, function (o) {
                    $s.ShowMenuSearch = true;
                    var parentids = $s.findParent(o.ID_Menus, o.ID).reverse(),
                        containerDiv = $('<div />'),
                        defaultDiv = $('<div class="res" />'),
                        startDiv = $('<div />'),
                        coloredDiv = $('<div />'),
                        endDiv = $('<div />'),
                        crumb = $('<ul />'),
                        startIndex = o.Name.toLowerCase().indexOf($s.searchKeyword.toLowerCase()),
                        endIndex = startIndex + $s.searchKeyword.length;

                    o.MenuCrumb = parentids;
                    coloredDiv.addClass('highlight');
                    crumb.addClass('crumb');

                    startDiv.append(o.Name.substr(0, startIndex).replace(new RegExp(' ', 'g'), '&nbsp;'));
                    coloredDiv.append(o.Name.substr(startIndex, $s.searchKeyword.length).replace(new RegExp(' ', 'g'), '&nbsp;'));
                    endDiv.append(o.Name.substr(endIndex, o.Name.length - endIndex).replace(new RegExp(' ', 'g'), '&nbsp;'));
                    crumb.append($s.concatSearchMenu(parentids));
                    defaultDiv.append((startDiv.html() != '' ? startDiv : ''), coloredDiv, (endDiv.html() != '' ? endDiv : ''));
                    containerDiv.append(defaultDiv, (crumb.html() != '' ? crumb : ''));
                    o.Template = containerDiv.html();
                });

                
            } else {
                $s.menuFound = [];
            }
        }

        $s.ChangeChild = function (m) {
            if (m.Children.length == 0) {
                $s.generateLink(m);
            }
            $s.ShowMenuSearch = false;
        }

        $s.generateLink = function (d) {
            $st.go(d.Code.replace(new RegExp(' ', 'g'), '-'));
            
            $s.showMenu = false;
        }

        $s.findParent = function (id, childId) {
            var p = Enumerable.From($s.bareTree).Where(function (x) { return x.ID == id }).SingleOrDefault();
            var q = [];
            if (p != null) {
                q.push(childId);
                q.push(id);
                if (p.ID_Menus != null) {
                    q = $s.findParentParent(p.ID_Menus, q);
                }
            }
            return q;
        }

        $s.findParentParent = function (id, parents) {
            var p = Enumerable.From($s.bareTree).Where(function (x) { return x.ID == id }).SingleOrDefault();
            if (p != null) {
                parents.push(id);
                if (p.ID_Menus != null) {
                    parents = $s.findParentParent(p.ID_Menus, parents);
                }
            }
            return parents;
        }

        $s.concatSearchMenu = function (ids) {
            var str = [];
            angular.forEach(ids, function (id) {
                var p = Enumerable.From($s.bareTree).Where(function (x) { return x.ID == id }).SingleOrDefault();
                str.push('<li>' + p.Name + '</li>');
            });
            if (str.length > 0) {
                return str;
            } else {
                return null;
            }
        }

        $(document).click(function (e) {
            if (e.target.id == 'menu-search-input') return;
            $s.ShowMenuSearch = false;
        });

        $s.Logoff = function () {
            $s.IsLogOff = true; //Set flag to true, this will be used in conditional statements.
            $s.SetSystemStatus('Logging off', 'loading');
            $s.Request('Logoff', {}, 'Index').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    //window.location = "#!";
                    window.location.reload();
                }
            })
        }

        $s.Home = function () {
            window.location = "#!";
        }

        $s.Notif = function () {
            $s.ShowNotif = !$s.ShowNotif;
            console.log($s.ShowNotif)
        }

        $s.MyProfile = function () {
            if ($s.ID_Employee != null) {
                $('.m-profile').addClass('hide-profile');
                $s.Dialog({
                    template: 'MyProfile',
                    controller: 'dlgMyProfile',
                    size: 'lg',
                    animation: false,
                    windowClass: 'my-profile-dlg',
                    data: { ID_Employee: $s.ID_Employee }
                })
            }
        }

        $s.OpenMenu = function () {
            $s.Dialog({
                template: 'MenuDialog',
                controller: 'dlgMenuDialog',
                size: 'md',
                windowClass: 'menu-dlg',
                data: null
            })
        }

        $s.ViewMessage = function () {
            $s.ShowMsg = !$s.ShowMsg;
        }

        $s.HideMessage = function () {
            $s.ShowMsg = false;
        }

        $rs.$on("CallViewMessageMethod", function () {
            $s.ViewMessage();
        });

        $rs.$on("CallHideMessageMethod", function () {
            $s.HideMessage();
        });

        $s.Init = function () {
            $s.SetSystemStatus('Loading session', 'loading');
            $s.LoadBrowserSession().then(function (ret) {
                $s.SetSystemStatus('Loading menus', 'loading');
                $s.InitMenu().then(function () {
                    $s.SetSystemStatus('Loading companies', 'loading');
                    $s.$apply();
                    $s.LoadCompany();
                });
                $c('clientNotificationController', { $scope: $s });
                $s.LoadNotification();
            });
        }

        $s.Init();

        $s.ReloadSystem = function () {
            $s.Request('ReloadSystem', {}, 'Base').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.Prompt('System successfully reloaded.').then(function () {
                        window.location = "#!";
                        window.location.reload();
                    })
                }
            })
        }
    }])
    .controller('dlgMyProfile', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });
        $s.ID_Employee = $dlgData.ID_Employee;
        $s.Employee = {};
        $s.Persona = {};
        $s.Address = {};
        $s.Education = {};
        $s.EmpHist = {};
        $s.Licenses = {};

        $s.Request('LoadProfile', { ID: $s.ID_Employee }, 'Index').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message, 'error');
            } else {
                $s.Employee = ret.Data.Employee;
                $s.Persona = ret.Data.Persona;
                $s.Address = ret.Data.Address;
                $s.Education = ret.Data.Education;
                $s.EmpHist = ret.Data.EmpHist;
                $s.Licenses = ret.Data.Licenses;

                $s.SetSystemStatus('Ready');

                console.log(ret.Data)
            }
            $s.$apply();
        });

        $s.Val = function (val) {
            return val || 'N/A';
        }

        $s.close = function () {
            $('.m-profile').removeClass('hide-profile');
            $mi.close();
        }
    }])
    .controller('dlgMenuDialog', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('Index', { $scope: $s });

        $s.close = function () {
            $mi.close();
        }
    }]);