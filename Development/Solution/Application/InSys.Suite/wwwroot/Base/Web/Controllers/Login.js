angular.module('app')
    .controller('Login', ['$scope', '$controller', '$cookies', function ($s, $c, $cookies) {
        $c('BaseController', { $scope: $s });
        $s.CtrlReady = true;
        $s.UserLogin = { Username: null, Password: null, InvalidCount: 0 };
        $s.ResultSet = {};
        $s.InvalidSystemCount = 3;
        $s.BlockSeconds = null;

        if ($s.theme == 2) {
            $('head').append('<link href="/bundles/' + localStorage.getItem("HtmlTemplatePath").replace(/\//g, "") +'-DarkMasterCss" rel="stylesheet" />');
        } else {
            $('head').append('<link href="/bundles/' + localStorage.getItem("HtmlTemplatePath").replace(/\//g, "") +'-MasterCss" rel="stylesheet" />');
        }
        if ($cookies.get('XSRF-REQUEST-TOKEN') != null) $cookies.remove('XSRF-REQUEST-TOKEN');
        $s.Login = function () {
            if ($s.UserLogin.InvalidCount > $s.InvalidSystemCount) {
                $s.ShowBlock($s.BlockSeconds);
                return;
            }
            $s.Request('Login', { Data: $s.UserLogin }, 'Login').then(function (result) {
                $s.ResultSet = result;
                
                if (result.Type == 4) {
                    //firstlogin
                    $s.Dialog({
                        template: 'FirstLogin',
                        controller: 'FirstLoginDlg',
                        size: 'sm',
                        data: result
                    });
                } else if (result.Type == 3) {
                    //password expired
                    $s.Dialog({
                        template: 'PasswordExpired',
                        controller: 'PasswordExpiredDlg',
                        size: 'sm',
                        data: result
                    })
                } else if (result.Type == 5) {
                    //invalid password
                    $s.UserLogin.InvalidCount += 1;
                    $s.InvalidSystemCount = result.Data.InvalidCount;
                } else if (result.Type == 6) {
                    //block
                    $s.UserLogin.InvalidCount += 1;
                    $s.InvalidSystemCount = result.Data.InvalidCount;
                    $s.BlockSeconds = result.Data.Seconds;
                    $s.ShowBlock($s.BlockSeconds);
                } else if (result.Type == 2) {
                    //$s.Prompt(result.Message, 'Login');
                } else {
                    if (result.Type == 1) {
                        if (result.Data.Authorize == true) {
                            $s.Request('LoadCompany', { ID: result.Data.ID }, 'Login').then(function (ret) {
                                if (ret.Type == 2) {
                                    $s.Prompt(ret.Message);
                                } else if (ret.Type == 1) {
                                    var companyList = [];
                                    companyList = ret.Data;
                                    if (companyList.length == 1) {
                                        $s.Request('ServerSession', { ID_User: result.Data.ID, ID_Company: companyList[0].ID }, 'Login').then(function (ret2) {
                                            if (ret2.Type == 2) {
                                                $s.Prompt(ret2.Message);
                                            } else if (ret2.Type == 1) {
                                                window.location.reload();
                                            }
                                        });
                                    } else {
                                        result.Data.CompanyList = companyList;
                                        $s.Dialog({
                                            template: 'CompanyList',
                                            controller: 'CompanyListDlg',
                                            windowClass: 'select-company-dlg',
                                            data: result
                                        })
                                    }
                                }
                            });
                        }
                    } else {
                        $s.Prompt(result.Message);
                    }
                }
                $s.$apply();
            });
        }
        $('.txt').keyup(function () {
            $('.login-bottom button').removeClass('error');
            $s.ResultSet = {};
            $s.$apply();
        });
        $s.ShowBlock = function (sec) {
            $s.Dialog({
                template: 'AccountBlock',
                size: 'md',
                windowClass: 'account-blocked-dlg',
                controller: ['$scope', '$uibModalInstance', '$interval', function ($sc, $d, $interval) {
                    $sc.Title = 'Account blocked';
                    var stop;
                    $sc.TickTime = function () {
                        var days = Math.trunc(sec / 86400);
                        var hours = Math.trunc((sec % 86400) / 3600);
                        var minutes = Math.trunc(((sec % 86400) % 3600) / 60);
                        var seconds = Math.trunc(((sec % 86400) % 3600) % 60);

                        var msg = (days > 0 ? days + (days > 1 ? ' days ' : ' day ') : '');
                        msg += (hours < 10 ? '0' + hours : hours) + ':';
                        msg += (minutes < 10 ? '0' + minutes : minutes) + ':';
                        msg += (seconds < 10 ? '0' + seconds : seconds);
                        sec -= 1;
                        return msg;
                    }
                    $sc.startCounter = function () {
                        if (angular.isDefined(stop)) return;
                        stop = $interval(function () {
                            if (sec >= 0) {
                                $sc.Message = $sc.TickTime();
                            } else {
                                $sc.stopCounter();
                                window.location.reload();
                            }
                        }, 1000);
                    }
                    $sc.stopCounter = function () {
                        if (angular.isDefined(stop)) {
                            $interval.cancel(stop);
                            stop = undefined;
                        }
                    }
                    $sc.OK = (function () {
                        $d.close(0);
                        $sc.stopCounter();
                    })
                    $sc.startCounter();

                    $sc.$on('$destroy', function () {
                        $sc.stopCounter();
                    });
                }],
                data: $s.ResultSet
            });
        }

    }])
    .controller('FirstLoginDlg', ['$scope', '$uibModalInstance', 'dData', '$controller', function ($s, $mi, $dlgData, $c) {
        $c('BaseController', { $scope: $s });
        $s.SecretQuestions = [];
        $s.UserData = {
            OldPassword: null,
            NewPassword: null,
            ID_SecretQuestion: null,
            SecretAnswer: null,
            ID_User: $dlgData.Data.ID
        };
        $s.Request('LoadQuestion', {}, 'Login').then(function (ret) {
            if (ret.Type == 2) {
                $s.Prompt(ret.Message);
            } else if (ret.Type == 1) {
                $s.SecretQuestions = ret.Data;
            }
        });
        $s.SetupAccount = function () {
            if ($s.UserData.OldPassword == $s.UserData.NewPassword) {
                $s.Prompt('Old and New password must not be the same.');
                return;
            }
            $s.Request('SaveAccount', { Data: $s.UserData }, 'Login').then(function (ret) {
                if (ret.Type == 2) {
                    $s.Prompt(ret.Message);
                } else if (ret.Type == 1) {
                    $s.Prompt(ret.Message).then(function () {
                        window.location.reload();
                    });
                }
            });
        }
    }])
    .controller('PasswordExpiredDlg', ['$scope', '$uibModalInstance', 'dData', '$controller', function ($s, $mi, $dlgData, $c) {
        $c('BaseController', { $scope: $s });
        $s.UserData = {
            NewPassword: null,
            ConfirmPassword: null,
            ID_User: $dlgData.Data.ID
        };
        $s.SetupAccount = function () {
            if ($s.UserData.NewPassword != $s.UserData.ConfirmPassword) {
                $s.Prompt('Password does not match.');
                return;
            }
            $s.Request('SaveAccountExpired', { Data: $s.UserData }, 'Login').then(function (ret) {
                if (ret.Type == 2) {
                    $s.Prompt(ret.Message);
                } else if (ret.Type == 1) {
                    $s.Prompt(ret.Message).then(function () {
                        window.location.reload();
                    });
                }
            });
        }
    }])
    
    .controller('CompanyListDlg', ['$scope', '$uibModalInstance', 'dData', '$controller', function ($s, $mi, $dlgData, $c) {
        $c('BaseController', { $scope: $s });
        $s.CompanyList = $dlgData.Data.CompanyList;
        
        $s.SetIcon = (function (str) {
            var abbr = "";
            str = str.split(" ");
            for (var i = 0; i < str.length; i++) {
                abbr += str[i].substr(0, 1);
            }

            if (abbr.length > 2) {
                abbr = abbr.substr(0, 2);
            }

            return abbr.toLowerCase();
        });
        $s.SelectCompany = function (id) {
            $s.Request('ServerSession', { ID_User: $dlgData.Data.ID, ID_Company: id }, 'Login').then(function (ret) {
                if (ret.Type == 2) {
                    $s.Prompt(ret.Message);
                } else if (ret.Type == 1) {
                    window.location.reload();
                }
            });
        }
        
    }]);