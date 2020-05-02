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
                            e.css({ 'background-image': "url(data:image/png;base64,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)", 'background-size': 'initial' });
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
                                    e.css({ 'background-image': "url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAM1BMVEXu7u67u7vv7++4uLjg4ODCwsLk5OS2trby8vLd3d2+vr7q6urKysrHx8fY2NjV1dXPz8/wKvVCAAAHpUlEQVR4nO2d67qjKgyGBeQg4uH+r3YHBHW12oLVms7O92M9M2tay9twSCBhqopEIpFIJBKJRCKRSCQSiUQikUgkEolEIpFIJBKJRCKRSCQSiUQikUgk0jfFUesMwBOecaU+b9+/T4hdRPj7IsLfFxH+vojw90WEv6+7Cbl5r88ChJsJedsxAWKCrWTXknJsP2nlvYS8/kO2IyF688FnnNfc/c/Yi0V5mwPoGfvj7byckPN2VvWAabo8QEBsjzfgU4I3MkMYZ1FS/f28TBMC4XC4oRcT8kE/2GL1gbzJJmTs8Ei8mvDRFuPqA43LBxTqaEuvJXyeSrqVLbjNJ2TuqBFvJOSqoJMycbgNp5DsPv0l4VBEeHTBuJHQyAJAxuTBbnojYe5yn4x4cEm8j7BorWAP03BJG06j2Xr4FmE6UDCujBCWxEPnE9cRcsPbun+kkHUT1PdNIR/4Nc2+VGt2UC4jNCrERc/tnFVKuHb/NmSHbZirCLnT75t8rgSrt6bbawg5l+Um+ly62QrRriEc7wB8cOyvJMyObE8n7J776TWEN5kQ+ulGY64gNCVBw6na8F6vGYd3mZCx8ambXkH4yTA8ulYmPQ/ESwiLAr81ne3G3js8g5MHMZ8jEDSEQrim5cn1NKaqR3bgMWgJhe2rh917wKzL/QakhMI2m34zN6qUESehGHaDH24KQxCMhMKqV/sTvOpKnoaQUHRLG/wU4/8CE816W7XEQ8JHKJZ9UG6qZpQhqLSuX0W05imO/iHCBRBmFbdaAoWWzTw6CxCxES6xgGmfhpuwdbKjyd5axUZo08c/nuCkLyC1L3vbChlhClh5tbPsCZZC2twzDlyE6fCaV3b3DenMKfeZuAjlDPjqa4iImWsGKsLZPK/PL0Rq4+8Rxnn0rXGSqbPmU0yE0YTvXzyf4Wc8FBVhbEvGGVs8HM0aiYgI455RzgmUmDZfsjZHMBFOx4FZx6TJiBlrIiJCmW+X2d4Z3RQPoRhCU4pWuZwsOESEdfjkzL3j5N79FOHU5sydVdHkfh+ICKeRlZV9Ofdp/j7TDw+hnZqcGdrGQDlj1OIhjHNHbmSb/XI8hJNTmn0M978hfN+p8RDKf96GheMw2+R4CKec39zMr5jxlZFyi4dQ5H8XbHZMM9x0RIQq1w9bvdq8fyUiwuiHZdYjTM9t3+dZ4SGMeduZTo3LHrWICNN2d1Z8WOdONJgI09DKqUiwcbct46WYCKcFICfIj2M2KxBBRBjjp4J4IWtWQkUYE7Te9r20sZoVLWMiTHMNV6/XgBj9ZtYQoSJMe9mvT3jTIeq7LwIjIZsOB/nLoShkOkDMqz/BRTifce8jiq7wnBsX4XxEWpkdb0WkZMrsRyIjXKoKebtxzi1YyiXiWedOGAmX1HNuGvs331KwJRkscxBiJGRzKkJIqJlq88PPbkmnKQDESLgg+qSvthlGUK+qJSVqN1PjVwhFs+R9VdVzoRZvCx6GktD74C8qz0qS2rAShoKl7XaUp9DiJPSei9pg5KYtrlHESugZ/yZ6+0zTuivP18dL6JcIOSifre9vpKlUfwAPOWGAFMxK0GZtJiLC0irtM/Wdmplb656+U9lVeJfAiRLPVaTX1B8W3Qdxpr5Vf3hfDenGDSjXjMP8zPOTCb9VB+z3Nu9AjLv/XyG8oxRYsM17iK4irMCnfHMJwsmS/XaIchmhL15qVf0trUPmbxFW1Vdva99vxJWEp+um1mZ+ySbuUVTGhD9Mt1ou/2x8v56fVvkAI/1+fkl8y3Ip5ge2Pf1rUKxJ5RXOw9ROMjm2aYVumeOVlcvrZTi75/VcjGAaZ+EdteENi1mYX2t9jiCiisuxsR2Yp9PMjZ3QyVNuNXAvhQfg94V8FFhXdVgDeGu1daNjWvJGjNMUdgvJnnzMKBdCqQff2yqXECfCxSmZMm7A/RtDmgJvvVPtu/rgTKMH890emCMgdDp4jZ5w0DEl1nSTiSZC4dicSDpqmFf4oFvny4LAlnHTEcCAEBdcEFe6HrU/9vWEjKUmKt2tCIchAHvT9YHQSlPDu+Dd60vC0BJy609igLBdFcdG2Ilw5GFiMWC3AQi5pzPwK97r5dISnnrpLSC7AsLGtMJCx7SdmicYmDOnkC4Sml433mKD8YTQN/2ko5UZ9eRU+ypvmGmkAyEzpCf0I6gzLwk5lxbAmOfyWV7O+DvfRhiXgZB3UkrViG4AfXDz7hUKhBBxgHWgly6j6rGX+o6p/MgDQj8hwbKgpOD9dMkVb/pRK7y91E8zUiuYaaxYfj0NyUTos2akNcGGqbzCCoCO1V3QDRTamcYT8kpYIOyhwcEm0EnVHxv6runHnCcEc07OGcw1MpaSoicMqVuw4nd6DNcOdnFhXAiBzYWfmrtU3wY9U4EvE1zSZumluDAToR9cXRiQWnZSz+XcC+F0JbZf61epNKOBb6ZvWzWA29No1wfdBbMprtJN18Z5DqOGrnP9vHPUSuh53WrR62Uz343NXQdT6mi1FnKAbtBNko8fcq/4HINPvYuHG5OWkM//8zpMj/HW6i0xBuPL/zbwpaaTSCQSiUQikUgkEolEIpFIJBKJRCKRSCQSiUQikUgkEolEIpH+6D/+pF/9/+I9DwAAAABJRU5ErkJggg==)", 'background-size': 'contain' });
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