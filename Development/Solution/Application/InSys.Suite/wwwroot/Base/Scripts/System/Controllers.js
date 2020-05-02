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