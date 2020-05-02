angular.module('app', [
    'ngAnimate',
    'ngSanitize',
    'ngCookies',
    'ui.bootstrap',
    'ui.router',
    'textAngular',
    'ui-notification',
    'rbGrid',
    'ui.bootstrap.contextMenu'
]);
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
angular.module('app')
    .controller('BaseController', ['$scope', 'DataService', '$sce', 'Session', '$uibModal', '$rootScope', '$timeout', '$state', function ($s, $r, $sce, $ss, $uibModal, $rs, $timeout, $state) {
        //#START --STILL USING VARIABLES AND FUNCTIONS-- OPERATION CLEAN UP!!!!!!!
        $s.ShowInfoTab = true;
        $s.toggleInfoTab = function () {
            $s.ShowInfoTab = !$s.ShowInfoTab;
        }
        $s.isChrome = !!window.chrome && (!!window.chrome.webstore || !!window.chrome.runtime);
        $s.isFirefox = typeof InstallTrigger !== 'undefined';
        $s.isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
        $s.isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || (typeof safari !== 'undefined' && safari.pushNotification));
        
        $s.theme = 1;

        if (localStorage.getItem('theme') != undefined) {
            $s.theme = localStorage.getItem('theme');
        }

        $s.SwitchTheme = function () {
            var isDefault = $s.theme == 1 ? true : false;

            if (!isDefault) {
                localStorage.setItem('theme', 1);
            } else {
                localStorage.setItem('theme', 2);
            }

            $s.loadTheme();
        }

        $s.loadTheme = function () {
            if (localStorage.getItem('theme') != undefined) {
                $s.theme = localStorage.getItem('theme');
            }

            if ($s.theme == 2) {
                $("link[href='/bundles/Base-MasterCss']").remove();
                $('head').append('<link href="/bundles/' + localStorage.getItem("HtmlTemplatePath").replace(/\//g, "") + '-DarkMasterCss" rel="stylesheet" />');
            } else {
                $("link[href='/bundles/Base-DarkMasterCss']").remove();
                $('head').append('<link href="/bundles/' + localStorage.getItem("HtmlTemplatePath").replace(/\//g, "") + '-MasterCss" rel="stylesheet" />');
            }
        }

        $s.SetIcon = (function (str) {
            if (str != undefined) {
                var abbr = "";
                str = str.split(" ");
                for (var i = 0; i < str.length; i++) {
                    abbr += str[i].substr(0, 1);
                }

                if (abbr.length > 2) {
                    abbr = abbr.substr(0, 2);
                }

                return abbr.toLowerCase();
            }
        });
        $s.FormatDate = function (d, f) {
            
            if (d == null || d == '') return null;
            return moment(new Date(d)).format(f ? f : 'MMM, DD YYYY');
        }
        $s.FormatCurrency = function (nStr) {

            if (nStr == undefined || nStr == null) return;


            var _nStr = 0;
            if (nStr.toString().indexOf(',') >= 0)
                _nStr = nStr.replace(',', '');
            else _nStr = nStr;
            _nStr = parseFloat(_nStr);
            _nStr = _nStr.toFixed(2);
            _nStr += '';
            x = _nStr.split('.');
            x1 = x[0];

            x2 = x.length > 1 ? '.' + x[1] : '';
            var rgx = /(\d+)(\d{3})/;
            while (rgx.test(x1)) {
                x1 = x1.replace(rgx, '$1' + ',' + '$2');
            }
            return x1 + x2;
        }

        $s.SetSystemStatus = function (message, type, autoShowMsg) {
            $rs.SetSystemStatus(message, type);
            //Automatically show and hide message, normally used in after saving
            if (autoShowMsg == true) {
                $rs.$emit("CallViewMessageMethod", {});
                $timeout(function () {
                    $rs.$emit("CallHideMessageMethod", {});
                }, 1500);
            }
        }
        
        $s.SetEncryption = (function (token, salt) {
            $r.SetEncryption(token, salt);
        });
        $s.UserRow = function (data) {
            $ss.UserRow(data);
        }
        $s.Dialog = (function (opt) {
            return $uibModal.open({
                animation: true,
                templateUrl: opt.templateUrl || (localStorage.getItem("HtmlTemplatePath") + '/Web/Dialogs/' + opt.template + '.tmpl.html'),
                controller: opt.controller,
                size: opt.size || 'lg',
                appendTo: angular.element(document.body),
                backdrop: 'static',
                keyboard: true,
                windowClass: 'custom-dialog ' + opt.windowClass,
                resolve: opt.resolve || {
                    dData: opt.data || null
                }
            });
        });
        $s.Confirm = (function (message, title, icon) {
            var def = new $.Deferred();
            var dd = $s.Dialog({
                controller: ['$scope', 'dData', '$uibModalInstance', function ($s, $data, $d) {

                    $s.Message = $data.message;
                    $s.Title = $data.title;
                    $s.Icon = icon || 'info';
                    $s.Cancel = (function () {
                        $d.close(1);
                    })

                    $s.OK = (function () {
                        $d.close(0);
                    })
                }],
                template: 'Confirm',
                size: 'sm',
                windowClass: 'confirm-dlg',
                data: {
                    message: message, title: title || document.title
                }
            });
            dd.result.then(function (x) {
                if (x == 0)
                    def.resolve(x);
                else {
                    def.reject();
                }
            });
            return def.promise();
        });
        $s.Prompt = (function (message, title, icon, arrMsg) {
            var def = new $.Deferred();
            var dd = $s.Dialog({
                controller: ['$scope', 'dData', '$uibModalInstance', function ($s, $data, $d) {
                    $s.Message = $data.message;
                    $s.arrMsg = arrMsg;
                    $s.Title = $data.title;
                    $s.Icon = icon || 'info';
                    $s.OK = (function () {
                        $d.close(0);
                    })
                }],
                template: 'Prompt',
                size: 'sm',
                windowClass: 'confirm-dlg',
                data: {
                    message: message, title: title || document.title
                }
            });
            dd.result.then(function (x) {
                if (x == 0)
                    def.resolve(x);
                else {
                    def.reject();
                }
            });
            return def.promise();
        });

        $s.Request = function (Name, Param, Controller, disableInterceptor) {
            Controller = (Controller == undefined || Controller == null ? $s.GetCurrentController : Controller);
            //console.log(Name, Param, Controller, disableInterceptor);
            return $r.Post(Name, Param, Controller, disableInterceptor).then(function (ret) {
                if (ret.Type == 8) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                    $state.go('404', {}, { reload: true });
                } else {
                    return ret;
                }
            }).fail(function (e) {
                console.log(Name + ': ' + e.toString());
                if (e.toString().replace('Error: ', '') == "Session Expired.") {
                    $s.Prompt(e.toString().replace('Error: ', '')).then(function () {
                        window.location.reload();
                    });
                }
                $rs.SetSystemStatus(e.toString().replace('Error: ', ''), 'error');
                return e;
            });
        }
        $s.ByteLength = (function (obj) {
            if (typeof obj === 'object')
                obj = JSON.stringify(obj);
            return $r.ByteLength(obj);
        });
        //$s.Download = (function (Name, Param) {
        //    return $r.Download(Name, Param, 'Download').fail(function (e) {
        //        $s.SetSystemStatus(e.toString(), 'error');
        //    });
        //});
        $s.Download = (function (mFile, Container, origFileName, contentType) {
            return $r.Download(mFile, Container, origFileName, contentType).then(function (ret) {
                if (ret != undefined) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Ready');
                    }
                } else {
                    $s.SetSystemStatus('Ready');
                }
            }).catch(function (data, e) {
                $s.SetSystemStatus(e.toString(), 'error');
                //var msg = e.toString().substr(0, 40);
                //if (msg.length < e.toString().length) msg = msg + '...';

                //$t.error(e.toString(), document.title);
            });;
        });
        $s.UrlApi = (function (Name) {
            return $r.UrlApi(Name);
        });
        $s.ActionUrl = function (action, controller, param) {
            var url;
            if (controller.toLowerCase().indexOf("controller") >= 0) {
                url = '/' + controller.replace('Controller', '') + '/' + action;
            } else {
                url = '/' + controller + '/' + action;
            }
            if (vcl.String.StartsWith(url, '/http'))
                return url.substr(1);

            if (param) {
                var h = [];
                var jh = Object.keys(param);
                for (var kk in jh)
                    h.push(jh[kk] + '=' + param[jh[kk]]);
                url = url + '?' + h.join('&');
            }

            window.location = url;
        };
        $s.IsNull = (function (inp, oup) {
            return inp == null || typeof inp == typeof undefined || inp == '' ? oup : inp;
        });
        $s.Goto = (function (path, parameter) {
            $state.go(path, parameter || {}, { reload: true });
        })
        $s.UploadFile = function (Name, Param, IsMultiple, Accept) {
            try {
                var def = $.Deferred();
                var accp = "";

                if (Accept)
                    accp = ' accept="' + Accept + '" ';

                var fle = $('<input type="file" ' + (IsMultiple ? 'multiple' : '') + accp + ' />');

                fle.change(function (d) {
                    setTimeout(function () {
                        try {
                            if (fle[0].files.length > 10) {
                                $s.SetSystemStatus('Maximum of 10 files per upload.', 'warning');
                                def.reject();
                                return;
                            }

                            if (fle[0].files.length == 0) {
                                def.reject();
                                return;
                            }

                            if (Accept) {
                                var b = false;
                                var acc = Accept.split(',');
                                for (var c = 0; c < acc.length; c++) {
                                    var kgb = acc[c].trim().indexOf('*') !== -1;
                                    if (kgb) {
                                        var gg = acc[c].trim().split('/');
                                        if (fle[0].files[0].type.toLowerCase().startsWith(gg[0].toLowerCase())) {
                                            b = true;
                                            break;
                                        }
                                    } else {
                                        if (acc[c].trim().toLowerCase() === "." + fle[0].files[0].name.split('.')[fle[0].files[0].name.split('.').length - 1].toLowerCase()) {
                                            b = true;
                                            break;
                                        }
                                    }
                                }
                                if (!b) {
                                    def.reject("Invalid File");
                                    return;
                                }
                            }

                            var fd = new FormData();
                            if (Param) {
                                var ky = Object.keys(Param);
                                for (var j in ky)
                                    fd.append(ky[j], Param[ky[j]]);
                            }

                            for (var fi = 0; fi < fle[0].files.length; fi++) {
                                fd.append("fileToUpload_" + fi.toString(), fle[0].files[fi]);
                            }
                            $r.Upload(Name, fd).then(function (d, s) {
                                def.resolve(d, s, fle[0].files);
                            }).fail(function (e) {
                                var msg = e.toString().substr(0, 40);
                                if (msg.length < e.toString().length) msg = msg + '...';

                                $s.SetSystemStatus("$r.Upload " + e.toString(), 'error');
                                //$t.error(e.toString(), document.title);
                                def.reject();
                            });
                        } catch (ex) {
                            console.error(ex);
                            $s.SetSystemStatus(ex, 'error');
                        }
                    }, 100);
                });

                fle.trigger('click');
                return def.promise();
            } catch (ex) {
                console.error(ex);
            }
        }
        $s.UploadFileSlim = (function (Container, Param) {
            var p = Param || {
            };

            p.Container = Container;

            if (!p.UseOriginalName) p.UseOriginalName = false;

            return $s.UploadFile('UploadFile', p);
        })
        $s.Task = (function (action, timeout) {
            var def = $.Deferred();
            if (action)
                action(def);
            else
                $timeout(function () { def.resolve(); }, timeout || 100);

            return def.promise();
        });
        $s.WaitForElement = (function (query) {
            var def = $.Deferred();
            var retry = 0;
            var act = function () {
                var j = setTimeout(function () {
                    var k = $(query);
                    clearTimeout(j);
                    if (k.length > 0)
                        def.resolve(k);
                    else {
                        retry++;

                        if (retry < 10)
                            act();
                        else
                            def.reject('Could not find Element')
                    }
                }, 100);
            }

            act();

            return def.promise();
        });

        $s.SetDirtyFormToFalse = function (form) {
            if (form != {}) {
                for (var formObject in form) {
                    if (form[formObject] != undefined)
                        if (form[formObject].$dirty)
                            form[formObject].$dirty = false;
                }
            }
        }

        $s.CompareObject = function (MasterObj, ReferenceObj) {
            var HasChanges = false;
            for (var key in MasterObj) {
                if (MasterObj[key] != ReferenceObj[key]) {
                    HasChanges = true;
                    break;
                }
            }

            return HasChanges;
        }
        $s.GetFileNameWithOutExtension = (function (file) {
            return vcl.Path.GetFileName(file);
        });
        $s.TrustSrc = function (src) {
            return $sce.trustAsResourceUrl(src);
        }
        $s.StringFormat = function (text) {
            var args = Array.prototype.slice.call(arguments, 1);
            return text.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] != 'undefined'
                    ? args[number]
                    : match;
            });
        };
        $s.SQLFormat = function (obj, suffix) {
            var s = obj;
            if (obj === null || typeof obj === typeof undefined) {
                s = "NULL";
            }
            else {
                if (obj === true || obj === false || obj === 'true' || obj === 'false') s = (obj === true || obj === 'true') ? 1 : 0;
                else if (isNaN(obj)) s = "'" + obj + ((suffix) ? '%' : '') + "'";
            }
            return s;
        }
        $s.DataBank = function (Name, Value) {
            if (Value)
                $ss.DataBank(Name, Value);
            else
                return $ss.DataBank(Name);
        }
        $s.Title = function (Name) {
            document.title = Name + ' - ' + $s.Session('Company');
        }
        $s.Session = (function (Name) {
            return $ss.Session(Name);
        });
        $s.SessionUpdate = function (Name, Value) {
            $ss.Update(Name, Value);
        }
        $s.PassParameter = (function (cmd, row, passkey, fixColumn) {
            try {
                if (cmd === undefined || cmd === null) return;

                var rfmrt = function (data) {
                    return (data == 'NULL') ? data.toLowerCase() : data;
                }
                var urow = row || $ss.UserRow();
                passkey = passkey || '@';
                var keys = Object.keys(urow);

                if (fixColumn) {
                    for (var i = 0; i < fixColumn.length; i++) {
                        var gg = fixColumn[i].split('.');
                        cmd = cmd.replace(new RegExp('\\' + gg[0] + '\\b' + gg[1] + '\\b', 'g'), rfmrt($s.SQLFormat(urow[gg[1]])));
                    }
                }

                for (var i = 0; i < keys.length; i++) {
                    if (passkey instanceof Array) {
                        passkey.forEach(function (k) {
                            cmd = cmd.replace(new RegExp('\\' + k + '\\b' + keys[i] + '\\b', 'g'), rfmrt($s.SQLFormat(urow[keys[i]])));
                        });
                    } else
                        cmd = cmd.replace(new RegExp('\\' + passkey + '\\b' + keys[i] + '\\b', 'g'), rfmrt($s.SQLFormat(urow[keys[i]])));
                }
                return cmd;
            } catch (ex) {
                console.error(ex.message, cmd);
            }
        });
        $s.hasValue = function (data) {
            if (!data) return false;
            if (typeof data == "string") if (data.trim() === "") return false;
            return true;
        }
        $s.SetFromSystemQueryParameter = function (data) {
            if (!$s.hasValue(data)) return data;

            var systemParam = vcl.Options.SystemParameters;

            for (var i = 0; i < systemParam.length; i++) {
                var row = systemParam[i];
                var key = '@' + row.Key;
                if (row.Value) {
                    var value = row.Value.toString();
                    data = data.replace(key, value);
                }
            }

            return data;
        }
        $s.FormatColumn = function (colname, row) {
            var r = row[colname];
            if (typeof r == 'string' || r instanceof String) {
                if (colname.toLowerCase().indexOf('datetime') !== -1 && vcl.DateTime.IsNewtonFormat(r))
                    r = vcl.DateTime.Format(r, 'mmm dd, yyyy' + ' - ' + 'hh:MM TT');
                else if (colname.toLowerCase().indexOf('date') !== -1 && vcl.DateTime.IsNewtonFormat(r))
                    r = vcl.DateTime.Format(r, 'mmm dd, yyyy');
                else if (vcl.DateTime.IsNewtonFormat(r))
                    r = vcl.DateTime.Format(r, 'hh:MM TT');

            } else if (r && !isNaN(r) && colname.toLowerCase() !== 'id' && colname.toLowerCase() !== 'year') {
                r = $s.FormatNumber(r); // parseFloat(r).toFixed(2).toLocaleString();
            }
            return r;
        }
        $s.FormatNumber = function (x, decimal) {
            var parts = parseFloat(x).toFixed(decimal || 2).toString().split(".");
            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return parts.join(".");
        }
        $s.Compress = (function (obj) {
            return LZString.compressToEncodedURIComponent(JSON.stringify(obj));
        });
        $s.Decompress = (function (obj) {
            return JSON.parse(LZString.decompressFromEncodedURIComponent(obj));
        });
        $s.GetMenu = function (ID) {
            return $r.GetMenu(ID);
        }
        $s.convertDataURIToBinary = function (dataURI) {
            var BASE64_MARKER = ';base64,';
            var base64Index = dataURI.indexOf(BASE64_MARKER) + BASE64_MARKER.length;
            var base64 = dataURI.substring(base64Index);
            var raw = window.atob(base64);
            var rawLength = raw.length;
            var array = new Uint8Array(new ArrayBuffer(rawLength));

            for (var i = 0; i < rawLength; i++) {
                array[i] = raw.charCodeAt(i);
            }
            return array;
        }
        $s.DefaultColumns = (function () {
            return [
                {
                    'ID': 0, 'Name': 'ID', 'EffectiveLabel': 'Reference ID'
                },
                { 'ID': 0, 'Name': 'Name', 'EffectiveLabel': 'Name' },
            ]
        });
        $s.LongDate = function (d) {
            var dateTime = vcl.DateTime;
            return (d != null) ? dateTime.Format(d, dateTime.masks.inSysDateTime) : null;
        }
        $s.LoadCombo = function (DataSource, Name, FixedFilter) {
            return $s.Request('LoadCombo', {
                DataSource: DataSource, filter: FixedFilter, Name: Name
            });
        }
        $s.PlotDefault = function (form, schema, id) {
            if (id == 0 || id == '') {
                Enumerable.From(schema).ForEach(function (sch) {
                    if (sch.ColumnName.toLowerCase() == 'id') {
                        form[sch.ColumnName] = $s.IsNull(sch.DefaultValue, 0);

                    } else {
                        if (sch.DataType.toLowerCase() == "int" || sch.DataType.toLowerCase() == "decimal") {
                            if (form[sch.ColumnName] == null || form[sch.ColumnName] == 0 || form[sch.ColumnName] == undefined) {
                                form[sch.ColumnName] = $s.IsNull(sch.DefaultValue, "").indexOf('@') == 0 ? $s.Session(sch.DefaultValue.substr(1)) : ((sch.DefaultValue >= 0 || sch.DefaultValue == '0') && sch.DefaultValue != null ? parseInt(sch.DefaultValue) : sch.DefaultValue);
                            }
                        }
                        else if (sch.DataType == "bit") {
                            if (form[sch.ColumnName] == null) {
                                form[sch.ColumnName] = sch.DefaultValue == "1" ? true : false;
                            }
                        }
                        else {
                            if (form[sch.ColumnName] == null) {
                                form[sch.ColumnName] = (sch.DefaultValue != null ? (sch.DefaultValue.indexOf('@') == 0 ? $s.Session(sch.DefaultValue.substr(1)) : sch.DefaultValue) : null);
                            }
                        }
                    }

                });
            }
            return form;
        }
        $s.GetMaxLength = function (columnName, schema) {
            return Enumerable.From(schema || $s.TableSchema).Where(x => x.ColumnName == columnName).Select(x => x.Length).FirstOrDefault();
        }

        $s.SetMaxLengthForVarchar = function (TableSchema) { //POWERED BY BRIAN BUEN ANTONIO REMEMBER ME
            //Applicable only for varchar
            if (TableSchema != undefined) {
                //ensure datatype is varchar
                var _TableSchema = TableSchema.filter(x => (x.DataType == "varchar"));
                var x = 0;
                var input_name = "";
                if (_TableSchema.length > 0) {
                    while (x < _TableSchema.length) {
                        //For input type text
                        input_name = "input[name=" + _TableSchema[x].ColumnName + "]";
                        if (input_name.length > 0) {
                            $(input_name).prop("maxlength", _TableSchema[x].Length);
                        }
                        x++;
                    }
                }
            }
        }
        $s.SetIfRequired = function (TableSchema) { //POWERED BY BRIAN BUEN ANTONIO REMEMBER ME
            if (TableSchema != undefined) {
                var input_name = "";
                var x = 0;
                if (TableSchema.length > 0) {
                    while (x < TableSchema.length) {
                        //For input type text
                        input_name = "input[name=" + TableSchema[x].ColumnName + "]";
                        if ($(input_name).length > 0) { //for input
                            $(input_name).prop("required", !TableSchema[x].AllowNull);
                            //$(input_name).prop("type", TableSchema[x].DataType === "int" ? "number" : "text");

                            //if (TableSchema[x].DataType === "int") {
                            //    console.log(TableSchema[x].ColumnName);
                            //}

                            $(input_name).parent().attr("required", !TableSchema[x].AllowNull);
                            $(input_name).parent().parent().attr("required", !TableSchema[x].AllowNull);
                        }
                        select_name = "select[name=" + TableSchema[x].ColumnName + "]"; //for combobox
                        if ($(select_name).length > 0) {
                            $(select_name).prop("required", !TableSchema[x].AllowNull);
                            $(select_name).parent().attr("required", !TableSchema[x].AllowNull);
                        } 
                        if (TableSchema[x].ColumnName.toLowerCase().includes("date")) { //for dates
                            if ($(input_name).length > 0) {
                                $(input_name).parent().parent().attr("required", !TableSchema[x].AllowNull);
                            }
                        }
                        
                        x++;
                    }
                }
            } else {
                console.log("TableSchema not found.");
            }
        }
        $s.regex = function (data) {
            var patt = new RegExp("^[0-9]*$");
            if (patt.test(data) == false) {
                $s.SetSystemStatus("doesnt match", 'error');
                //$s.Prompt('help'); //message box
                return;
            }
        }

        $s.SetupInputField = function (TableSchema) {
            //$s.Compiled = function (TableSchema) {
                $s.SetMaxLengthForVarchar(TableSchema);
                $s.SetIfRequired(TableSchema);
            //}
            //$(document).prop('onload', function () {
            //    $s.Compiled(TableSchema);
            //});
        }

        //#END

        $s.GetTableSchema = function (table) {
            if (table == undefined || table == null || table == '') {
                console.log('Error', "No table name defined");
                return;
            }
            return $s.Request('LoadSchema', { Table: table }, 'Menu');
        }
        $s.RequestSchema = function (tbl) {
            $s.GetTableSchema(tbl).then(function (ret) {
                return ret.Data;
            });
        }

        $s.DataSort = function (data, columnName, order) {
            return data.sort(function (a, b) {
                if (a[columnName] < b[columnName])
                    return (order == 'asc' ? -1 : 1);
                if (a[columnName] > b[columnName])
                    return (order == 'asc' ? 1 : -1);
                return 0;
            });
        }

        $s.IsTabsValid = function (formName, tableSchema, tabId,isDialog) {
            function findPos(containerWithScroll, obj) {
                $(containerWithScroll).scrollTop($(containerWithScroll).height() + $(obj).offset().top);
                $(containerWithScroll).scrollLeft($(containerWithScroll).width() + $(obj).offset().left);
            }
            tabId = tabId == undefined ? "xxx" : '#' + tabId;
            if (tableSchema == undefined || tableSchema == null)
                tableSchema = $s.TableSchema;
            if (tableSchema.length == 0 || tableSchema == undefined) {
                $s.Prompt('Table Schema is undefined or empty. Please report immediately.');
                return;
            }

            //Check if Schema has value based from TableShema
            //var RequiredfField = (tableSchema.filter(x => { return x.AllowNull == false; }));
            //Enumerable.From(RequiredfField).ForEach(function (val) {
            //    if ($s.Schema[val.ColumnName] == null) {
            //        console.log($s.Schema[val.ColumnName]);
            //        console.log(val);
            //    }
            //});

            var countError = 0;
            var form = $('form[name="' + formName + '"]');

            if (form.length > 0) {
                
                var tabs = form.find(tabId + '[role="tabpanel"]');
                if (tabs.length == 0) {
                    var errorCol = [];
                    form.find('input,textarea,select:visible').not('.listing-footer select, input[type="checkbox"], .insys-filter input,.insys-filter textarea,.insys-filter select,[text-angular] textarea').each(function (idx, elem) {


                        var elemName = $(elem).attr('name');
                        var column = Enumerable.From(tableSchema).Where(function (x) { return x != undefined && x.ColumnName.toLowerCase() == elemName.toLowerCase() }).FirstOrDefault();
                        if (column != undefined && column != null) {
                            if (!column.AllowNull)
                                if ($(elem).val() == "" || $(elem).val() == null || $(elem).val() == undefined || $(elem).val() == 'object:null' || $(elem).val() == "?" || $(elem).val() == "? number:0 ?") {
                                    if (errorCol.indexOf(elemName) == -1) errorCol.push(elemName);
                                    countError += 1;
                                }
                        }
                        else if ($(elem).prop('required')) {

                            if ($(elem).val() == "" || $(elem).val() == null || $(elem).val() == undefined || $(elem).val() == 'object:null' || $(elem).val() == "?" || $(elem).val() == "? number:0 ?") {
                                if (errorCol.indexOf(elemName) == -1) errorCol.push(elemName);
                                countError += 1;
                            }
                        }
                    });
                    if (errorCol.length > 0) {
                        var elem = form.find('input,textarea,select:visible').not('.listing-footer select, input[type="checkbox"], .insys-filter input,.insys-filter textarea,.insys-filter select,[text-angular] textarea');
                        //setTimeout(function () {
                        //    findPos($(".tab-content"), objError);
                        //}, 200);
                        for (var idx in errorCol) {
                            $(elem).filter('[name=' + errorCol[idx] + ']').each(function (xx, elReq) {
                                if ($(elReq).val() == "" || $(elReq).val() == null || $(elReq).val() == undefined || $(elReq).val() == 'object:null' || $(elReq).val() == "?" || $(elem).val() == "?" || $(elem).val() == "? number:0 ?") {
                                    errorCol[idx] = $(elReq).attr('c-label') || $(elReq).attr('name')
                                    $(elReq).addClass('required');
                                    $(elReq).on('focus', function () {
                                        $(this).removeClass('required');
                                    })
                                }
                            })
                        }

                        if (isDialog) {
                            $s.Prompt(errorCol.join(', ') + ' is required.', 'InSys', 'warning')
                        } else {
                            $s.SetSystemStatus(errorCol.join(', ') + ' is required.', 'InSys', 'warning');
                        }
                       
                    }
                } else {
                    tabs.each(function (idx, tb) {
                        var header = $(tb).find('.rf-header').first().text() || $(form).parent().find('ul[role="tablist"]:visible').find('a[data-target="#' + $(tb).attr('id') + '"]').text();
                        var tabRequired = null;
                        var errorCol = [];
                        var objError = null;
                        $(tb).find('input,textarea,select:visible').not('.listing-footer select, input[type="checkbox"], .insys-filter input,.insys-filter textarea,.insys-filter select,[text-angular] textarea').each(function (idx, elem) {
                            var elemName = $(elem).attr('name');
                            var column = Enumerable.From(tableSchema).Where(function (x) { return x != undefined && x.ColumnName.toLowerCase() == elemName.toLowerCase() }).FirstOrDefault();
                            if (column != undefined && column != null) {
                                if (!column.AllowNull)
                                    if ($(elem).val() == "" || $(elem).val() == null || $(elem).val() == undefined || $(elem).val() == 'object:null' || $(elem).val() == "?" || $(elem).val() == "? number:0 ?") {
                                        if (errorCol.indexOf(elemName) == -1) errorCol.push(elemName);
                                        countError += 1;
                                        tabRequired = $(tb);
                                        if (objError == null) objError = $(elem);
                                    }
                            }
                            else if ($(elem).prop('required')) {
                                if ($(elem).val() == "" || $(elem).val() == null || $(elem).val() == undefined || $(elem).val() == 'object:null' || $(elem).val() == "?" || $(elem).val() == "? number:0 ?") {
                                    if (errorCol.indexOf(elemName) == -1) errorCol.push(elemName);
                                    countError += 1;
                                    tabRequired = $(tb);
                                    if (objError == null) objError = $(elem);
                                }
                            }
                        });
                        if (errorCol.length > 0) {
                            $(form).parent().find('ul[role="tablist"]:visible').find('a[data-target="#' + tabRequired.attr('id') + '"]').click();
                            setTimeout(function () {
                                findPos($(".tab-content"), objError);
                            }, 200);
                            var elem = $(tb).find('input,textarea,select:visible').not('.listing-footer select, input[type="checkbox"], .insys-filter input,.insys-filter textarea,.insys-filter select,[text-angular] textarea');
                            for (var idx in errorCol) {
                                $(elem).filter('[name=' + errorCol[idx] + ']').each(function (xx, elReq) {
                                    if ($(elReq).val() == "" || $(elReq).val() == null || $(elReq).val() == undefined || $(elReq).val() == 'object:null' || $(elReq).val() == "?" || $(elem).val() == "?" || $(elem).val() == "? number:0 ?") {
                                        errorCol[idx] = $(elReq).attr('c-label') || $(elReq).attr('name')
                                        $(elReq).addClass('required');
                                        $(elReq).on('focus', function () {
                                            $(this).removeClass('required');
                                        })
                                    }
                                })
                            }

                            if (isDialog) {
                                $s.Prompt(errorCol.join(', ') + ' is required on tab ' + header + '.', header, 'warning')
                            } else {
                                $s.SetSystemStatus(errorCol.join(', ') + ' is required on tab ' + header + '.', 'warning');
                            }
                           
                            return false;
                        }
                    });
                }
            } else return true;
            if (countError == 0) return true;
            else return false;
        }
        $s.GetIsRequired = function (columnName, Schema) {
            return !Enumerable.From(Schema || $s.TableSchema).Where(x => x.ColumnName == columnName).Select(x => x.AllowNull).FirstOrDefault();
        }

        $s._CalendarMonths = [
            { ID: 1, Code: 'Jan', Name: 'January' }
            , { ID: 2, Code: 'Feb', Name: 'February' }
            , { ID: 3, Code: 'Mar', Name: 'March' }
            , { ID: 4, Code: 'Apr', Name: 'April' }
            , { ID: 5, Code: 'May', Name: 'May' }
            , { ID: 6, Code: 'Jun', Name: 'June' }
            , { ID: 7, Code: 'Jul', Name: 'July' }
            , { ID: 8, Code: 'Aug', Name: 'August' }
            , { ID: 9, Code: 'Sep', Name: 'September' }
            , { ID: 10, Code: 'Oct', Name: 'October' }
            , { ID: 11, Code: 'Nov', Name: 'November' }
            , { ID: 12, Code: 'Dec', Name: 'December' }
        ];

        $s.getFilingStatusIcon = function (status) {
            switch (status.toString().toLowerCase()) {
                case '1':
                case 'filed':
                    return 'fa-file-alt';
                    break;
                case '2':
                case 'approve':
                case 'approved':
                    return 'fa-check-circle approved-status';
                    break;
                case '3':
                case 'disapprove':
                case 'disapproved':
                    return 'fa-times-circle disapproved-status';
                    break;
                case '4':
                case 'cancel':
                case 'cancelled':
                    return 'fa-ban';
                    break;
                default:
                    return 'fa-file-alt';
                    break;
            }
        }

        $s.FilingStatusList = [
            { ID: 1, Name: 'Filed' },
            { ID: 2, Name: 'Approved' },
            { ID: 3, Name: 'Disapproved' },
            { ID: 4, Name: 'Cancelled' }
        ];

        $s.GoToState = function (stateName, params) {
            $state.go(stateName, params, { reload: true });
        }
        $s.GetAge = function (bday) {
            var bdate = new Date(bday);
            var cDate = new Date();
            var age = cDate.getFullYear() - bdate.getFullYear();
            if (cDate.getMonth() < bdate.getMonth()) age -= 1;
            else if (cDate.getDate() < bdate.getDate()) age -= 1;
            return age;
        }
        $s.GetValueFromArray = function (arr, colName, value) {
            return Enumerable.From(arr).Where(function (x) { return x[colName] == value }).FirstOrDefault();
        }
        $s.RemoveDecimal = function (val) {
            return Math.floor(val);
        }

        
        $s.GenerateDateRange = function (startDate, endDate,limit) { //POWERED BY LEO AGUANTA REMEMBER ME 14012020
            
            var range = moment().range(startDate, endDate);
            if (range.diff('days') > limit) {
                return false;
            }
            return range.toArray('days');


        }

        $s.DisableEnableFields = function (tableSchema, formName, type) { //POWERED BY LEO AGUANTA REMEMBER ME 11032020

            setTimeout(function () {

                var form = $('form[name="' + formName + '"]');

                if (form.length > 0) {
                    var hasAttachment = 0;
                    form.find('input,textarea,button,select:visible').not('.listing-footer select, input[type="checkbox"], .insys-filter input,.insys-filter textarea,.insys-filter select,[text-angular] textarea').each(function (idx, elem) {

                        var elemName = $(elem).attr('name') ? $(elem).attr('name') : '';
                        var elemTitle = $(elem).attr('title') ? $(elem).attr('title') : '';
                        var elemValue = $(elem).attr('value') ? $(elem).attr('value') : '';
                        var column = Enumerable.From(tableSchema).Where(function (x) { return x != undefined && x.ColumnName.toLowerCase() == elemName.toLowerCase()}).FirstOrDefault();
                        if (column != undefined && column != null || (elemTitle == 'Upload' || elemTitle == 'Clear' || elemTitle == 'Download')) {

                           
                            if (type) {

                                elem.disabled = true;
                                $(elem).attr('readonly', true);

                            } else {

                                if (column != undefined && column != null)
                                    hasAttachment = column.ColumnName == 'Attachment' ? (elemValue != '' ? (hasAttachment + 1) : (hasAttachment + 0)) : (hasAttachment + 0);


                                if (hasAttachment > 0) {
                                    elem.disabled = false;
                                    $(elem).attr('readonly', false);
                                } else {

                                    if (elemTitle == 'Upload' || (column != undefined && column != null)) {
                                        elem.disabled = false;
                                        $(elem).attr('readonly', false);
                                    }
                                }
                                
                              
                            }
                        }

                      
                    });
                }
            }, 500)
           
        };

        //$s.CheckFormDirty = function (formName) {
        //    $s.$watch($s[formName], function (nv, ov) {
        //        console.log(nv, ov)
        //    }, true)
        //    $(document.body).on("keydown", this, function (event) {
        //        if (event.keyCode == 116) {
        //            event.preventDefault();

        //            $s.Confirm('Are you sure you want to proceed?').then(function (ret) {
        //                window.location.reload();
        //            });

        //            //if (confirm('You have unsaved changes on this page. Do you want to leave and discard your changes or stay on this page?')) {

        //            //} else {
        //            //    event.preventDefault();
        //            //}
        //        }
        //    });
        //}
        //bba brian
        $s.addRow = function (datacontainer, tableschema) {
            datacontainer.push($s.PlotDefault({}, tableschema, 0));
            $s.SetupInputField(tableschema);
        }
        $s.deleteCheckedRow = function (params) {
            var rows = params.Schema.filter($s.checkedRows);
            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete(' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record.').then(function () {
                    for (var x = 0; x < rows.length; x++) {
                        var Index = params.Schema.indexOf(rows[x]);
                        params.Schema.splice(Index, 1);
                    }
                    $s.CheckAll[params.SchemaName] = 0;
                    
                });
            }
        } //ng click in delete rows button
        $s.checkedRows = function (rows) {
            return rows.IsCheck ? rows.IsCheck == true : rows.IsChecked == true;
        }
        $s.fCheckAll = function (data, checkAllModel) {
            checkAllModel = !checkAllModel;
            Enumerable.From(data).ForEach(function (d) {
                d.IsCheck ? d.IsCheck : d.IsChecked = checkAllModel;
            });
        } // CheckAll.schema : ng-model ng check all checkbox. use IsCheck or IsChecked n-model of checkboxes
        //bba
        
        $s.SetMaxLengthForDecimal = function (TableSchema, FixedLength) {
            //Applicable only for varchar
            if (TableSchema != undefined) {
                //ensure datatype is varchar
                var _TableSchema = TableSchema.filter(x => (x.DataType == "decimal"));
                var x = 0;
                var DecimalLength = 0;
                var input_name = "";
                if (_TableSchema.length > 0) {
                    while (x < _TableSchema.length) {
                        input_name = "input[name=" + _TableSchema[x].ColumnName + "]";
                        DecimalLength = (FixedLength > 0 ? FixedLength : _TableSchema[x].Length);
                        $(input_name).prop("maxlength", DecimalLength);
                        x++;
                    }
                }
            }
        }

        $s.SetBlurActionForCurrency = function (TableSchema, TableData) {
            //Get all decimal type data.
            var _TableSchema = TableSchema.filter(x => (x.DataType == 'decimal'));
            var x = 0;

            while (x < _TableSchema.length) {
                var PropName = _TableSchema[x].ColumnName;
                if (TableData[PropName] != undefined && TableData[PropName] != null) {
                    //For Blur
                    $(document).on('blur', "input[name=" + PropName + "]", function () {
                        var _PropName = this.name;
                        var AmountValue = TableData[_PropName].toString();
                        if (AmountValue.search(".") > 0 && AmountValue.length < 12) {
                            TableData[_PropName] = $s.FormatCurrency(AmountValue);
                        } else {
                            AmountValue = AmountValue.length > 8 ? AmountValue.substring(0, 8) : AmountValue;
                            TableData[_PropName] = $s.FormatCurrency(AmountValue);
                        }
                    });

                    $(document).on('mousedown', "input[name=" + PropName + "]", function () {
                        var _PropName = this.name;
                        var AmountValue = TableData[_PropName].toString();
                        AmountValue = AmountValue.replace(new RegExp(",", "g"), '');
                        if (AmountValue > 0) {
                            TableData[_PropName] = Math.round(AmountValue);
                        }

                    });
                }

                x++;
            }
        }

        $s.SetDisableForm = function (e) {
            var formName = $("form :input");
            formName.prop("disabled", e);
        }

        $s.GetYesNoValue = function (e) {
            if (e != undefined)
                return e == true ? "Yes" : "No";
            else return "";
        }

        $s.GetTrueFalseValue = function (e) {
            if (e != undefined)
                return e == true ? "True" : "False";
            else return "";
        }

        $s.GetTimeDifference = function (TimeIn, TimeOut) {
            if ((TimeIn != '' && TimeOut != '') && (TimeIn != undefined && TimeOut != undefined)) {
                var _timein = new Date(moment(new Date()).format('YYYY-MM-DD') + ' ' + TimeIn);
                var _timeout = new Date(moment(new Date()).format('YYYY-MM-DD') + ' ' + TimeOut);

                if (_timeout < _timein) {
                    _timeout = _timeout.setDate(_timeout.getDate() + 1);
                }
                return $s.FormatNumber((Math.abs(_timein - _timeout) / 36e5), 3);// minus 1 for mid break
            }
        }

        $s.GetStandardLookUp = function (Lookupname) {
            //param should be in the following format
            //param = { LookUpName: "Employee", Controller: "ControllerName", MenuCode: "MenuCodeName"
            var _method = 'LoadLookup';
            var _controller = 'Common';
            return {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Name' }
                    ]
                },
                Filters: [
                    { Name: 'Name', Type: 9, Label: 'Name' }
                ],
                method: _method,
                controller: _controller,
                parameter: { LookupName: Lookupname }
            };
        };


        $s.GetBasicLookUp = function (Lookupname, LookupObject) {
            
        };

        $s.IsFirstDateGreaterThan = function (fDate, sDate) {
            var _fDate = moment(new Date(fDate)).format("MM/DD/YYYY hh:mm");
            var _sDate = moment(new Date(sDate)).format("MM/DD/YYYY hh:mm");

            return (_fDate > _sDate);
        }

        /////Controller JS end
        $s.GetApprovalHistory = function (MenuCode, ID_Reference) {
        

        }

    }])
    .controller('MenuController', ['$scope', '$rootScope', function ($s, $rs) {

        $s.tree = [];
        $s.bareTree = [];

        $s.InitMenu = function () {
            return $s.Request('LoadMenu', {}, 'Menu').then(function (ret) {
                if (ret.Type == 2) {
                    $rs.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.bareTree = ret.Data;
                    var parent = Enumerable.From(ret.Data).Where(function (x) { return (x.ID_Menus == null || x.ID_Menus == 0) }).ToArray();
                    Enumerable.From(parent).ForEach(function (x) {
                        x.Children = GetChildren(x.ID, ret.Data);
                    });
                    $s.tree = parent;
                }
            });
        }

        function GetChildren(pID, data) {
            var child = Enumerable.From(data).Where(function (x) { return x.ID_Menus == pID && x.ID_MenusType != 3 }).ToArray();
            Enumerable.From(child).ForEach(function (x) {
                x.Children = GetChildren(x.ID, data);
            });
            return child;
        }
    }])
    .controller('insysTableController', ['$scope', '$controller', '$state', '$timeout', '$q', function ($s, $c, $st, $t, $q) {
        $c('BaseController', { $scope: $s });
        var defaults = {
            Data: {
                Rows: [],
                Total: 0
            }, 
            Columns: [
                { Name: 'ID', Label: 'Refrence ID', IsFreeze: true }, //properties Name,Label,IsFreeze,IsReadOnly,ControlType,cascade,Options,DisplayContainer
                { Name: 'Name', Label: null },
            ],
            Filters: [], 
            FilterVisible: false,
            HasNew: false,
            HasDelete: false,
            HasEdit: false,
            SortName: 'ID',
            SortDirection: 'DESC',
            CountPerPage: [20, 60, 100],
            Pages: [],
            SelectedCount: 20,
            SelectedPage: 1,
            openForm: null,
            PinnedColumns: [],
            ShowTable: false,
            ShowPinnedHeader: false,
            isLookup: false,
            newForm: null,
            deleteRow: null,
            isEditable: false,
            hasOpenIcon: false,
            hasOpenIconClick: null,
            EnabledLoadButton: false,
            TableSchema: [],
            HasMultiSelect: false,
            Buttons: [], //Label, Type - 1 = Grid, Action(), Icon, Show - true|false
            SelectedRows: [],
            EnableApproveAll: false,
            EnableDisApproveAll: false
        }
        var param = {};
        var extraParam = {};
        $s.tblOptions = {},
            $s.action = null,
            $s.controller = null,
            $s.SelectAllRows = false,
            $s.ShowPinnedHeader = false;

        $s.CheckAll = function () {
            $s.SelectAllRows = !$s.SelectAllRows;
            Enumerable.From($s.tblOptions.Data.Rows).ForEach(function (row) {
                row.IsChecked = $s.SelectAllRows;
            });
        }

        $s.CheckSelectedRow = function (row) {
            row.IsChecked = !row.IsChecked;
        }
        $s.FormatTable = function (tblElem) {
            function fixSizes() {
                var rbGridSize = $(tblElem).width(),
                    rbGridColContainerSize = $(tblElem).find('.rbGrid-column-container').width(),
                    difference = rbGridSize - rbGridColContainerSize;
                if (difference > 0) {
                    //console.log($s.tblOptions);
                    var totalCols = $s.tblOptions.Columns.length + $s.tblOptions.PinnedColumns.length,
                        columnsAdditionalWidth = (difference / totalCols);
                    $(tblElem).find(".rbGrid-pinned-container").children().each(function (idx, col) {
                        if (!$(col).hasClass('selectBox'))
                            $(col).width($(col).width() + columnsAdditionalWidth);
                    });
                    $(tblElem).find(".rbGrid-column-container").children().each(function (idx, col) {
                        if (!$(col).hasClass('selectBox'))
                            $(col).width($(col).width() + columnsAdditionalWidth);
                    });
                }
            }

            var lastScrollLeft = 0;
            var lastScrollTop = 0;
            tblElem.on('scroll', function (ev) {
                var headers = tblElem.find('.rbGrid-column-label');
                var sticky = headers[0].offsetTop;

                var offsetTop = tblElem.scrollTop();
                if (offsetTop > sticky) {
                    $s.tblOptions.ShowPinnedHeader = true;
                    $s.$apply();
                } else {
                    $s.tblOptions.ShowPinnedHeader = false;
                    $s.$apply();
                }
                if (tblElem.find('.rbGrid-sticky-container').length > 0) {
                    tblElem.find('.rbGrid-sticky-container').css('marginTop', tblElem.scrollTop());
                }
                if (tblElem.find('.rbGrid-pinned-container').length > 0) {
                    tblElem.find('.rbGrid-pinned-container')
                        .css('marginLeft', tblElem.scrollLeft())
                        .scrollTop(tblElem.scrollTop());
                    tblElem.find('.rbGrid-sticky-pinned').css('marginLeft', tblElem.scrollLeft());
                }

                var tblElemScrollLeft = tblElem.scrollLeft();
                if (lastScrollLeft != tblElemScrollLeft) {
                    if (!tblElem.find('.rbGrid-pinned-container').hasClass('pinned-scroll'))
                        tblElem.find('.rbGrid-pinned-container').addClass('pinned-scroll');
                    lastScrollLeft = tblElemScrollLeft;
                    if (lastScrollLeft == 0)
                        tblElem.find('.rbGrid-pinned-container').removeClass('pinned-scroll');

                }
                var tblElemScrollTop = tblElem.scrollTop();
                if (lastScrollTop != tblElemScrollTop && lastScrollLeft > 0) {
                    if (!tblElem.find('.rbGrid-sticky-pinned').hasClass('pinned-scroll')) {
                        tblElem.find('.rbGrid-sticky-pinned').addClass('pinned-scroll');
                    }
                    lastScrollTop = tblElemScrollTop;
                } else if (lastScrollTop == tblElemScrollTop && lastScrollLeft > 0) {
                    if (!tblElem.find('.rbGrid-sticky-pinned').hasClass('pinned-scroll')) {
                        tblElem.find('.rbGrid-sticky-pinned').addClass('pinned-scroll');
                    }
                } else if (lastScrollLeft == 0) {
                    tblElem.find('.rbGrid-sticky-pinned').removeClass('pinned-scroll');
                }
            });
            if ($s.tblOptions.isLookup) {
                var resCnt = 0;
                new ResizeSensor(jQuery('#lookup-handle'), function () {
                    fixSizes();
                });
            } else
                fixSizes();
        }

        $s.LoadEmployees = function () {
            return tblOptions.Data.Rows.filter(x => { return x.IsChecked == true });
        }

        $s.current = {
            options: {}, action: null, controller: null, xParams: {}
        };

        $s.RefreshLoadTable = function () {
            return $s.LoadTable($s.current.options, $s.current.action, $s.current.controller, $s.current.xParams);
        };

        $s.LoadTable = function (options, action, controller, xParams) {
            $s.tblOptions.SelectedPage = 1;
            $s.current = {
                options: options, action: action, controller: controller, xParams: xParams
            };
            Enumerable.From(options.Columns).Where(function (x) { return x.IsFreeze == true }).ForEach(function (x) {
                defaults.PinnedColumns.push(x);
            });
            for (var pinned in defaults.PinnedColumns) {
                if (pinned < 2)
                    options.Columns.splice(options.Columns.indexOf(defaults.PinnedColumns[pinned]), 1);
                else
                    defaults.PinnedColumns.splice(pinned, 1);
            }
            $s.tblOptions = $.extend(defaults, options),
                $s.action = action,
                $s.controller = controller;
            extraParam = xParams;
            param = {
                data: {
                    SortName: $s.tblOptions.SortName,
                    SortDirection: $s.tblOptions.SortDirection,
                    Page: $s.tblOptions.SelectedPage,
                    Count: $s.tblOptions.SelectedCount,
                    Filters: $s.tblOptions.Filters
                }
            };
            param = $.extend(param, extraParam);
            //console.log(action, param, controller);
            return $s.Request(action, param || {}, controller).then(function (ret) {
                if (ret.Type != 2) {
                    $s.tblOptions.Data = ret.Data;
                    $s.BuildPage();
                    $s.tblOptions.ShowTable = true;
                    $s.SetupDropdownSources();
                    $t(function () {
                        $(".rbGrid:visible").each(function (idx, elem) {
                            $s.FormatTable($(elem));
                        });
                    }, 100);
                } else {
                    
                }
                //console.log($s.tblOptions);
                return ret;
            })
        }

        $s.RefreshTable = function () {
            $s.SelectAllRows = false;
            param = {
                data: {
                    SortName: $s.tblOptions.SortName,
                    SortDirection: $s.tblOptions.SortDirection,
                    Page: $s.tblOptions.SelectedPage,
                    Count: $s.tblOptions.SelectedCount,
                    Filters: $s.tblOptions.Filters
                }
            };
            param = $.extend(param, extraParam);
            return $s.Request($s.action, param || {}, $s.controller).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.tblOptions.Data = ret.Data;
                }
            })
        }

        $s.BuildPage = function () {
            $s.tblOptions.Pages = [];
            var cnt = parseInt($s.tblOptions.Data.Total / $s.tblOptions.SelectedCount);
            cnt = cnt + (($s.tblOptions.Data.Total % $s.tblOptions.SelectedCount > 0 ? 1 : 0));
            cnt = cnt == 0 ? 1 : cnt;
            for (var x = 0; x < cnt; x++) {
                $s.tblOptions.Pages.push(x + 1);
            }
        }

        $s.ChangeCount = function () {
            $s.SetSystemStatus('Loading ' + $s.tblOptions.SelectedCount + ' rows.', 'loading');
            $s.tblOptions.SelectedPage = 1;
            $s.RefreshTable().then(function () {
                $s.BuildPage();
                $s.SetSystemStatus('Ready');
                $s.$apply();
            })
        }

        $s.ChangePage = function () {
            $s.SetSystemStatus('Loading page ' + $s.tblOptions.SelectedPage + '.', 'loading');
            $s.RefreshTable().then(function () {
                $s.SetSystemStatus('Ready');
                $s.$apply();
            })
        }

        $s.newForm = function () {
            //console.log($s.tblOptions);
            if ($s.tblOptions.newForm != null || $s.tblOptions.newForm != undefined) {
                $s.tblOptions.newForm();
            } else
                if ($st.current.name.indexOf('.Form') > -1) {
                    $st.go($st.current.name, { ID: 0 }, { reload: true });
                } else {
                    $st.go($st.current.name + '.Form', { ID: 0 }, { reload: true });
                }
        }

        $s.deleteRow = function (method, controller, xparam) {
            if ($s.tblOptions.deleteRow != null)
                $s.tblOptions.deleteRow();
            else {
                var rows = $s.getSelectedRows();
                var param = { Data: rows };
                param = $.extend(param, xparam);
                if (rows.length > 0) {
                    $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                        $s.Request(method, param, controller).then(function (ret) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                                $s.$apply();
                            } else {
                                $s.Prompt(ret.Message, 'Delete Record');
                                $st.reload();
                            }
                        });
                    });
                } else {
                    $s.SetSystemStatus('No record selected.', 'warning');
                }
            }
        }

        $s.deleteDetailRow = function (method, controller, tblOptions, xparam) {
            tblOptions = tblOptions == undefined ? $s.tblOptions : tblOptions;
            if (!tblOptions.HasDelete) {
                $s.Prompt('You are not allowed to delete record(s).');
                return;
            }
            var rows = $s.getSelectedRows(tblOptions);
            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    var rowsNoID = Enumerable.From(rows).Where(function (x) { return x == null || x == 0 }).ToArray();
                    if (rowsNoID.length > 0)
                        for (var x in rowsNoID) {
                            tblOptions.Data.Rows.splice(rows.indexOf(rowsNoID[x]), 1);
                            rows.splice(rows.indexOf(rowsNoID[x]), 1);
                        }
                
                    var param = { Data: rows };
                    param = $.extend(param, xparam);
                    if (rows.length > 0)
                        $s.Request(method, param, controller).then(function (ret) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                                $s.$apply();
                            } else {
                                $s.Prompt(ret.Message, 'Delete Record');
                                $st.reload();
                            }
                        });
                });
            } else {
                $s.SetSystemStatus('No record selected.', 'warning');
            }
        }

        $s.openForm = function (row) {
            if ($s.tblOptions.openForm != null || $s.tblOptions.openForm != undefined) $s.tblOptions.openForm(row);
            else $st.go($st.current.name + '.Form', { ID: row.ID }, { reload: true });
        }

        $s.hasOpenIconClick = function (row) {
            if ($s.tblOptions.hasOpenIconClick != null || $s.tblOptions.hasOpenIconClick != undefined) $s.tblOptions.hasOpenIconClick(row);
            else $s.openForm(row);
        }

        //This will be called on ng-change
       

        $s.FormatVal = function (d, f) {
            if (d.toString().indexOf('.CDITI.') > -1) {
                return d.toString().split('.CDITI.')[0];
            }
            var date_formats = [moment.ISO_8601]
            var isDate = moment(d, date_formats, true).isValid();
            
            if (isDate) {
                d = moment(d).format(f);
            }
            return d || '-';
        }

        $s.Override_Return = null;
        $s.return = function () {
            if ($s.Override_Return != null || $s.Override_Return != undefined) $s.Override_Return();
            else $st.go($st.$current.parent.name, { ID: '' }, { reload: true });
        }
        
        $s.getSelectedRows = function (tblOptions) {
            if (tblOptions == undefined || tblOptions == null) {
                return Enumerable.From($s.tblOptions.Data.Rows).Where(function (x) { return x.IsChecked == true }).Select(function (x) { return x.ID || x.Id }).ToArray();
            }
            else {
                
                return Enumerable.From(tblOptions.Data.Rows).Where(function (x) { return x.IsChecked == true }).Select(function (x) { return x.ID || x.Id }).ToArray();
            }
        }
        $s.getDetailedSelectedRows = function (tblOptions) {
            if (tblOptions == undefined || tblOptions == null) {
                return Enumerable.From($s.tblOptions.Data.Rows).Where(function (x) { return x.IsChecked == true }).ToArray();
            }
            else {
                return Enumerable.From(tblOptions.Data.Rows).Where(function (x) { return x.IsChecked == true }).ToArray();
            }
        }

        $s.prev = function () {
            if ($s.tblOptions.SelectedPage > 1) {
                $s.tblOptions.SelectedPage--;
                $s.ChangePage();
            }
        }
        $s.next = function () {
            if ($s.tblOptions.SelectedPage < $s.tblOptions.Pages.length) {
                $s.tblOptions.SelectedPage++;
                $s.ChangePage();
            }
        }

        $s.ShowFilter = function () {
            $s.tblOptions.FilterVisible = !$s.tblOptions.FilterVisible;
        }

        $(document).mousedown(function (e) {
            var container = $("#insysDatePicker");
            var filterContainer = $(".insys-filter");
            
            if (!container.is(e.target) && container.has(e.target).length === 0) {
                $("#insysDatePicker").remove();
            }

            if (!filterContainer.is(e.target) && filterContainer.has(e.target).length === 0 && $s.tblOptions.FilterVisible) {
                $s.tblOptions.FilterVisible = !$s.tblOptions.FilterVisible;
            }

        });

        $s.IsBoolean = function (v) {
            return typeof v === "boolean";
        }
        $s.setSortIcon = function (colName) {
            if ($s.tblOptions.SortName == colName)
                if ($s.tblOptions.SortDirection.toLowerCase() == 'asc')
                    return 'fa-sort-up';
                else
                    return 'fa-sort-down';
            else
                return 'fa-sort';
        }
        $s.sortData = function (colName, label) {
            $s.tblOptions.SortName = colName;
            if ($s.tblOptions.SortDirection.toLowerCase() == 'asc') {
                $s.tblOptions.SortDirection = 'DESC';
                $s.SetSystemStatus('Sorting ' + (label || colName) + ' descending.', 'loading');
            }
            else {
                $s.tblOptions.SortDirection = 'ASC';
                $s.SetSystemStatus('Sorting ' + (label || colName) + ' ascending.', 'loading');
            }

            $s.RefreshLoadTable().then(function (ret) {
                $s.SetSystemStatus('Ready');
            });
        }
        $s.GetArrayValue = function (arr, colName, colValue) {
            return Enumerable.From(arr).Where(function () { return x[colName] == colValue }).FirstOrDefault();
        }
        $s.SetupDropdownSources = function () {
            if (!$s.tblOptions.isEditable) return;
            if ($s.DropdownSources == undefined)
                $s.DropdownSources = [];
            $s.GetDropdownSource = function (name) {
                var dd = Enumerable.From($s.DropdownSources).Where(x => x.Name == name).ToArray();
                if (dd.length > 0) return dd[0].Data;
                else return [];
            }
            if ($s.DropdownSources.length == 0) {
                var dropdowns = Enumerable.From($s.tblOptions.Columns).Where(x => $s.IsNull(x.ControlType, "").toLowerCase() == "select").ToArray();
                angular.forEach(dropdowns, function (dd) {
                    var source = { Name: dd.Name, Source: $s.Request(dd.Options.method, dd.Options.parameter, dd.Options.controller), Data: [] };
                    $s.DropdownSources.push(source);
                });
                if ($s.DropdownSources.length > 0)
                    $q.all(Enumerable.From($s.DropdownSources).Select(x => x.Source).ToArray()).then(function (results) {
                        angular.forEach(results, function (ret, idx) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                            } else {
                                $s.DropdownSources[idx].Data = ret.Data.Rows;
                            }
                        });
                    });
            }
        }
    }])
    .controller('insysTreeController', ['$scope', function ($s) {
        $s.RecursiveTree = function (rawData, parentKey, childKey, pID, xParentKey, xChildKey, xpID) {
            if (pID == undefined) pID = null;
            var tree = [];
            if (xParentKey != undefined && xChildKey != undefined) {
                Enumerable.From(rawData).Where(x => x[parentKey] == pID && x[xParentKey] == xpID).ForEach(function (t) {
                    var cnt = Enumerable.From(rawData).Count(x => x[parentKey] == t[childKey] && x[xParentKey] == t[xChildKey]);
                    if (cnt > 0) {
                        t.Children = $s.RecursiveTree(rawData, parentKey, childKey, t[childKey], xParentKey, xChildKey, t[xChildKey]);
                    } else {
                        t.Children = [];
                    }
                    tree.push(t);
                });
            } else {
                Enumerable.From(rawData).Where(x => x[parentKey] == pID).ForEach(function (t) {
                    var cnt = Enumerable.From(rawData).Count(x => x[parentKey] == t[childKey]);
                    if (cnt > 0) {
                        t.Children = $s.RecursiveTree(rawData, parentKey, childKey, t[childKey]);
                    } else {
                        t.Children = [];
                    }
                    tree.push(t);
                });
            }
            return tree;
        }
    }])
    .controller('insysDatePickerController', ['$scope', '$element', '$attrs', '$compile', function ($s, $e, $a, $compile) {
        $s.data = {};
        $s.data.currentMonth = null;
        $s.data.currentYear = null;
        $s.data.currentDate = null;
        $s.data.selectedValue = null;

        $s.today = new Date();
        $s.data.currentMonth = $s.today.getMonth();
        $s.data.currentYear = $s.today.getFullYear();
        $s.data.currentDate = $s.today.getDate();

        $s.firstDay = (new Date($s.data.currentYear, $s.data.currentMonth)).getDay();

        $s.months = [
            { ID: 0, Name: "January" },
            { ID: 1, Name: "February" },
            { ID: 2, Name: "March" },
            { ID: 3, Name: "April" },
            { ID: 4, Name: "May" },
            { ID: 5, Name: "June" },
            { ID: 6, Name: "July" },
            { ID: 7, Name: "August" },
            { ID: 8, Name: "September" },
            { ID: 9, Name: "October" },
            { ID: 10, Name: "November" },
            { ID: 11, Name: "December" },
        ];
        $s.years = [];

        $s.generateYears = function (start, end) {
            $s.years = [];
            for (var x = start; x < end; x++) {
                $s.years.push(x);
            }
        }

        $s.next = function () {
            $s.data.currentYear = ($s.data.currentMonth === 11) ? $s.data.currentYear + 1 : $s.data.currentYear;
            $s.data.currentMonth = ($s.data.currentMonth + 1) % 12;
            $s.updateFirstDay();
            $s.updateCalendar($s.data.currentMonth, $s.data.currentDate, $s.data.currentYear);
        }

        $s.previous = function () {
            $s.data.currentYear = ($s.data.currentMonth === 0) ? $s.data.currentYear - 1 : $s.data.currentYear;
            $s.data.currentMonth = ($s.data.currentMonth === 0) ? 11 : $s.data.currentMonth - 1;
            $s.updateFirstDay();
            $s.updateCalendar($s.data.currentMonth, $s.data.currentDate, $s.data.currentYear);
        }

        $s.jump = function () {
            $s.updateFirstDay();
            $s.updateCalendar($s.data.currentMonth, $s.data.currentDate, $s.data.currentYear);
        }

        $s.goToday = function () {
            $s.updateCalendar($s.today.getMonth(), $s.today.getDate(), $s.today.getFullYear());
            $s.updateFirstDay();
        }
        $s.getMinValue = function () {
            return $s.data.minValue;
        }
        $s.updateCalendar = function (month, currDate, year) {
            if ($s.IsDisabled(currDate)) {
                return;
            }
            $s.data.currentMonth = month;
            $s.data.currentYear = year;
            $s.data.currentDate = currDate;
            $s.data.selectedValue = new Date(year, month, [currDate]);
        }
        $s.updateFirstDay = function () {
            $s.firstDay = (new Date($s.data.currentYear, $s.data.currentMonth)).getDay();
        }
        $s.generateTemplate = function () {
            return '<div id="insysDatePicker"><div class="idp-header"><div class="idp-select"> <select name="insysDatePicker-month" ng-model="data.currentMonth" id="insysDatePicker-month" ng-change="jump()" ng-options="item.ID as item.Name for item in months"></select> <select name="insysDatePicker-year" ng-model="data.currentYear" id="insysDatePicker-year" ng-change="jump()" ng-options="item for item in years"></select></div><div class="idp-btn"> <i id="insysDatePicker-previous" class="fa fa-chevron-left" ng-click="previous()" title="Previous"></i> <i id="insysDatePicker-next" class="fa fa-chevron-right" ng-click="next()" title="Next"></i> <i id="insysDatePicker-today" class="fa fa-calendar" ng-click="goToday()" title="Current Day"></i></div></div><div class="idp-body"><table><thead><tr><th>Sun</th><th>Mon</th><th>Tue</th><th>Wed</th><th>Thu</th><th>Fri</th><th>Sat</th></tr></thead><tbody id="insysDatePicker-body"><tr><td ng-repeat="n in [].constructor(firstDay) track by $index"></td><td ng-class="{\'bg-info\': IsDateActive($index + 1), \'disabled\': IsDisabled($index + 1)}" ng-dblclick="CloseDatePicker()" ng-mouseup="getCalendarValue($index + 1)" ng-repeat="n in [].constructor(7 - firstDay) track by $index" ng-bind="$index + 1"></td></tr><tr><td ng-class="{\'bg-info\': IsDateActive($index + (8-firstDay)), \'disabled\': IsDisabled($index + (8-firstDay))}" ng-dblclick="CloseDatePicker()" ng-mouseup="getCalendarValue($index + (8-firstDay))" ng-repeat="n in [].constructor(7) track by $index" ng-bind="$index + (8-firstDay)"></td></tr><tr><td ng-class="{\'bg-info\': IsDateActive($index + (8-firstDay) + 7), \'disabled\': IsDisabled($index + (8-firstDay) + 7)}" ng-dblclick="CloseDatePicker();" ng-mouseup="getCalendarValue($index + (8-firstDay) + 7)" ng-repeat="n in [].constructor(7) track by $index" ng-bind="$index + (8-firstDay) + 7"></td></tr><tr><td ng-class="{\'bg-info\': IsDateActive($index + (8-firstDay) + 14), \'disabled\': IsDisabled($index + (8-firstDay) + 14)}" ng-dblclick="CloseDatePicker();" ng-mouseup="getCalendarValue($index + (8-firstDay) + 14)" ng-repeat="n in [].constructor(7) track by $index" ng-bind="$index + (8-firstDay) + 14"></td></tr><tr><td ng-class="{\'bg-info\': IsDateActive($index + (8-firstDay) + 21), \'disabled\': IsDisabled($index + (8-firstDay) + 21)}" ng-if="($index + (8-firstDay) + 20) < daysInMonth(data.currentMonth, data.currentYear)" ng-dblclick="CloseDatePicker();" ng-mouseup="getCalendarValue($index + (8-firstDay) + 21)" ng-repeat="n in [].constructor(7) track by $index" ng-bind="$index + (8-firstDay) + 21"></td></tr><tr><td ng-class="{\'bg-info\': IsDateActive($index + (8-firstDay) + 28), \'disabled\': IsDisabled($index + (8-firstDay) + 28)}" ng-if="($index + (8-firstDay) + 27) < daysInMonth(data.currentMonth, data.currentYear)" ng-dblclick="CloseDatePicker();" ng-mouseup="getCalendarValue($index + (8-firstDay) + 28)" ng-repeat="n in [].constructor(7) track by $index" ng-bind="$index + (8-firstDay) + 28"></td></tr></tbody></table></div></div>';
        }
        $s.daysInMonth = function (iMonth, iYear) {
            return 32 - new Date(iYear, iMonth, 32).getDate();
        }
        $s.generateYears(1980, $s.today.getFullYear() + 11);

        $s.getCalendarValue = function (date) {
            if ($s.IsDisabled(date)) return;
            $s.updateCalendar($s.data.currentMonth, date, $s.data.currentYear);
        }

        $s.IsDisabled = function (date) {
            var ret = false;
            var ret2 = false;
            var current = new Date(new Date($s.data.currentYear, $s.data.currentMonth, [date]).setHours(0, 0, 0, 0));
            if ($s.ngMin != undefined && $s.ngMin != "" && $s.ngMin != null) {
                var minDate = new Date($s.ngMin);
                if (current < minDate) ret = true;
                else ret = false;
            } else ret = false;

            if ($s.ngMax != undefined && $s.ngMax != "" && $s.ngMax != null) {
                var maxDate = new Date($s.ngMax);
                if (current > maxDate) ret2 = true;
                else ret2 = false;
            } else ret2 = false;
            return ret || ret2;
        }
        $s.IsDateActive = function (date) {
            if ($s.IsDisabled(date)) return false;
            if (date === $s.data.currentDate) return true;
        }

        $(document).mousedown(function (e) {
            var container = $("#insysDatePicker");
            if (!container.is(e.target) && container.has(e.target).length === 0) {
                $("#insysDatePicker").remove();
            }
        });

        $s.CloseDatePicker = function () {
            $("#insysDatePicker").remove();
        }

        $s.SetDateTimeLocation = function () {
            if ($('#insysDatePicker').length > 0) {
                var pckerWidth = $('#insysDatePicker').width();
                var pckerHeight = $('#insysDatePicker').height();
                var targetElem = $($e).parent();

                var left = targetElem.offset().left;
                var top = targetElem.offset().top;
                var totalTop = top + targetElem.height();
                if (left + pckerWidth > $(window).width()) {
                    var t = targetElem.offset().left + targetElem.width();
                    left = t - pckerWidth;
                } else {

                }
                if ((top + pckerHeight) > $(window).height()) { ///ung extrang 5 sa 305 is margin lang
                    var t = targetElem.offset().top;
                    totalTop = (totalTop - ((totalTop + pckerHeight) - t)) - 10;
                } else {
                }
                var elem = $("#insysDatePicker");
                elem.removeAttr('style');
                elem[0].style.top = totalTop + 'px';
                elem[0].style.left = left + 'px';
            }
        }

        $s.ShowDatePicker = function () {
            if ($a.readonly) return;
            if ($a.disabled) return;

            var picker = $compile($s.generateTemplate())($s);
            var pcker = $('body').find('#insysDatePicker');
            if (pcker.length == 0) $('body').append(picker);
            else pcker.remove(); $('body').append(picker);

            if ($s.ngModel.$modelValue != null && $s.ngModel.$modelValue != "") {
                var d = new Date($s.ngModel.$modelValue);
                $s.updateCalendar(d.getMonth(), d.getDate(), d.getFullYear());
            } else {
                $s.updateCalendar($s.today.getMonth(), $s.today.getDate(), $s.today.getFullYear());
            }
            $s.$watch('data.selectedValue', function (nv, ov) {
                $s.ngModel.$setViewValue(moment(new Date($s.data.selectedValue)).format($s.textFormat || 'MM/DD/YYYY'));
                $s.ngModel.$render();
            });
            $s.$apply();
        }

        //$e.on('mouseup', function (e) {
        //    //$s.SetDateTimeLocation();
        //});

        $e.on('mouseup', function (e) {
            $s.ShowDatePicker();
            $s.SetDateTimeLocation();
        });
    }])
    .controller('insysTimePickerController', ['$scope', '$element', '$attrs', '$compile', function ($s, $e, $a, $compile) {
        $s.data = {};
        $s.data.currentHour = null;
        $s.data.currentMinute = null;
        $s.data.currentFormat = null;
        $s.data.selectedValue = null;

        $s.hours = ['12', '01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11'];
        $s.minutes = [];
        $s.formats = ['AM', 'PM'];
        for (var x = 0; x < 60; x++) {
            if (x >= 0 && x <= 9) x = '0' + x.toString();
            $s.minutes.push(x.toString());
        }

        $s.jump = function () {
            if ($s.data.currentHour != null && $s.data.currentMinute != null && $s.data.currentFormat != null)
                $s.updateClock($s.data.currentHour, $s.data.currentMinute, $s.data.currentFormat);
        }

        $s.updateClock = function (hour, minute, format) {
            hour = hour > 12 ? hour - 12 : hour;

            $s.data.currentHour = (parseInt(hour) < 10 ? '0' + parseInt(hour) : hour.toString());
            $s.data.currentMinute = (parseInt(minute) < 10 ? '0' + parseInt(minute) : minute.toString());
            $s.data.currentFormat = format;
            $s.data.selectedValue = (parseInt($s.data.currentHour) < 10 ? '0' + parseInt($s.data.currentHour) : $s.data.currentHour.toString()) + ':' + $s.data.currentMinute + ' ' + $s.data.currentFormat;
        }

        $s.IsDisabledTime = function (hr, minute, timeformat) {
            if ($s.ngMin != undefined && $s.ngMin != null && $s.ngMin != "") {
                var currentValue = new Date();
                var minValue = new Date();
                var minTime = $s.ngMin.replace(" AM", "").replace(" PM", "").split(":");
                var increment = (timeformat.toLowerCase() == "pm" ? (parseInt(hr) == 12 ? 0 : 12) : 0);
                var increment2 = ($s.ngMin.split(" ")[$s.ngMin.split(" ").length - 1].toLowerCase() == "pm" ? (parseInt(minTime[0]) == 12 ? 0 : 12) : 0);

                currentValue.setHours(parseInt(hr) + increment, parseInt(minute));
                minValue.setHours(parseInt(minTime[0]) + increment2, parseInt(minTime[1]));

                if (currentValue < minValue)
                    return true;
                else return false;
            } else return false;
        }
        $(document).mousedown(function (e) {
            var container = $("#insysTimePicker");
            if (!container.is(e.target) && container.has(e.target).length === 0) {
                $("#insysTimePicker").remove();
            }
        });
        $e.on('mouseup', function (e) {
            if ($('#insysTimePicker').length > 0) {
                var pckerWidth = $('#insysTimePicker').width();
                var pckerHeight = $('#insysTimePicker').height();
                var targetElem = $($e);

                var left = targetElem.offset().left;
                var top = targetElem.offset().top;
                var totalTop = top + targetElem.height();
                if (left + pckerWidth > $(window).width()) {
                    var t = targetElem.offset().left + targetElem.width();
                    left = t - pckerWidth;
                } else {

                }
                if ((top + pckerHeight) > $(window).height()) { ///ung extrang 5 sa 305 is margin lang
                    var t = targetElem.offset().top;
                    totalTop = (totalTop - ((totalTop + pckerHeight) - t)) - (targetElem.height() + 5);
                } else {
                    totalTop = totalTop + 10;
                }
                var elem = $("#insysTimePicker");
                elem.removeAttr('style');
                elem[0].style.top = totalTop + 'px';
                elem[0].style.left = left + 'px';
            }
        });
    }])
    .controller('clientNotificationController', ['$scope', 'Notification', '$state', function ($s, $notify, $state) {
        $s.Notifications = [];
        $s.ProcessNotifications = [];
        $s.UnSeenNotification = [];
        $s.SeenNotification = [];
        $s.NotificationOnClickEvent = {};
        $s.hub = null;

        function GetUnSeenNotification() {
            $s.UnSeenNotification = Enumerable.From($s.Notifications).Where(function (x) { return x.IsSeen == false }).ToArray();
        }
        function GetSeenNotification() {
            $s.SeenNotification = Enumerable.From($s.Notifications).Where(function (x) { return x.IsSeen == true }).ToArray();
        }
        function connect() {
            $s.Request('LoadNotification', {}, 'Index', true).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.Notifications = ret.Data;
                    GetUnSeenNotification();
                    GetSeenNotification();
                    $s.$apply();

                    $s.hub.start().then(function () {
                        console.log('hub connected');
                        $s.hub.invoke('Register', $s.SessionID, $s.Session('ID_User')).catch(function (ex) {
                            $s.SetSystemStatus(ex.toString(), 'error');
                        });
                    }).catch(function (ex) {
                        //$s.SetSystemStatus(ex.toString(), 'error');
                        setTimeout(function () {
                            connect();
                        }, 60000);
                    });
                }
            });
        }

        $s.LoadNotification = function () {
            $s.hub = new signalR.HubConnectionBuilder()
                .withUrl($s.Session('WebNotificationServer') + '/webNotificationHub')
                .configureLogging(signalR.LogLevel.Error) //Debug, Trace
                .build();

            $s.hub.on('logout', function () {
                $s.Prompt('Your account has been logged in from another device.').then(function () {
                    $s.Logoff();
                });
            });
            $s.hub.on('newNotification', function (nData) {
                if (nData != null && nData != "" && nData != undefined) {
                    nData = JSON.parse(nData);
                    //console.log(nData)
                    $s.Notifications.unshift(nData);
                    $notify({ message: nData.Content, title: nData.Title, delay: 5000 });
                    GetUnSeenNotification();
                    GetSeenNotification();
                    $s.$apply();
                }
            });
            $s.hub.on('processNotification', function (nData) {
                //console.log(nData)
                if (nData != null && nData != "" && nData != undefined) {
                    nData = JSON.parse(nData);
                    var processIdx = Enumerable.From($s.ProcessNotifications).Select(function (xx) { return xx.ID }).IndexOf(nData.ID);
                    if (processIdx > -1) $s.ProcessNotifications[processIdx] = nData;
                    else $s.ProcessNotifications.push(nData);
                    $s.$apply();
                }
            });
            $s.hub.onclose(function () {
                console.log('reconnecting to hub...');
                connect();
            });
            connect();
        }
        $s.SeeNotification = function (row) {
            if (row.IsSeen) {
                return $s.Task().then(function () {
                    return { Type: 1 };
                });
            }
            row.IsSeen = true;
            return $s.Request('SeeNotification', { ID: row.ID }, 'Index', true).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    return ret;
                } else {
                    GetUnSeenNotification();
                    GetSeenNotification();
                    return ret;
                }
            })
        }
        $s.UpdateNotification = function (row, onClickEvent, linkOnClick) {
            $s.SeeNotification(row).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    if (linkOnClick != null && linkOnClick != "" && linkOnClick != undefined) {
                        if (linkOnClick.indexOf(".Form") > -1) $state.go(linkOnClick, { ID: row.ReferenceID }, { reload: true });
                        else $state.go(linkOnClick, {}, { reload: true });
                    } else
                        switch (row.ID_WebNotificationTypes) {
                            case 1: //Interview
                                if (onClickEvent == undefined) {
                                    $s.Prompt(row.Content, row.Title);
                                    break;
                                }
                                else if (onClickEvent[1] == undefined) {
                                    $s.Prompt(row.Content, row.Title);
                                    break;
                                }
                                onClickEvent[1].Trigger(row);
                                break;
                            case 2: //Exam
                                if (onClickEvent == undefined) {
                                    $s.Prompt(row.Content, row.Title);
                                    break;
                                }
                                else if (onClickEvent[2] == undefined) {
                                    $s.Prompt(row.Content, row.Title);
                                    break;
                                }
                                onClickEvent[2].Trigger(row);
                                break;
                            case 1004: //InitialMigration
                                if (onClickEvent == undefined) {
                                    $s.Prompt(row.Content, row.Title);
                                    break;
                                }
                                else if (onClickEvent[1004] == undefined) {
                                    $s.Prompt(row.Content, row.Title);
                                    break;
                                }
                                onClickEvent[1004].Trigger(row);
                                break;
                            default:
                                $s.Prompt(row.Content, row.Title)
                        }
                }
            })
        }
    }])
    .controller('insysReportController', ['$scope', '$controller', function ($s, $c) {
        $c('BaseController', { $scope: $s });

        $s.LoadReport = function (method, params, controller) {
            return $s.Request(method, params, controller);
        }
        $s.GenerateExcel = function (method, params, controller) {
            $s.Request(method, params, controller).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                    $s.$apply();
                } else {
                    $s.Download(ret.Data, 'Files', ret.Data.replace('.xlsx', '').replace('.xls', ''));
                }
            });
        }
    }])
    .controller('insysForm', ['$scope', function ($s) {
        $s.form = {};
    }])
angular.module('app')
    .provider("DataService", function () {
        var isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
        var isIE = false || !!document.documentMode;
        var isEdge = !isIE && !!window.StyleMedia;

        var _ = this;
        _.Controller = 'Action';
        _.AppName = 'Web';
        _.Encryption = {
            Key: null, Salt: null, IP: null, Epoch: null, UID: null
        }
        _.Dialog = null;

        _.ResultAdapter = function (rdata) {
            var d = {};
            var data = rdata;
            if (typeof data === 'string') {
                var gData = _.Base64Decode(LZString.decompressFromBase64(unescape(data)));
                nData = gData;
                //var nData = NickCrypt.Decrypt(gData, _.Encryption.Key, _.Encryption.Salt);
                data = JSON.parse(nData);
            }
            if (data.status === 1) {
                if (data.errorMsg == "Session Expired.") {
                    _.Dialog(data.errorMsg).then(function () {
                        window.location.reload();
                    })
                } else throw new Error(data.errorMsg);
            };

            if (typeof data.resultSet === 'number' || typeof data.resultSet === 'object') {
                d.ResultSet = data.resultSet;
            } else {
                if (data.resultSet) {
                    var ggh = _.Base64Decode(data.resultSet);
                    var ggg = NickCrypt.Decrypt(ggh, _.Encryption.Key, _.Encryption.Salt);
                    d.ResultSet = JSON.parse(ggg);
                }
            }
            d.Status = data.status;
            d.ErrorMsg = data.errorMsg;
            d.data = rdata;
            if (d.ResultSet.Message == "Session Expired.") {
                _.Dialog(d.ResultSet.Message).then(function () {
                    window.location.reload();
                })
            }
            return d;
        }

        _.EncryptData = function (name, obj) {
            var k = JSON.stringify(obj || {});
            //console.log('chard', name, obj, k);
            k = NickCrypt.Encrypt(k, _.Encryption.Key, _.Encryption.Salt);
            var j = _.GenerateToken();

            return {
                Key: j,
                Data: k
            }
        };

        _.GenerateToken = function () {

            var ticks = new Date().getTime() / 1e3 | 0;
            //console.log('chards', _.Encryption.UID, _.Encryption.IP, ticks, _.Encryption.Key, _.Encryption.Salt);
            var enc1 = [_.Encryption.UID, _.Encryption.IP, ticks].join(':');
            var hash = NickCrypt.Encrypt(enc1, _.Encryption.Key, _.Encryption.Salt);

            return _.Base64Encode(hash);
        }

        _.Base64Decode = function (str, encoding) {
            if (encoding == null || encoding == undefined) { encoding = 'utf-8' }
            var bytes = base64js.toByteArray(str);
            return new (TextDecoder || TextDecoderLite)(encoding).decode(bytes);
        }

        _.Base64Encode = function (str, encoding) {
            if (encoding == null || encoding == undefined) { encoding = 'utf-8' }
            var bytes = new (TextEncoder || TextEncoderLite)(encoding).encode(str);
            return base64js.fromByteArray(bytes);
        }

        return {
            SetDefaultController: function (value) {
                _.Controller = value;
            },
            SetAppName: function (value) {
                _.AppName = value;
            },
            $get: ["$http", '$q', function ($http, $q) {
                return {
                    SetController: function (value) {
                        _.Controller = value;
                    },
                    GetController: function () {
                        return _.Controller;
                    },
                    SetEncryption: function (token) {
                        var ndec = _.Base64Decode(token);
                        var jdec = ndec.split(':');
                        var nKey = LZString.decompressFromUTF16(jdec[2]);
                        var nPass = LZString.decompressFromUTF16(jdec[3]);

                        //test token if valid
                        var nData = NickCrypt.Decrypt(jdec[4], nKey, nPass);

                        if (nData == null)
                            throw new Error('Invalid Token');

                        var gData = nData.split(':');

                        _.Encryption = {
                            Key: nKey,
                            Salt: nPass,
                            IP: gData[0],
                            Epoch: gData[1],
                            UID: jdec[0]
                        }
                    },
                    Post: (function (name, param, CustomController, disableInterceptor) {
                        var deferred = $.Deferred();
                        var vPath = window.VirtualPath || '';
                        var hash = _.EncryptData(name, param);

                        var p = {
                            url: vPath + "/api/" + (CustomController || _.Controller) + "/" + name,
                            method: "POST",
                            data: encodeURI(LZString.compressToBase64(hash.Data)),
                            dataType: "json",
                            disableInterceptor: (disableInterceptor == undefined ? false : disableInterceptor),
                            headers: {
                                "Api-Key": hash.Key
                            }
                        };
                        //console.log(p);
                        $http(p).then(function (d) {
                            //console.log(d);
                            try {
                                var j = _.ResultAdapter(d.data);
                                deferred.resolve(j.ResultSet, j.Status);
                            } catch (ex) {
                                deferred.reject(ex);
                            }
                        }).catch(deferred.reject);
                        //}

                        return deferred.promise();
                    }),
                    ByteLength: function (str) {
                        // returns the byte length of an utf8 string
                        var s = str.length;
                        for (var i = str.length - 1; i >= 0; i--) {
                            var code = str.charCodeAt(i);
                            if (code > 0x7f && code <= 0x7ff) s++;
                            else if (code > 0x7ff && code <= 0xffff) s += 2;
                            if (code >= 0xDC00 && code <= 0xDFFF) i--; //trail surrogate
                        }
                        return s;
                    },
                    UrlApi: (function (name) {
                        var vPath = window.VirtualPath || '';
                        return vPath + "/api/" + _.Controller + "/" + name
                    }),
                    Upload: (function (postUrl, payload) {
                        var deferred = $.Deferred(); //$q.defer();
                        var vPath = window.VirtualPath || '';
                        var hash = _.EncryptData(name, null);
                        $http.post(vPath + "/api/Upload/" + postUrl, payload, {
                            headers: {
                                'Content-Type': undefined,
                                "Api-Key": hash.Key,
                            },
                            transformRequest: function (data) { return data; }
                        })
                            .then(function (d) {
                                try {
                                    var j = _.ResultAdapter(d.data);
                                    deferred.resolve(j.ResultSet, j.Status);
                                } catch (ex) {
                                    deferred.reject(ex);
                                }
                            })
                            .catch(deferred.reject);
                        // }
                        return deferred.promise();
                    }),
                    Download: function (fileName, Container, displayName, contentType) {
                        var _ = this;
                        var param = { folder: Container, fileName: fileName, displayName: displayName };
                        return this.Post('DownloadFile', param, 'Download').then(function (ret) {
                            if (ret.Type == 2) {
                                return ret;
                            } else {
                                return _.DownloadFile(fileName, Container, displayName, contentType);
                            }
                        })
                    },
                    DownloadFile: function (fileName, Container, displayName, contentType) {
                        var vPath = window.VirtualPath || '';
                        var param = { folder: Container, fileName: fileName, displayName: displayName, contentType: contentType };

                        var hash = _.EncryptData('DownloadFile', param);

                        var p = {
                            url: vPath + "/api/Download/FetchFile",
                            method: "POST",
                            data: encodeURI(LZString.compressToBase64(hash.Data)),
                            dataType: "json",
                            headers: {
                                "Api-Key": hash.Key
                            },
                            responseType: 'arraybuffer',
                        };
                        return $http(p).then(function (data) {
                            try {
                                var filename = null;
                                var disposition = data.headers('content-disposition');
                                if (disposition && disposition.indexOf('attachment') !== -1) {
                                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                                    var matches = filenameRegex.exec(disposition);
                                    if (matches != null && matches[1]) {
                                        filename = matches[1].replace(/['"]/g, '');
                                    }
                                }
                                var octetStreamMime = 'application/octet-stream';
                                var success = false;

                                // Determine the content type from the header or default to "application/octet-stream"
                                var contentType = data.headers('content-type') || octetStreamMime;

                                try {
                                    // Try using msSaveBlob if supported
                                    console.log("Trying saveBlob method ...");
                                    var blob = new Blob([data.data], { type: contentType });
                                    if (navigator.msSaveBlob)
                                        navigator.msSaveBlob(blob, filename);
                                    else {
                                        // Try using other saveBlob implementations, if available
                                        var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                                        if (saveBlob === undefined) throw "Not supported";
                                        saveBlob(blob, filename);
                                    }
                                    console.log("saveBlob succeeded");
                                    success = true;
                                } catch (ex) {
                                    console.log("saveBlob method failed with the following exception:");
                                    console.log(ex);
                                }

                                if (!success) {
                                    // Get the blob url creator
                                    var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
                                    if (urlCreator) {
                                        // Try to use a download link
                                        var link = document.createElement('a');
                                        if ('download' in link) {
                                            // Try to simulate a click
                                            try {
                                                // Prepare a blob URL
                                                console.log("Trying download link method with simulated click ...");
                                                var blob = new Blob([data.data], { type: contentType });
                                                var url = urlCreator.createObjectURL(blob);
                                                link.setAttribute('href', url);

                                                // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                                                link.setAttribute("download", filename);

                                                // Simulate clicking the download link
                                                var event = document.createEvent('MouseEvents');
                                                event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                                                link.dispatchEvent(event);
                                                console.log("Download link method with simulated click succeeded");
                                                success = true;

                                            } catch (ex) {
                                                console.log("Download link method with simulated click failed with the following exception:");
                                                console.log(ex);
                                            }
                                        }

                                        if (!success) {
                                            // Fallback to window.location method
                                            try {
                                                // Prepare a blob URL
                                                // Use application/octet-stream when using window.location to force download
                                                console.log("Trying download link method with window.location ...");
                                                var blob = new Blob([data.data], { type: octetStreamMime });
                                                var url = urlCreator.createObjectURL(blob);
                                                window.location = url;
                                                console.log("Download link method with window.location succeeded");
                                                success = true;
                                            } catch (ex) {
                                                console.log("Download link method with window.location failed with the following exception:");
                                                console.log(ex);
                                            }
                                        }

                                    }
                                }

                                if (!success) {
                                    // Fallback to window.open method
                                    console.log("No methods worked for saving the arraybuffer, using last resort window.open");
                                    window.open(httpPath, '_blank', '');
                                }
                            } catch (ex) {
                                console.log(ex);
                            }
                        });
                    },
                    GetMenu: (function (ID_Menu) {
                        return this.Post('GetMenu', { ID_Menu: ID_Menu });
                    }),
                    AppName: _.AppName,
                    SetDialog: function (dlg) {
                        _.Dialog = dlg;
                    }
                };
            }]
        };
    })
    .factory('Session', function ($http, $rootScope) {

        var _ = this;

        _.DataBank = {};
        _.SessionData = angular.module('app').Session || {};

        return {
            UserRow: function (value) {
                if (value) {
                    _.SessionData = value;
                }
                else {
                    return _.SessionData;
                }
            },
            Session: function (name) {
                return _.SessionData[name];
            },
            Length: function () {
                return Object.keys(_.SessionData).length;
            },
            Clear: function () {
                _.SessionData = {};
            },
            Update: function (Name, Value) {
                if (_.SessionData[Name] == undefined) return;
                _.SessionData[Name] = Value;
            }
        }
    })
    .factory('$Invoker', function () {
        return {
            events: {},
            group: function (ID) {
                var _ = this;

                if (_.events[ID] == null)
                    _.events[ID] = []; //register Group ID; 


                return {
                    on: function (Name, Action) {
                        _.on(ID, Name, Action);
                    },
                    invoke: function (Name) {
                        try {
                            var infodata = [];
                            var args = Array.prototype.slice.call(arguments, 1);
                            _.events[ID].forEach(function (d) {
                                if (Name === d.Name)
                                    infodata.push(d.Action.apply(this, args));
                            });
                            return $.when.apply(undefined, infodata).promise().then(function () {
                                return Array.prototype.slice.call(arguments, 0);
                            }).fail(function (x) {
                                console.error(x);
                            })
                        } catch (ex) {
                            console.error(Name, ex);
                            throw new Error("Method " + Name + ' not found');
                        }
                    },
                    all: function (Name) {
                        try {
                            var infodata = [];
                            var args = Array.prototype.slice.call(arguments, 1);
                            var kys = Object.keys(_.events);

                            for (var ii = 0; ii < kys.length; ii++)
                                _.events[kys[ii]].forEach(function (d) {
                                    if (Name === d.Name)
                                        infodata.push(d.Action.apply(this, args));
                                });

                            return $.when.apply(undefined, infodata).promise().then(function () {
                                return Array.prototype.slice.call(arguments, 0);
                            }).fail(function (x) {
                                console.error(x);
                            })
                        } catch (ex) {
                            console.error(Name, ex);
                            throw new Error("Method " + Name + ' not found');
                        }
                    },
                    clear: function () {
                        _.clear(ID);
                    }
                }
            },
            on: function (ID, Name, Action) {
                var _ = this;

                //vcl.Array.Remove(this.events[ID], function (x) { return x.ID === ID && x.Name === Name }); //check constraints

                this.events[ID].push({ Name: Name, Action: Action });
            },
            invoke: function (ID, Name) {
                try {
                    var infodata = [];
                    var args = Array.prototype.slice.call(arguments, 2);

                    this.events[ID].forEach(function (d) {
                        if (Name === d.Name)
                            infodata.push(d.Action.apply(this, args));
                    });
                    return $.when.apply(undefined, infodata).promise().then(function () {
                        return Array.prototype.slice.call(arguments, 0);
                    }).fail(function (x) {
                        console.error(x);
                    })
                } catch (ex) {
                    console.error(Name, ex);
                    throw new Error("Method " + Name + ' not found');
                }
            },
            clear: function (ID) {
                delete this.events[ID] //remove object
                // vcl.Array.Remove(this.events, function (x) { return x.ID === ID });
            }
        }
    })
    .filter('nospace', function () {
        return function (value) {
            return (!value) ? '' : value.replace(/ /g, '');
        };
    })
    .filter('humanizeDoc', function () {
        return function (doc) {
            if (!doc) return;
            if (doc.type === 'directive') {
                return doc.name.replace(/([A-Z])/g, function ($1) {
                    return '-' + $1.toLowerCase();
                });
            }

            return doc.label || doc.name;
        };
    });

var baseUrl = localStorage.getItem("HtmlTemplatePath") + "/Web/",
    wcOverlayDirective = function (q, t, w, httpInterceptor, $c) {
        return {
            restrict: 'EA',
            transclude: true,
            s: {
                wcOverlayDelay: "@"
            },
            templateUrl: baseUrl + 'Directives/overlayLoading.tmpl.html',
            link: function (s, element, A) {
                $c('BaseController', { $scope: s });
                var overlayContainer = null,
                    timerPromise = null,
                    timerPromiseHide = null,
                    inSession = false,
                    ngRepeatFinished = true,
                    queue = [];

                init();

                function init() {
                    wireUpHttpInterceptor();
                    overlayContainer = document.getElementById('overlay-container');
                }
                function wireUpHttpInterceptor() {

                    httpInterceptor.request = function (config) {
                        if (config.disableInterceptor == undefined || config.disableInterceptor == false) processRequest();
                        return config || q.when(config);
                    };

                    httpInterceptor.response = function (response) {
                        processResponse();
                        return response || q.when(response);
                    };

                    httpInterceptor.responseError = function (rejection) {
                        processResponse();
                        s.SetSystemStatus("Error occurred. Contact system administrator.", 'error');
                        return rejection || q.when(rejection);
                    };
                }
                function wirejQueryInterceptor() {

                    $(document).ajaxStart(function () {
                        processRequest();
                    });

                    $(document).ajaxComplete(function () {
                        processResponse();
                    });

                    $(document).ajaxError(function () {
                        processResponse();
                    });

                    var $mylist = $("body");
                    $mylist.livequery('iframe', function (elem) {
                        processRequest();
                        $('iframe').ready(function (e) {
                            processResponse();
                        });
                    });

                }

                function processRequest() {
                    queue.push({});
                    //showOverlay();
                    if (queue.length == 1) {
                        timerPromise = t(function () {
                            if (queue.length) showOverlay();
                        }, s.wcOverlayDelay ? s.wcOverlayDelay : 300); //Delay showing for 300 millis to avoid flicker
                    }
                }
                function processResponse() {
                    queue.pop();
                    if (queue.length == 0) {
                        timerPromiseHide = t(function () {
                            if (queue.length == 0) {
                                hideOverlay();
                                if (timerPromiseHide) t.cancel(timerPromiseHide);
                            }
                        }, s.wcOverlayDelay ? s.wcOverlayDelay : 300);
                    }
                }

                function showOverlay() {
                    var W = 0;
                    var h = 0;
                    if (!w.innerWidth) {
                        if (!(document.documentElement.clientWidth == 0)) {
                            W = document.documentElement.clientWidth;
                            h = document.documentElement.clientHeight;
                        }
                        else {
                            W = document.body.clientWidth;
                            h = document.body.clientHeight;
                        }
                    }
                    else {
                        W = w.innerWidth;
                        h = w.innerHeight;
                    }
                    overlayContainer.style.display = 'block';
                }

                function hideOverlay() {
                    if (timerPromise) t.cancel(timerPromise);
                    overlayContainer.style.display = 'none';
                }

                var getComputedStyle = function () {
                    var func = null;
                    if (document.defaultView && document.defaultView.getComputedStyle) {
                        func = document.defaultView.getComputedStyle;
                    } else if (typeof (document.body.currentStyle) !== "undefined") {
                        func = function (element, anything) {
                            return element["currentStyle"];
                        };
                    }

                    return function (element, style) {
                        return func(element, null)[style];
                    }
                }();
            }
        }
    },
    httpProvider = function ($httpProvider) {
        $httpProvider.interceptors.push('httpInterceptor');
    },
    httpInterceptor = function () {
        return {}
    },
    authHttpResponseInterceptor = function ($q, $location, t, $injector, $rs) {
        return {
            response: function (response) {
                if (response.status === 401) {
                    console.log(response);
                }
                return response || $q.when(response);
            },
            responseError: function (rejection) {
                if (rejection.status === 400) {
                    rejection.data = "Invalid Token.";
                }
                if (rejection.status === 401) {
                    console.log(rejection);
                }
                if (rejection.status === 404) {
                    if (rejection.config.method == 'POST') {
                        var url = rejection.config.url.split("/");
                        rejection.data = "Method " + url[url.length - 1] + " not found.", "error";
                    } else {
                        rejection.data = "Page not found.";
                    }
                } else if (rejection.status === 500) {
                    if (rejection.config.method == 'POST') {
                        var url = rejection.config.url.split("/");
                        rejection.data = "Internal server error on " + url[url.length - 1] + ".";
                    } else {
                        rejection.data = "Internal server error.";
                    }
                }
                return $q.reject(rejection.data);
            }
        }
    }, httpAuthProvider = function ($httpProvider) {
        $httpProvider.interceptors.push('authHttpResponseInterceptor');
    }

angular.module('app')
    .factory('httpInterceptor', httpInterceptor)
    .config(['$httpProvider', httpProvider])
    .directive('wcOverlay', ['$q', '$timeout', '$window', 'httpInterceptor', '$controller', wcOverlayDirective])
    .factory('authHttpResponseInterceptor', ['$q', '$location', '$timeout', '$injector', '$rootScope', authHttpResponseInterceptor])
    .config(['$httpProvider', httpAuthProvider])
    .directive('insysMenu', ['$compile', '$state', '$rootScope', function ($c, $st, $rs) {
        return {
            templateUrl: baseUrl + 'Directives/insysMenu.tmpl.html',
            restrict: 'E',
            replace: true,
            transclude: true,
            scope: {
                treeData: '='
            },
            compile: function (E, A, transclude) {
                var contents = E.contents().remove();
                var compiledContents;

                return function ($s, iE, iA) {
                    $s.$watch('treeData', function (nv, ov) {
                        $s.tree = nv;
                    });

                    $s.openChild = function (m) {
                        m.IsOpen = !m.IsOpen;
                        if (m.Children.length == 0) {
                            $rs.SetSystemStatus('Loading ' + m.Name, 'loading');
                            $s.gotoLink(m.Code.replace(new RegExp(' ', 'g'), '-'));
                        }
                    }

                    $s.checkActiveMenu = function (m) {
                        var mm = m.Code.replace(new RegExp(' ', 'g'), '-');
                        if (mm == $st.current.name.replace('.Form', '')) return true;
                        else return false;
                    }

                    $s.gotoLink = function (stateName) {
                        try {
                            $st.go(stateName, { ID: '' }, { reload: true });
                            //$loc.path(stateName)
                        } catch (ex) {
                            $rs.SetSystemStatus('Page not found.', 'error');
                            $st.go('404');
                        }
                    }

                    if (!compiledContents) {
                        compiledContents = $c(contents, transclude);
                    };

                    compiledContents($s, function (clone, $s) {
                        iE.append(clone);
                    });
                }
            }
        }
    }])
    .directive('formatDate', [function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function ($s, $e, $attr, modelCtrl) {
                modelCtrl.$formatters.push(function (modelValue) {
                    return new Date(modelValue);
                })
            }
        }
    }])
    .directive('includeReplace', [function () {
        return {
            require: 'ngInclude',
            restrict: 'A',
            link: function ($s, $e, $attr) {
                $e.replaceWith($e.children());
            }
        }
    }])
    .directive('insysLookup', ['$controller', '$compile', function ($c, $compile) {
        return {
            restrict: 'A',
            scope: {
                lookupOptions: '=',
                lookupData: '=',
                isRequired: '=?',
                isDisabled: '=?',
                cascade: '&?',
                isGrid: '=?'
            },
            templateUrl: baseUrl + 'Directives/insysLookup.tmpl.html',
            replace: true,
            link: function ($s, $e, $attr) {
                $c('insysTableController', { $scope: $s });
                $s.tblOptions = {};
                //reference for config
                //$s.lookupData = {
                //    model: null,
                //    targetValue: null,
                //    targetDisplay: null,
                //    valueContainer: null,
                //    diplayContainer: null,
                //    label: null
                //};
                $s.$watch('lookupOptions', function (nv, ov) {
                    if (nv != null) {
                        if (nv.tblOptions == undefined) return;
                        $.extend($s.tblOptions, nv.tblOptions);
                        //$s.tblOptions = nv.tblOptions;
                        $s.tblOptions.isLookup = true;
                    }
                }, true);

                $s.$watch('lookupData', function (nv, ov) {
                    if (nv.model[$s.lookupData.valueContainer] != ov.model[$s.lookupData.valueContainer]) {
                        if (nv.model[$s.lookupData.valueContainer] == null)
                            $s.lookupData.model[$s.lookupData.displayContainer] = null;
                    }
                }, true)
                $s.disable = function (ev) {
                    ev.preventDefault();
                }

                $s.clear = function () {
                    $s.lookupData.model[$s.lookupData.valueContainer] = null;
                    $s.lookupData.model[$s.lookupData.displayContainer] = null;
                }

                $s.openForm = function (row) {
                    $s.lookupData.model[$s.lookupData.valueContainer] = row[$s.lookupData.targetValue];
                    $s.lookupData.model[$s.lookupData.displayContainer] = row[$s.lookupData.targetDisplay];
                    if ($s.cascade != undefined) $s.cascade({ row: row, model: $s.lookupData.model });

                    //Close upon selecting record
                    $("#lookup-handle").remove();
                }
                $s.openLookup = function (ev) {
                    ev = ev.currentTarget;
                    if ($('#lookup-handle').length == 0) {
                        var left = $(ev).offset().left + $(ev).width();
                        var top = $(ev).offset().top;
                        var totalLeft = (left - $(ev).parent().parent().width()) + $(ev).width();
                        var totalTop = top + $(ev).parent().parent().height() + 5;
                        if (totalLeft + 400 > $(window).width()) {
                            var t = $(ev).parent().parent().offset().left + $(ev).parent().parent().width();
                            totalLeft = totalLeft - ((totalLeft + 400) - t);
                        }
                        if ((top + 305) > $(window).height()) { ///ung extrang 5 sa 305 is margin lang
                            var t = $(ev).parent().parent().offset().top;
                            totalTop = totalTop - ((totalTop + 305) - t);
                        }
                        var tmpPath = localStorage.getItem("HtmlTemplatePath");

                        $s.SearchField = "";
                        $('body')
                            .append(
                                $compile(
                                    '<div id="lookup-handle" class="lookup-table">' +
                                        '<div id="lookup-handleheader">' +
                                             $s.lookupData.label +
                                        '</div>' +
                                        '<div class="lookup-title">' +
                                            '<input class="form-control" type="text" ng-model="SearchField" placeholder="Search..." />' +                                            
                                            '    <button class="lookup-btn" type="button" name="search" ng-click="Search()"><i class="fas fa-search"></i></button>' +
                                            '    <button class="lookup-btn" type="button" name="clear" ng-click="Clear()"><i class="fas fa-redo"></i></button>' +
                                        '</div>' +
                                        '<div onload="RemoveElement()" id="lookup-view" ng-include="\'' + tmpPath + '/Web/Directives/insysTable.tmpl.html\'" include-replace ></div>' +
                                    '</div>')($s));

                        $s.RemoveElement = function () {
                            $(".listing-header").remove();
                        }
                        //$(".listing-header").remove();
                        var elem = $("#lookup-handle");
                        elem.removeAttr('style');
                        elem[0].style.top = totalTop + 'px';
                        elem[0].style.left = totalLeft + 'px';

                        $s.Search = function () {
                            $s.tblOptions.Filters = [];
                            $s.tblOptions.Filters.push(
                                { Name: 'Name', Type: 9, Label: 'Name', Value: $s.SearchField }
                            );
                            $s.RefreshLoadTable();
                        }

                        $s.Clear = function () {
                            $s.tblOptions.Filters = [];
                            $s.RefreshLoadTable();
                            $s.SearchField = "";
                        }


                        //Search data
                        $(document).on("keypress", "input", function (e) {
                            if (e.which === 13) {
                                $s.Search();
                            }
                        });
                        
                        $s.SetSystemStatus('Loading lookup', 'loading');
                        $s.LoadTable($s.tblOptions, $s.lookupOptions.method, $s.lookupOptions.controller, $s.lookupOptions.parameter).then(function (ret) {
                            if (ret.Type == 2) {
                                $s.SetSystemStatus(ret.Message, 'error');
                            } else {
                                $s.SetSystemStatus('Ready');
                            }
                            $s.$apply();
                        });




                        if (elem.length > 0) {

                            function dragElement(elmnt) {
                                var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
                                if (document.getElementById(elmnt.id + "header")) {
                                    document.getElementById(elmnt.id + "header").onmousedown = dragMouseDown;
                                } else {
                                    elmnt.onmousedown = dragMouseDown;
                                }

                                function dragMouseDown(e) {
                                    e = e || window.event;
                                    e.preventDefault();
                                    pos3 = e.clientX;
                                    pos4 = e.clientY;
                                    document.onmouseup = closeDragElement;
                                    document.onmousemove = elementDrag;
                                }

                                function elementDrag(e) {
                                    e = e || window.event;
                                    e.preventDefault();
                                    pos1 = pos3 - e.clientX;
                                    pos2 = pos4 - e.clientY;
                                    pos3 = e.clientX;
                                    pos4 = e.clientY;
                                    elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
                                    elmnt.style.left = (elmnt.offsetLeft - pos1) + "px";
                                }

                                function closeDragElement() {
                                    //document.onmouseup = null;
                                    document.onmousemove = null;

                                }
                            }
                            dragElement(elem[0]);

                            $(document).on('mouseup', function (ee) {
                                if ($(ee.target).closest("#lookup-handle").length === 0) {
                                    $("#lookup-handle").remove();
                                }
                            });
                        }
                    }
                }
            }
        }
    }])
    .directive('insysTree', ['$compile', function ($c) {
        return {
            restrict: 'A',
            scope: {
                insysTree: '=',
                displayColumn: '=',
                getSelected: '&',
                allowCheck: '=?'
            },
            transclude: true,
            replace: true,
            templateUrl: baseUrl + 'Directives/insysTree.tmpl.html',
            compile: function (E, A, transclude) {
                var contents = E.contents().remove();
                var compiledContents;

                return function ($s, $e, $a) {
                    $s.allowCheck = $s.allowCheck == undefined ? true : $s.allowCheck;
                    if (!compiledContents) {
                        compiledContents = $c(contents, transclude);
                    };
                    compiledContents($s, function (clone) {
                        $e.append(clone);
                    });
                    $s.ToggleChildren = function (node) {
                        node.ShowChild = !node.ShowChild
                    }
                    $s.$watch('insysTree', function () {
                        Enumerable.From($s.insysTree).ForEach(function (node) {
                            if (node.Children == undefined) node.Children = [];
                        });
                    });
                    $s.getNodeState = function (node) {
                        if (node.ShowChild == false || node.ShowChild == undefined) {
                            return 'fa-plus-square';
                        } else {
                            return 'fa-minus-square';
                        }
                    }
                    $s.selectNode = function (row) {
                        if (row.Children.length == 0) {
                            $s.getSelected({ data: row });
                            $('.tree-li').removeClass('tree-selected');
                            $('#tree-' + row.ID_Menus).addClass('tree-selected');
                        }
                    }
                }
            }
        }
    }])
    .directive('insysFilter', ['$controller', '$q', function ($c, $q) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: baseUrl + 'Directives/insysFilter.tmpl.html',
            link: function ($s, $e, $a) {

                if ($s.DropdownSources == undefined)
                    $s.DropdownSources = [];
                $s.GetDropdownSource = function (name) {
                    var dd = Enumerable.From($s.DropdownSources).Where(x => x.Name == name).ToArray();
                    if (dd.length > 0) return dd[0].Data;
                    else return [];
                }
                $s.$watch('tblOptions.FilterVisible', function () {
                    if ($s.tblOptions != undefined)
                        if ($s.tblOptions.FilterVisible == true) {
                            if ($s.DropdownSources.length == 0) {
                                var dropdowns = Enumerable.From($s.tblOptions.Filters).Where(x => $s.IsNull(x.ControlType, "").toLowerCase() == "select").ToArray();
                                angular.forEach(dropdowns, function (dd) {
                                    var source = { Name: dd.Name, Source: $s.Request(dd.Options.method, dd.Options.parameter, dd.Options.controller), Data: [] };
                                    $s.DropdownSources.push(source);
                                });
                                if ($s.DropdownSources.length > 0)
                                    $q.all(Enumerable.From($s.DropdownSources).Select(x => x.Source).ToArray()).then(function (results) {
                                        angular.forEach(results, function (ret, idx) {
                                            if (ret.Type == 2) {
                                                $s.SetSystemStatus(ret.Message, 'error');
                                            } else {
                                                $s.DropdownSources[idx].Data = ret.Data.Rows;
                                            }
                                        });
                                    });
                            }
                        }
                });

                $s.ClearFilter = function () {
                    function IsOptionsDefined(oo) {
                        if (oo.Options != undefined) {
                            if (oo.Options.tblOptions != undefined) {
                                if (oo.Options.tblOptions.Filters != undefined) {
                                    Enumerable.From(oo.Options.tblOptions.Filters).ForEach(function (x, idx) {
                                        if (x.ControlType != undefined) {
                                            if (x.Type == 12) {
                                                x.Value = [null, null]
                                            } else {
                                                IsOptionsDefined(x);
                                            }
                                        } else x.Value = null;
                                    })
                                } oo.Value = null;
                            } oo.Value = null;
                        } else oo.Value = null;
                    }
                    Enumerable.From($s.tblOptions.Filters).ForEach(function (x, idx) {
                        if (x.ControlType != undefined) {
                            if (x.Type == 12) {
                                x.Value = [null, null]
                            } else {
                                IsOptionsDefined(x);
                            }
                        } else {
                            x.Value = null;
                        }
                    })
                    $s.RefreshLoadTable();
                }

            }
        }
    }])
    .directive('ngEnter', [function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });
                    event.preventDefault();
                }
            });
        };
    }])
    .directive('ngInput', ['$compile', '$controller', '$timeout', function ($compile, $c, $t) {
        return {
            require: "ngModel",
            restrict: "A",
            scope: {
                textType: "=",
                decimalPlace: "=?",
                dbDataType: "=?",
                textFormat: "=?",
                ngMin: "=?",
                ngMax: "=?",
                restrictInput: "=?",//bba
                cascade: "&"


            },
            link: function (scope, element, attrs, ngModel) {
                scope.ngModel = ngModel;
                if (scope.dbDataType != undefined) {
                    $(element).input({
                        type: (scope.dbDataType == 'int' ? 'number' : (scope.dbDataType == 'decimal' ? 'decimal' : ''))
                    });
                } else {
                    if (scope.restrictInput == "lettersonly") { //bba
                        $(element).on('keydown', function (event) {
                            var inputValue = event.which;
                            if (!(inputValue >= 65 && inputValue <= 120) && (inputValue != 32 && inputValue != 0) && inputValue != 8 && inputValue != 189 && inputValue != 190) {
                                event.preventDefault();
                            }
                        });
                    } //bba
                    if (scope.textType != undefined) {
                        $(element).input({
                            type: scope.textType,
                            format: scope.textFormat
                        });
                        if (scope.textType == "date") {
                            $c('insysDatePickerController', { $scope: scope, $element: element, $attrs: attrs });
                            
                            scope.DisableTypeDate = function (d) {
                                var ret = false;
                                var ret2 = false;
                                var current = new Date(d.getFullYear(), d.getMonth(), [d.getDate()]);
                                if (scope.ngMin != undefined && scope.ngMin != "" && scope.ngMin != null) {
                                    var minDate = new Date(scope.ngMin);
                                    if (current < minDate) ret = true;
                                    else ret = false;
                                } else ret = false;

                                if (scope.ngMax != undefined && scope.ngMax != "" && scope.ngMax != null) {
                                    var maxDate = new Date(scope.ngMax);
                                    if (current > maxDate) ret2 = true;
                                    else ret2 = false;
                                } else ret2 = false;
                                return ret || ret2;
                            }
                            element.on('blur', function () {
                                if (/\S/.test(element.val())) {
                                    if (element.val() == "" || element.val() == null) {
                                        ngModel.$setViewValue(null);
                                        ngModel.$render();
                                        scope.$apply();
                                        return;
                                    }
                                    var d = new Date(element.val());
                                    if (scope.DisableTypeDate(d)) {
                                        element.val(null);
                                        return;
                                    }
                                    ngModel.$setViewValue(moment(new Date(element.val())).format(scope.textFormat || 'MM/DD/YYYY'));
                                    ngModel.$render();
                                    if ($("#insysDatePicker").length > 0) {
                                        scope.updateCalendar(d.getMonth(), d.getDate(), d.getFullYear());
                                        
                                        scope.$apply();
                                    }
                                }
                            });
                            scope.IsFirstLoad = false;
                            scope.$watch(function () {
                                return ngModel.$modelValue;
                            }, function (newValue) {
                                if (newValue != "" && newValue != undefined && newValue != null) {
                                    //if (!scope.IsFirstLoad) {
                                    ngModel.$setViewValue(moment(new Date(newValue)).format(scope.textFormat || 'MM/DD/YYYY'));
                                    ngModel.$render();
                                    var d = new Date(newValue);
                                    if ($("#insysDatePicker").length > 0) {
                                        scope.updateCalendar(d.getMonth(), d.getDate(), d.getFullYear());
                                        scope.cascade({ DateSelected: newValue });
                                        //scope.$apply();
                                    }
                                    scope.IsFirstLoad = true;
                                    //} else return;
                                } else {
                                    ngModel.$setViewValue(null);
                                    ngModel.$render();
                                    //scope.$apply();
                                }
                            });
                        } else if (scope.textType == "time") {
                            $c('insysTimePickerController', { $scope: scope, $element: element, $attrs: attrs });
                            element.on('focus', function (e) {
                                scope.IsFirstLoad = true;
                                if (attrs.readonly) return;
                                if (attrs.disabled) return;
                                var picker = $compile('<div id="insysTimePicker">' + //IsDisabledHours(d, data.currentFormat).toString()
                                    '<select name="insysTimePicker-hours" ng-model="data.currentHour" id="insysTimePicker-hours" ng-change="jump()" ng-options="item disable when IsDisabledTime(item, data.currentMinute, data.currentFormat) for item in hours"></select>' + '<i>:</i>' +
                                    '<select name="insysTimePicker-minutes" ng-model="data.currentMinute" id="insysTimePicker-minutes" ng-change="jump()" ng-options="item disable when IsDisabledTime(data.currentHour, item, data.currentFormat) for item in minutes"></select>' + '<i>:</i>' +
                                    '<select name="insysTimePicker-format" ng-model="data.currentFormat" id="insysTimePicker-format" ng-change="jump()" ng-options="item disable when IsDisabledTime(data.currentHour, data.currentMinute, item) for item in formats"></select>' +
                                    '</div>')(scope);
                                var pcker = $('body').find('#insysTimePicker');
                                if (pcker.length == 0) $('body').append(picker);
                                else pcker.remove(); $('body').append(picker);

                                if (ngModel.$modelValue != null) {
                                    var hour = ngModel.$modelValue.split(':')[0];
                                    var minute = ngModel.$modelValue.split(':')[1].split(' ')[0];
                                    var format = ngModel.$modelValue.split(' ')[1];
                                    scope.updateClock(hour, minute, format);
                                } else {
                                    var today = new Date();
                                    scope.updateClock(today.getHours(), today.getMinutes(), (today.getHours() >= 12 ? 'PM' : 'AM'));
                                }
                                scope.$watch('data.selectedValue', function (nv, ov) {
                                    ngModel.$setViewValue(scope.data.selectedValue);
                                    ngModel.$render();
                                })
                                scope.$apply();
                                //$('select').each(function () {
                                //    $('select option[value="Disabled"]').not(this).prop('disabled', true);
                                //});
                            });
                            element.on('blur', function () {
                                if (/\S/.test(element.val())) {
                                    if (element.val() == "" || element.val() == null) {
                                        ngModel.$setViewValue(null);
                                        ngModel.$render();
                                        scope.$apply();
                                        return;
                                    }
                                    ngModel.$setViewValue(moment(new Date(new Date().toDateString() + ' ' + element.val())).format(scope.textFormat || 'hh:mm A'));
                                    ngModel.$render();
                                    var d = element.val();
                                    if ($("#insysTimePicker").length > 0) {
                                        var hour = d.split(':')[0];
                                        var minute = d.split(':')[1].split(' ')[0];
                                        var format = d.split(' ')[1];
                                        scope.updateClock(hour, minute, format);
                                        scope.$apply();
                                    } else {
                                        //scope.$apply();
                                    }
                                }
                            });
                            scope.IsFirstLoad = false;
                            scope.$watch(function () {
                                return ngModel.$modelValue;
                            }, function (newValue) {
                                if (newValue != "" && newValue != undefined && newValue != null) {
                                    if (!scope.IsFirstLoad) {
                                        ngModel.$setViewValue(moment(newValue).format(scope.textFormat || 'hh:mm A'));
                                        ngModel.$render();
                                        var d = newValue;
                                        if ($("#insysTimePicker").length > 0) {
                                            var hour = d.split(':')[0];
                                            var minute = d.split(':')[1].split(' ')[0];
                                            var format = d.split(' ')[1];
                                            scope.updateClock(hour, minute, format);
                                            //scope.$apply();
                                        }
                                        scope.IsFirstLoad = true;
                                    } else return;
                                } else {
                                    ngModel.$setViewValue(null);
                                    ngModel.$render();
                                    //scope.$apply();
                                }
                            });
                        } else if (scope.textType == "datetime") {
                            element.on('blur', function () {
                                if (element.val() == "" || element.val() == null) {
                                    ngModel.$setViewValue(null);
                                    ngModel.$render();
                                    scope.$apply();
                                    return;
                                }
                                if (/\S/.test(element.val())) {
                                    ngModel.$setViewValue(vcl.DateTime.Format(element.val(), vcl.DateTime.masks.inSysDateTime));
                                    ngModel.$render();
                                }
                            });
                            scope.$watch(function () {
                                return ngModel.$modelValue;
                            }, function (newValue) {
                                if (newValue && newValue != "") {
                                    ngModel.$setViewValue(vcl.DateTime.Format(newValue, vcl.DateTime.masks.inSysDateTime));
                                    ngModel.$render();
                                }
                            });
                        }
                    }
                }
            }
        };
    }])
    .directive('ngRadio', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: {},
            link: function ($s, $e, $a, $ngModel) {
                $e.on('click', function () {
                    var val = $a.value;
                    if (val == 1 && $ngModel.$viewValue != 1) {
                        $ngModel.$setViewValue(1);
                    } else if (val == 0 && $ngModel.$viewValue != 0) {
                        $ngModel.$setViewValue(0);
                    } else if (val == 1 && $ngModel.$viewValue == 1) {
                        $ngModel.$setViewValue(null);
                    } else if (val == 0 && $ngModel.$viewValue == 0) {
                        $ngModel.$setViewValue(null);
                    }
                    $ngModel.$render();
                })
            }
        }
    })
    .directive('insysUpload', ['$controller', function ($c) {
        return {
            restrict: 'E',
            scope: {
                readonly: '=',
                required: '=',
                options: '=',
                targetModel: '=',
            },
            replace: true,
            templateUrl: baseUrl + 'Directives/insysUpload.tmpl.html',
            link: function ($s, $e, $a) {
                $s.label = $a.cLabel;
                $c('BaseController', { $scope: $s });
                $s.elementName = $a.name;
                $s.fileName = "";
                $s.$watch('options', function () {
                    if ($s.options != undefined) {
                        $s.IsHidden = $s.options.IsHidden;
                    }
                }, true);
                $s.$watch('targetModel', function () {
                    if ($s.IsNull($s.targetModel, "") != "") {
                        var files = $s.targetModel.split(",");
                        $s.fileName = Enumerable.From(files).Select(x => x.split('.CDITI.')[0]).ToArray().join();
                    }
                });

                $s.clear = function () {
                    $s.targetModel = null;
                    $s.fileName = "";
                }
                $s.upload = function () {
                    $s.UploadFile('UploadFile', $s.options.Parameter, $s.options.IsMultiple, $s.options.Accept).then(function (ret, s, files) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.targetModel = Enumerable.From(ret.Data).Select(x => x.FileName).ToArray().join();
                            $s.fileName = Enumerable.From(ret.Data).Select(x => x.FileName.split('.CDITI.')[0]).ToArray().join();
                            $s.$apply();
                            if ($s.options.CallBack != undefined) {
                                $s.options.CallBack(files);
                            }
                        }
                    })
                }

                $s.downloadFile = function () {
                    var folder = ($s.options.Parameter != undefined ? ($s.options.Parameter.folder != undefined ? $s.options.Parameter.folder : 'Files') : 'Files');
                    if ($s.options.DownloadFolder != undefined && $s.options.DownloadFolder != null && $s.options.DownloadFolder != "") folder = $s.options.DownloadFolder;
                    if ($s.targetModel.split(",").length == 1) {
                        $s.Download($s.targetModel, folder, $s.targetModel.split(".CDITI.")[0]);
                    } else {
                        var dlg = {
                            template: 'DownloadContainer',
                            controller: ['$scope', 'dData', '$uibModalInstance', '$controller', function ($ss, $data, $mi, $cc) {
                                $cc('BaseController', { $scope: $ss });
                                $ss.files = [];
                                $ss.Request('CheckFiles', { files: $data.files, folder: $data.folder }, 'Upload').then(function (ret) {
                                    if (ret.Type == 2) {
                                        $ss.SetSystemStatus(ret.Message, 'error');
                                    } else {
                                        $ss.files = ret.Data;
                                        $ss.$apply();
                                    }
                                })
                                $ss.close = function () {
                                    $mi.close(0);
                                }

                                $ss.getExtension = function (ext) {

                                    switch (ext) {
                                        case 'doc':
                                        case 'docx':
                                            return 'fa-file-word';
                                            break;
                                        case 'xls':
                                        case 'xlsx':
                                            return 'fa-file-excel';
                                            break;
                                        case 'pdf':
                                            return 'fa-file-pdf';
                                            break;
                                        case 'txt':
                                            return 'fa-file-alt';
                                            break;
                                        default:
                                            return 'fa-file';

                                    }
                                }

                                $ss.getSize = function (size) {
                                    //byte
                                    var kb = Math.round(size / 1024);
                                    if (kb > 0) {
                                        var mb = Math.round(kb / 1024);
                                        if (mb > 0) {
                                            var gb = Math.round(mb / 1024);
                                            if (gb > 0) {
                                                var tb = Math.round(gb / 1024);
                                                if (tb > 0) {
                                                    return tb + ' ' + 'TB';
                                                } else {
                                                    return gb + ' ' + 'GB';
                                                }
                                            } else {
                                                return mb + ' ' + 'MB';
                                            }
                                        } else {
                                            return kb + ' ' + 'KB';
                                        }
                                    } else {
                                        return size + ' ' + (size > 1 ? 'bytes' : 'byte');
                                    }
                                }
                                $ss.downloadFile = function (name2, display2) {
                                    $ss.Download(name2, folder, display2);
                                }
                            }],
                            size: 'lg',
                            windowClass: 'download-files-dlg',
                            data: { files: $s.targetModel.split(","), folder: folder }
                        }
                        $s.Dialog(dlg);
                    }
                }

                $s.disable = function (ev) {
                    ev.preventDefault();
                }
            }
        }
    }])
    .directive('lazyLoadImage', ['DataService', '$controller', function ($ds, $c) {
        return {
            restrict: 'A',
            scope: { lazyLoadImage: '=' },
            link: function (s, e, a) {
                $c('BaseController', { $scope: s });
                s.$watch('lazyLoadImage.Image', function (c, v) {
                    e.css({
                        'background-image': "url(data:image/png;base64,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)",
                        'background-repeat': 'no-repeat',
                        'background-size': 'contain'
                        //opacity: 1
                    });

                    if (!s.lazyLoadImage.Image) {
                        e.addClass('no-img');
                    }
                    $ds.Post('LoadImage', { ImgFile: s.lazyLoadImage.Image, Container: s.lazyLoadImage.Container || null, Path: s.lazyLoadImage.Path || null, Size: s.lazyLoadImage.Size || null }, 'Base', true).then(function (d) {
                        //e.css({ opacity: 0 });
                        if (d.Type == 2) {
                            s.SetSystemStatus(d.Message, 'error');
                            e.css({ 'background-image': "url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAM1BMVEXu7u67u7vv7++4uLjg4ODCwsLk5OS2trby8vLd3d2+vr7q6urKysrHx8fY2NjV1dXPz8/wKvVCAAAHpUlEQVR4nO2d67qjKgyGBeQg4uH+r3YHBHW12oLVms7O92M9M2tay9twSCBhqopEIpFIJBKJRCKRSCQSiUQikUgkEolEIpFIJBKJRCKRSCQSiUQikUgk0jfFUesMwBOecaU+b9+/T4hdRPj7IsLfFxH+vojw90WEv6+7Cbl5r88ChJsJedsxAWKCrWTXknJsP2nlvYS8/kO2IyF688FnnNfc/c/Yi0V5mwPoGfvj7byckPN2VvWAabo8QEBsjzfgU4I3MkMYZ1FS/f28TBMC4XC4oRcT8kE/2GL1gbzJJmTs8Ei8mvDRFuPqA43LBxTqaEuvJXyeSrqVLbjNJ2TuqBFvJOSqoJMycbgNp5DsPv0l4VBEeHTBuJHQyAJAxuTBbnojYe5yn4x4cEm8j7BorWAP03BJG06j2Xr4FmE6UDCujBCWxEPnE9cRcsPbun+kkHUT1PdNIR/4Nc2+VGt2UC4jNCrERc/tnFVKuHb/NmSHbZirCLnT75t8rgSrt6bbawg5l+Um+ly62QrRriEc7wB8cOyvJMyObE8n7J776TWEN5kQ+ulGY64gNCVBw6na8F6vGYd3mZCx8ambXkH4yTA8ulYmPQ/ESwiLAr81ne3G3js8g5MHMZ8jEDSEQrim5cn1NKaqR3bgMWgJhe2rh917wKzL/QakhMI2m34zN6qUESehGHaDH24KQxCMhMKqV/sTvOpKnoaQUHRLG/wU4/8CE816W7XEQ8JHKJZ9UG6qZpQhqLSuX0W05imO/iHCBRBmFbdaAoWWzTw6CxCxES6xgGmfhpuwdbKjyd5axUZo08c/nuCkLyC1L3vbChlhClh5tbPsCZZC2twzDlyE6fCaV3b3DenMKfeZuAjlDPjqa4iImWsGKsLZPK/PL0Rq4+8Rxnn0rXGSqbPmU0yE0YTvXzyf4Wc8FBVhbEvGGVs8HM0aiYgI455RzgmUmDZfsjZHMBFOx4FZx6TJiBlrIiJCmW+X2d4Z3RQPoRhCU4pWuZwsOESEdfjkzL3j5N79FOHU5sydVdHkfh+ICKeRlZV9Ofdp/j7TDw+hnZqcGdrGQDlj1OIhjHNHbmSb/XI8hJNTmn0M978hfN+p8RDKf96GheMw2+R4CKec39zMr5jxlZFyi4dQ5H8XbHZMM9x0RIQq1w9bvdq8fyUiwuiHZdYjTM9t3+dZ4SGMeduZTo3LHrWICNN2d1Z8WOdONJgI09DKqUiwcbct46WYCKcFICfIj2M2KxBBRBjjp4J4IWtWQkUYE7Te9r20sZoVLWMiTHMNV6/XgBj9ZtYQoSJMe9mvT3jTIeq7LwIjIZsOB/nLoShkOkDMqz/BRTifce8jiq7wnBsX4XxEWpkdb0WkZMrsRyIjXKoKebtxzi1YyiXiWedOGAmX1HNuGvs331KwJRkscxBiJGRzKkJIqJlq88PPbkmnKQDESLgg+qSvthlGUK+qJSVqN1PjVwhFs+R9VdVzoRZvCx6GktD74C8qz0qS2rAShoKl7XaUp9DiJPSei9pg5KYtrlHESugZ/yZ6+0zTuivP18dL6JcIOSifre9vpKlUfwAPOWGAFMxK0GZtJiLC0irtM/Wdmplb656+U9lVeJfAiRLPVaTX1B8W3Qdxpr5Vf3hfDenGDSjXjMP8zPOTCb9VB+z3Nu9AjLv/XyG8oxRYsM17iK4irMCnfHMJwsmS/XaIchmhL15qVf0trUPmbxFW1Vdva99vxJWEp+um1mZ+ySbuUVTGhD9Mt1ou/2x8v56fVvkAI/1+fkl8y3Ip5ge2Pf1rUKxJ5RXOw9ROMjm2aYVumeOVlcvrZTi75/VcjGAaZ+EdteENi1mYX2t9jiCiisuxsR2Yp9PMjZ3QyVNuNXAvhQfg94V8FFhXdVgDeGu1daNjWvJGjNMUdgvJnnzMKBdCqQff2yqXECfCxSmZMm7A/RtDmgJvvVPtu/rgTKMH890emCMgdDp4jZ5w0DEl1nSTiSZC4dicSDpqmFf4oFvny4LAlnHTEcCAEBdcEFe6HrU/9vWEjKUmKt2tCIchAHvT9YHQSlPDu+Dd60vC0BJy609igLBdFcdG2Ilw5GFiMWC3AQi5pzPwK97r5dISnnrpLSC7AsLGtMJCx7SdmicYmDOnkC4Sml433mKD8YTQN/2ko5UZ9eRU+ypvmGmkAyEzpCf0I6gzLwk5lxbAmOfyWV7O+DvfRhiXgZB3UkrViG4AfXDz7hUKhBBxgHWgly6j6rGX+o6p/MgDQj8hwbKgpOD9dMkVb/pRK7y91E8zUiuYaaxYfj0NyUTos2akNcGGqbzCCoCO1V3QDRTamcYT8kpYIOyhwcEm0EnVHxv6runHnCcEc07OGcw1MpaSoicMqVuw4nd6DNcOdnFhXAiBzYWfmrtU3wY9U4EvE1zSZumluDAToR9cXRiQWnZSz+XcC+F0JbZf61epNKOBb6ZvWzWA29No1wfdBbMprtJN18Z5DqOGrnP9vHPUSuh53WrR62Uz343NXQdT6mi1FnKAbtBNko8fcq/4HINPvYuHG5OWkM//8zpMj/HW6i0xBuPL/zbwpaaTSCQSiUQikUgkEolEIpFIJBKJRCKRSCQSiUQikUgkEolEIpH+6D/+pF/9/+I9DwAAAABJRU5ErkJggg==)", 'background-size': 'initial' });
                            e.addClass('no-img');
                        } else {
                            var a = setTimeout(function () {
                                clearTimeout(a);
                                if (d) {
                                    e.css({ 'background-image': "url(" + d.Data + ")", 'background-size': 'contain' });
                                    var b = setTimeout(function () {
                                        clearTimeout(b);
                                    }, 1000)
                                } else {
                                    e.css({ 'background-image': "url(data:image/png;base64,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)", 'background-size': 'contain' });
                                    e.addClass('no-img');
                                }
                                if (d.Data == null || d.Data == "") {
                                    e.css({ 'background-image': "url(data:image/png;base64,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)", 'background-size': 'contain' });
                                    e.addClass('no-img');
                                }
                            });
                        }
                        s.$apply();
                    }).fail(function () {
                        e.css({ 'background-image': "url(data:image/png;base64,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)", 'background-size': 'initial' });
                        e.addClass('no-img');
                    });
                })
            }
        }
    }])
    .directive('insysReport', ['$controller', '$http', '$rootScope', '$sce', function ($c, $http, $rs, $sce) {
        return {
            restrict: 'E',
            scope: { options: '=' },
            replace: true,
            templateUrl: baseUrl + 'Directives/insysReport.html',
            link: function ($s, $e, $a) {
                $c('insysReportController', { $scope: $s });
                var options = {
                    Pdf: { Method: '', Parameter: {}, Controller: '' },
                    Xls: { Method: '', Parameter: {}, Controller: '' },
                    hasLoadPdf: true,
                    hasExportXls: true,
                    triggerLoadPdf: function () { },
                    triggerLoadXls: function () { },
                    reportReady: false
                }

                $s.options = $.extend(options, $s.options);
                $s.webReportUrl = $sce.trustAsResourceUrl(localStorage.getItem("HtmlTemplatePath").substr(1) + 'Web/PDFViewer/web/viewer.html');
                $e.ready(function () {
                    var frame = $e.find('iframe')[0];
                    var iframe = frame.contentWindow ? frame.contentWindow : frame.contentDocument.defaultView;
                    $s.RefreshReport = function () {
                        if ($s.options.Pdf.Method == '' || $s.options.Pdf.Controller == '')
                            $s.SetSystemStatus('Method or Controller not defined.');
                        else
                            $s.LoadReport($s.options.Pdf.Method, $s.options.Pdf.Parameter, $s.options.Pdf.Controller).then(function (ret) {
                                if (ret.Type == 2) {
                                    $s.SetSystemStatus(ret.Message, 'error');
                                } else {
                                    $s.SetSystemStatus('Loading report', 'loading');
                                    if (ret.Data.UseReportServer == true) {
                                        $http.post(ret.Data.Url, { ID: ret.Data.ID, sID: $rs.SessionID }).then(function (ret2) {
                                            if (ret2.data.Type == 2) {
                                                $s.SetSystemStatus(ret2.data.Message, 'error');
                                            } else {
                                                var pdfArray = $s.convertDataURIToBinary(ret2.data.Data);
                                                iframe.PDFViewerApplication.open(ret2.data.Data);
                                                $s.SetSystemStatus('Ready');
                                            }
                                        });
                                    } else {
                                        var pdfArray = $s.convertDataURIToBinary(ret.Data);
                                        iframe.PDFViewerApplication.open(ret.Data)
                                        $s.SetSystemStatus('Ready');
                                    }
                                }
                            });
                    }
                    $s.DownloadExcel = function () {
                        if ($s.options.Xls.Method == '' || $s.options.Xls.Controller == '')
                            $s.SetSystemStatus('Method or Controller not defined.');
                        else
                            $s.LoadReport($s.options.Xls.Method, $s.options.Xls.Parameter, $s.options.Xls.Controller).then(function (ret) {
                                if (ret.Type == 2) {
                                    $s.SetSystemStatus(ret.Message, 'error');
                                } else {
                                    $s.SetSystemStatus('Loading report', 'loading');
                                    if (ret.Data.UseReportServer == true) {
                                        $http.post(ret.Data.Url, { ID: ret.Data.ID, sID: $rs.SessionID }).then(function (ret2) {
                                            if (ret2.data.Type == 2) {
                                                $s.SetSystemStatus(ret2.data.Message, 'error');
                                            } else {
                                                window.open(ret2.data.Data);
                                                $s.SetSystemStatus('Ready');
                                            }
                                        });
                                    } else {
                                        window.open(ret.Data);
                                        $s.SetSystemStatus('Ready');
                                    }
                                }
                            });
                    }

                    $s.options.triggerLoadPdf = $s.RefreshReport;
                    $s.options.triggerLoadXls = $s.DownloadExcel;
                    $s.$apply();
                    $s.options.reportReady = true;
                })
            }
        }
    }])
    .directive('insysOrgChart', ['$compile', function ($c) {
        return {
            restrict: 'A',
            scope: {
                insysOrgChart: '=',
                displayColumn: '=',
                getSelected: '&',
                allowCheck: '=?'
            },
            transclude: true,
            replace: true,
            templateUrl: baseUrl + 'Directives/insysOrgChart.tmpl.html',
            compile: function (E, A, transclude) {
                var contents = E.contents().remove();
                var compiledContents;

                return function ($s, $e, $a) {
                    $s.allowCheck = $s.allowCheck == undefined ? true : $s.allowCheck;
                    if (!compiledContents) {
                        compiledContents = $c(contents, transclude);
                    };
                    compiledContents($s, function (clone) {
                        $e.append(clone);
                    });
                    $s.ToggleChildren = function (node) {
                        node.ShowChild = !node.ShowChild
                    }
                    $s.$watch('insysOrgChart', function () {
                        Enumerable.From($s.insysOrgChart).ForEach(function (node) {
                            if (node.Children == undefined) node.Children = [];
                        });
                    });
                    $s.getNodeState = function (node) {
                        if (node.ShowChild == false || node.ShowChild == undefined) {
                            return 'fa-chevron-down';
                        } else {
                            return 'fa-chevron-up';
                        }
                    }
                    $s.selectNode = function (row) {
                        if (row.Children.length == 0) {
                            $s.getSelected({ data: row });
                            $('.tree-li').removeClass('tree-selected');
                            $('#tree-' + row.ID_Menus).addClass('tree-selected');
                        }
                    }
                }
            }
        }
    }])
    .directive('insysTable', ['$controller', function ($c) {
        return {
            templateUrl: baseUrl + 'Directives/insysTable.tmpl.html',
            scope: {
                options: '=',
                getLoad: '=',
                updateOptions: '='
            },
            restrict: 'E',
            link: function ($s, $e, $a) {
                $c('insysTableController', { $scope: $s });
                $s.initTable = function () {
                    return $s.LoadTable($s.options.tblOptions, $s.options.Method, $s.options.Controller, $s.options.Parameter).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.SetSystemStatus('Ready');
                        }
                        $s.$apply();
                        return $s.tblOptions;
                    });
                }


                $s.ValidateField = {
                    FieldChangeEvent: function () {
                        console.log("change");
                    },
                    FieldBlurEvent: function () {
                        console.log("blur");
                    }

                }

                $s.$watch('options.tblOptions', function (nv, ov) {
                    if (JSON.stringify(nv) != JSON.stringify(ov))
                        $s.tblOptions = $.extend($s.options.tblOptions, $s.tblOptions);
                }, true);
                $s.getLoad = $s.initTable;

                $s.delete = function () {
                    console.log($s.options.tblOptions);
                    $s.options.tblOptions.deleteRow($s.tblOptions);
                }
            }
        }
    }])
    .directive('insysMenuTree', ['$compile', function ($c) {
        return {
            restrict: 'A',
            scope: {
                insysMenuTree: '=',
                displayColumn: '=',
                getSelected: '&',
                allowCheck: '=?'
            },
            transclude: true,
            replace: true,
            templateUrl: baseUrl + 'Directives/insysMenuTree.tmpl.html',
            compile: function (E, A, transclude) {
                var contents = E.contents().remove();
                var compiledContents;

                return function ($s, $e, $a) {
                    $s.allowCheck = $s.allowCheck == undefined ? true : $s.allowCheck;




                    if (!compiledContents) {
                        compiledContents = $c(contents, transclude);
                    };
                    compiledContents($s, function (clone) {
                        $e.append(clone);
                    });
                    $s.ToggleChildren = function (node) {
                        console.log("toggle children");
                        node.ShowChild = !node.ShowChild;

                    }
                    $s.$watch('insysTree', function () {
                        Enumerable.From($s.insysTree).ForEach(function (node) {
                            if (node.Children == undefined) node.Children = [];
                        });
                    });

                    $s.RowsIsChecked = false;
                    //This will check the Children once Parent is checked
                    $s.UpdateNode = function (rows) {
                        $s.RowsIsChecked = rows.IsChecked;
                        //Update Children Node.
                        $s.UpdateChildNode(rows);
                    }

                    $s.UpdateChildNode = function (rows) {
                        //Update Node, This is a recursive method
                        if (!rows.Children) return;
                        if (rows.Children.length > 0) {
                            rows.IsChecked = $s.RowsIsChecked;
                            if (rows.AccessRights) {
                                var keys = Object.keys(rows.AccessRights);
                                for (const key of keys) {
                                    rows.AccessRights[key] = $s.RowsIsChecked;
                                }
                            }
                            Enumerable.From(rows.Children).ForEach(function (row) {
                                $s.UpdateChildNode(row)
                            });
                        } else {
                            rows.IsChecked = $s.RowsIsChecked;
                            if (rows.AccessRights) {
                                var keys = Object.keys(rows.AccessRights);
                                for (const key of keys) {
                                    rows.AccessRights[key] = $s.RowsIsChecked;
                                }
                            }
                        }
                    }

                    $s.getNodeState = function (node) {
                        if (node.ShowChild == false || node.ShowChild == undefined) {
                            return 'fa-plus-square';
                        } else {
                            return 'fa-minus-square';
                        }
                    }
                    $s.selectNode = function (row) {
                        if (row.Children.length == 0 || row.ID_MenusType == 2 || row.ID_MenusType == 3) {
                            $s.getSelected({ data: row });
                            $('.tree-li').removeClass('tree-selected');
                            $('#tree-' + row.ID_Menus).addClass('tree-selected');
                        }
                    }
                }
            }
        }
    }])
    .directive('orgMenu', ['$controller', function ($c) {
        return {
            templateUrl: baseUrl + 'Directives/orgMenu.tmpl.html',
            restrict: 'E',
            replace: true,
            transclude: true,
            scope: {
                treeData: '=',
                request: '&',
                parentData: '=',
                deleteOrg: '='
            },
            link: function ($s, E, A) {
                $c('BaseController', { $scope: $s });
                $s.$watch('treeData', function (nv, ov) {
                    $s.tree = nv;
                });
                $s.IsCompany = false;
                $s.addRecord = function (parent) {
                    if (parent.ID_Parent === undefined) {
                        $s.IsCompany = true;
                    }
                    $s.Request('LoadCompanyNewDefaultOrg', { Data: parent, IsCompany: $s.IsCompany, MenuCode: 'PeopleOrganizationCompany' }, 'Company').then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            return $s.Dialog({
                                template: 'CompanyOrg',
                                controller: 'CompanyOrgCtrl',
                                size: 'sm',
                                windowClass: 'company-org-dlg',
                                data: { Schema: ret.Data.Schema }
                            });
                        }
                    });
                }
                $s.Check = function (m) {
                    setTimeout(function () {
                        if (m.IsChecked) {
                            $s.deleteOrg.push(m.ID);
                        } else {
                            $s.deleteOrg.splice($s.deleteOrg.indexOf(m.ID), 1)
                        }
                        $s.$apply();
                    }, 50);
                }
                $s.openChild = function (m) {
                    $s.Request('GetLastHeirarchyLevel', { ID_Company: m.ID_Company, MenuCode: 'PeopleOrganizationCompany' }, 'Company').then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            if (ret.Data.ID != m.ID_CompanyOrgType) {
                                m.IsOpen = !m.IsOpen;
                                if (m.Children.length == 0) {
                                    $s.Request('GetChildOrg', { ID: m.ID, MenuCode: 'PeopleOrganizationCompany' }, 'Company').then(function (ret) {
                                        if (ret.Type == 2) {
                                            $s.SetSystemStatus(ret.Message, 'error');
                                        } else {
                                            m.Children = ret.Data.CompanyChildren;
                                            $s.SetSystemStatus('Ready');
                                        }
                                    });
                                }
                            }
                            $s.SetSystemStatus('Ready');
                        }
                    });
                }
            }
        }
    }])
    .controller('CompanyOrgCtrl', ['$scope', 'dData', '$controller', '$state','$uibModalInstance', function ($s,$dlgData,$c,$st,$modal) {
        $c('BaseController', { $scope: $s });
        $s.Org = $dlgData.Schema;
        $s.$watch('Org', function (nv) {
            $s.Org = nv;
        })

        $s.MasterLookup = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' },
                    { Name: 'Name', Label: 'Name' }
                ]
            },
            method: 'LoadLookUp',
            controller: 'Company',
            parameter: { OrgType: $s.Org.ID_OrgType,Name:"Master", MenuCode: 'PeopleOrganizationCompany' }
        }
        $s.Close = function () {
            $modal.dismiss();
        }
        $s.Submit = function () {
            $s.Request('SaveHeirarchy', { Data: $s.Org || {}, MenuCode: 'PeopleOrganizationCompany' }, 'Company').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.SetSystemStatus('Successfully Saved.', 'success', true);
                    $st.go($st.current.name, {}, { reload: true });
                }
            });
        };
    }])
    .directive('inputSearch', ['$timeout',function ($t) {
        return {
            restrict: 'E',
            scope: {
                sourceList: '=',
                tmpList:'='
            },
            templateUrl: baseUrl + 'Directives/inputSearch.tmpl.html',
            link: function ($s, $e, $a) {
                $e.on('keyup', function (e) {
                    var val = $(e.target).val();
                    var split = val.trim().toLowerCase().split(' ');
                    if (val == '') {
                        $s.tmpList = $s.sourceList;
                        $s.$apply();
                    } else {
                        if (val.length > 2) {
                            $s.tmpList = Enumerable.From($s.sourceList).Where(function (x) {
                               for (var i in split) {
                                   return x.Name.toLowerCase().trim().indexOf(split[i]) > -1
                               }
                            }).ToArray();
                            $s.$apply();
                        }  
                    }
                });
                $s.preventEnterKey = function (ev) {
                    if (ev.keyCode == 13) {
                        ev.preventDefault();
                    }
                }
            }
        }
    }]);