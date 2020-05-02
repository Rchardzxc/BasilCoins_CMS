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
