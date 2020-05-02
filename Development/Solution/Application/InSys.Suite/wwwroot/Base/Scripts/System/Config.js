angular.module('app')
    .config(['$stateProvider', function ($stateProvider) { //cfpLoadingBarProvider
        var c = "/Web/Controllers/",
            v = localStorage.getItem("HtmlTemplatePath") + "/Web/Views/";
        $stateProvider
            .state({
                name: '404',
                url: '/404',
                templateUrl: v + '404.html',
                controller: ['$scope', '$location', '$rootScope', function (s, S, R) {
                    s.GoBack = function () {
                        if (R.prevState == undefined) {
                            window.location = '/';
                        } else {
                            S.go(R.prevState.name, R.prevParams, { reload: true, inherit: false, notify: true });
                        }
                    }
                }]
            });
    }])
    .run(['$rootScope', '$http', 'DataService', '$timeout', '$document', '$uibModal', '$cookies', function ($rs, $http, $ds, $timeout, $document, $uibModal, $cookies) {
        var env = $('environment');
        var showSystemError = (env.attr("showSystemError") == null ? false : (env.attr("showSystemError").toLowerCase() == 'true' ? true : false));
        $rs.SessionID = env.attr("sessionID");
        $rs.HtmlTemplatePath = (env.attr("tmpPath") == null || env.attr("tmpPath") == undefined ? "" : env.attr("tmpPath"));
        $rs.SystemStatus = { message: "Ready", icon: "success", moreInfo: null };
        $http.defaults.headers.post['X-XSRF-TOKEN'] = $cookies.get('XSRF-REQUEST-TOKEN');

        $rs.SetSystemStatus = function (msg, icon, moreinfo) {
            if (icon == 'error') console.log(msg);
            icon = (icon != undefined && icon != null ? icon : 'success');
            moreinfo = (moreinfo == undefined ? null : moreinfo);
            if (showSystemError == false && icon == 'error') msg = "Internal system error, please contact administrator.", moreinfo = null;
            $rs.SystemStatus = { message: msg, icon: icon, moreInfo: moreinfo };
        }

        $rs.ShowSystemLoading = function () {
            $("#manualLoading").css('display', 'block');
        }
        $rs.HideSystemLoading = function () {
            $("#manualLoading").css('display', 'none');
        }
        $rs.GetSystemIcon = function (icon) {
            var ret = "fa-check-circle";
            switch (icon.toLowerCase()) {
                case "success":
                    ret = "fa-check-circle";
                    break;
                case "warning":
                    ret = "fa-exclamation-triangle";
                    break;
                case "error":
                    ret = "fa-times-circle";
                    break;
                case "loading":
                    ret = "fa-spinner fa-spin";
                    break;
                default:
                    ret = "fa-check-circle";
                    break;
            }
            return ret;
        }

        var insys = document.querySelector("insys");
        var auth = insys.innerHTML.trim();
        
        function prompt(message, title) {
            var def = new $.Deferred();
            var dlg = $uibModal.open({
                animation: true,
                templateUrl: localStorage.getItem("HtmlTemplatePath") + '/Web/Dialogs/Prompt.tmpl.html',
                controller: ['$scope', 'dData', '$uibModalInstance', function ($s, $data, $d) {

                    $s.Message = $data.message;
                    $s.Title = $data.title;

                    $s.OK = (function () {
                        $d.close(0);
                    })
                }],
                size: 'sm',
                appendTo: angular.element(document.body),
                backdrop: 'static',
                keyboard: true,
                windowClass: 'custom-dialog confirm-dlg',
                resolve: {
                    dData: {
                        message: message, title: title || document.title
                    }
                }
            });
            dlg.result.then(function (x) {
                if (x == 0)
                    def.resolve(x);
                else {
                    def.reject();
                }
            });
            return def.promise();
        }
        $ds.SetEncryption(auth);
        $ds.SetDialog(prompt);
        insys.parentNode.removeChild(insys);
        env.remove();
        auth = null;
    }]);