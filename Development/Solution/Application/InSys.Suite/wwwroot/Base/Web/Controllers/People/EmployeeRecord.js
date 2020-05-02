angular.module('app')
    .controller('PeopleEmployeeRecord', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
        $c('insysTableController', { $scope: $s });
        $s.MenuPrivileges = $priv.Data;
        $s.Schema = {};
        $s.Persona = {};
        $s.TableSchema = [];
        $s.PersonaAddress = [];
        $s.PersonaAddressSchema = [];
        $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.isEditable = false;
        $s.showPersonalInfo = true;
        $s.activeView = 1;

        $s.NationalityList = [];
        $s.CitizenshipList = [];
        $s.CivilStatusList = [];
        $s.GenderList = [];
        $s.ReligionList = [];
        $s.BloodTypeList = [];
        $s.BarangayList = [];
        $s.CityList = [];
        $s.ProvinceList = [];
        $s.EducationalAttainment = [];
        $s.EducationalSchema = [];
        $s.EmploymentSchema = [];
        $s.EmployeeRecord = null;

        $s.MenuCode = 'PeopleEmployeeRecord';
        $s.myController = 'EmployeeRecord';

        $s.ShiftSchedule = [];

        $s.PermanentPersonaAddress = {};
        $s.PresentPersonaAddress = {};

        $s.SameAsPermanent1 = 0;
        $s.CheckBoxChange = function (e) {
            $s.SameAsPermanent1 = e.SameAsPermanent1;
            if ($s.SameAsPermanent === true) {
                $s.PresentPersonaAddress.HouseNo = $s.PermanentPersonaAddress.HouseNo;
                $s.PresentPersonaAddress.StreetName = $s.PermanentPersonaAddress.StreetName;
                $s.PresentPersonaAddress.ID_Province = $s.PermanentPersonaAddress.ID_Province;
                $s.PresentPersonaAddress.Province = $s.PermanentPersonaAddress.Province;
                $s.PresentPersonaAddress.ID_City = $s.PermanentPersonaAddress.ID_City;
                $s.PresentPersonaAddress.City = $s.PermanentPersonaAddress.City;
                $s.PresentPersonaAddress.ID_Barangay = $s.PermanentPersonaAddress.ID_Barangay;
                $s.PresentPersonaAddress.Barangay = $s.PermanentPersonaAddress.Barangay;

            }
        }

        //$s.$watch('GetAddress(1)', function (newval) {
        //    var condi = [
        //        newval.HouseNo !== $s.GetAddress(0).HouseNo,
        //        newval.StreetName !== $s.GetAddress(0).StreetName,
        //        newval.ID_Province !== $s.GetAddress(0).ID_Province,
        //        newval.Province !== $s.GetAddress(0).Province,
        //        newval.ID_City !== $s.GetAddress(0).ID_City,
        //        newval.City !== $s.GetAddress(0).City,
        //        newval.ID_Barangay !== $s.GetAddress(0).ID_Barangay,
        //        newval.Barangay !== $s.GetAddress(0).Barangay
        //    ];
        //    if (condi.includes(true))
        //        $s.SameAsPermanent = 0;
        //    //if (condi.every((val, i, arr) => val === arr[0]))
        //    //    $s.SameAsPermanent = 1;
        //}, true);

        //$s.$watch('PermanentPersonaAddress', function (newval) {
        //    var condi = [
        //        newval.HouseNo !== $s.PresentPersonaAddress.HouseNo,
        //        newval.StreetName !== $s.PresentPersonaAddress.StreetName,
        //        newval.ID_Province !== $s.PresentPersonaAddress.ID_Province,
        //        newval.Province !== $s.PresentPersonaAddress.Province,
        //        newval.ID_City !== $s.PresentPersonaAddress.ID_City,
        //        newval.City !== $s.PresentPersonaAddress.City,
        //        newval.ID_Barangay !== $s.PresentPersonaAddress.ID_Barangay,
        //        newval.Barangay !== $s.PresentPersonaAddress.Barangay
        //    ];
        //    if (condi.includes(true))
        //        $s.SameAsPermanent = 0;
        //    //if (condi.every((val, i, arr) => val === arr[0]))
        //    //    $s.SameAsPermanent = 1;
        //}, true);

        //Initialize container for data
        $s.tblOptions = {
            Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Code', Label: 'Code' },
                { Name: 'LastName', Label: 'Last Name' },
                { Name: 'FirstName', Label: 'First Name' },
                { Name: 'MiddleName', Label: 'Middle Name' },
                { Name: 'Designation' },
                { Name: 'Branch' },
                { Name: 'EmployeeStatus', Type: 9, Label: 'Employee Status' },
                { Name: 'PayrollScheme', Type: 9, Label: 'Payroll Scheme' },
                { Name: 'PayrollFrequency', Type: 9, Label: 'Payroll Frequency' },
                { Name: 'Parameter', Type: 9, Label: 'Parameter' },
                { Name: 'LeaveParameter', Type: 9, Label: 'Leave Parameter' }
            ],
            HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: false,
            HasEdit: $s.MenuPrivileges.HasEdit,
            Filters: [
                { Name: 'Code', Type: 9, Label: 'Code' },
                { Name: 'LastName', Type: 9, Label: 'Last Name' },
                { Name: 'FirstName', Type: 9, Label: 'First Name' },
                { Name: 'MiddleName', Type: 9, Label: 'Middle Name' },
                { Name: 'Designation', Type: 9, Label: 'Designation' },
                { Name: 'Branch', Type: 9, Label: 'Branch' },
                { Name: 'EmployeeStatus', Type: 9, Label: 'Employee Status' },
                { Name: 'PayrollScheme', Type: 9, Label: 'Payroll Scheme' },
                { Name: 'PayrollFrequency', Type: 9, Label: 'Payroll Frequency' },
                { Name: 'Parameter', Type: 9, Label: 'Parameter' },
                { Name: 'LeaveParameter', Type: 9, Label: 'Leave Parameter' }
            ]
        };

        $s.newForm = function () {
            if ($s.tblOptions.newForm != null || $s.tblOptions.newForm != undefined) {
                $s.tblOptions.newForm();
            } else
                if ($st.current.name.indexOf('.Form') > -1) {
                    $st.go($st.current.name, { ID: 0 }, { reload: true });
                } else {
                    $s.OpenPersonaWizard();
                    //$st.go($st.current.name + '.Form', { ID: 0 }, { reload: true });
                }
        }

        $s.GetAddress = function (IsPresent) {
            return $s.PersonaAddress.filter(x => x.IsPresentAddress === IsPresent);
            //return Enumerable.From($s.PersonaAddress).Where(function sa(x) { return x.IsPresentAddress == IsPresent }).FirstOrDefault();
        }

        $s.GetShiftSched = function (row) {
            $s.Request('ExecuteAction', { MethodName: 'LoadShiftSchedule', ID_WeeklySchedule: row.ID, MenuCode: $s.MenuCode }, $s.myController)
                .then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.ShiftSchedule = ret.Data;
                        $s.EmployeeRecord.ID_WeeklySchedule = row.ID;
                    }
                });
        }

        $s.OpenPersonaWizard = function () {
            //$s.Dialog({
            //    template: 'PersonaSelect',
            //    controller: 'dlgPersonaSelect',
            //    size: 'sm',
            //    windowClass: 'persona-select-dlg', //Please create new windowClass
            //}).result.then(function (ret) {

            //    if (ret != undefined) {
            //if (ret == 0) {
            $s.Dialog({
                template: 'PersonaWizard',
                controller: 'dlgPersonaWizard',
                size: 'lg',
                windowClass: 'employee-wizard-dlg er-wizard-dlg',
                data: {}
            }).result.then(function (ret) {
                console.log("return", ret);
                if (ret != undefined) {
                    $s.Persona = ret.Persona;
                    $s.PersonaAddress = ret.PersonaAddress;
                    $s.References = ret.CharacterReference;
                    $s.EducationalRecord = ret.EducationalBackGround;
                    $s.Licences = ret.LicensesAndCertificates;
                    $s.EmploymentHistory = ret.PreviousEmployment;
                    $s.Schema = $.extend($s.Schema, $s.Persona);
                    $s.TableSchema = ret.Schema;
                    $s.EducationalSchema = ret.TableSchemaEducationalBackGround;
                    $s.EmploymentSchema = ret.TableSchemaPreviousEmployment;
                    $s.LicencesSchema = ret.TableSchemaLicensesCertificates;
                    $s.ReferencesSchema = ret.TableSchemaCharacterReference;
                    $st.go($st.current.name + '.Form', { ID: 0 }, { reload: false });
                    //$s.RequestSave();
                } else {
                    $st.go($s.MenuCode, {}, { reload: true });
                }

            });
            //    } else if (ret == -1) { //Since this is close, then redirect to listing
            //        //redirect to list.
            //        $st.go($s.MenuCode, {}, { reload: true });
            //    } else {
            //        $s.Schema.ID_Persona = ret.ID;
            //        $s.Schema.FirstName = ret.Firstname;
            //        $s.Schema.LastName = ret.Lastname;
            //    }
            //}
            //});
        }

        $s.Init = function () {
            if ($s.RecordID != '') {
                $s.GetLookUpList();
                $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
                $s.LoadForm().then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = $s.PlotDefault(ret.Data.Persona, ret.Data.Schema, $s.RecordID);
                        $s.EmployeeRecord = ret.Data.Form;
                        $s.Schema.FirstName = $s.Schema.FirstName == null ? '' : $s.Schema.FirstName;
                        $s.Schema.LastName = $s.Schema.LastName == null ? '' : $s.Schema.LastName;
                        //$s.PersonaAddress = ret.Data.PersonaAddress;

                        $s.PresentPersonaAddress = ret.Data.PersonaAddress.filter(x => x.IsPresentAddress === true)[0];
                        $s.PermanentPersonaAddress = ret.Data.PersonaAddress.filter(x => x.IsPresentAddress === false)[0];

                        $s.TableSchema = ret.Data.Schema;
                        $s.PersonaAddressSchema = ret.Data.PersonaAddressSchema;
                        $s.EducationalSchema = ret.Data.EducationalSchema;
                        $s.EmploymentSchema = ret.Data.EmploymentSchema;
                        $s.LicencesSchema = ret.Data.LicencesSchema;
                        $s.ReferencesSchema = ret.Data.ReferencesSchema;


                        $s.SetSystemStatus('Ready');
                        if ($s.PersonaAddress.length == 0) {
                            $s.PersonaAddress.push($s.PlotDefault({}, $s.PersonaAddressSchema, 0));
                            $s.PersonaAddress[0].IsPresentAddress = true;
                            $s.PersonaAddress.push($s.PlotDefault({}, $s.PersonaAddressSchema, 0));
                        } else if ($s.PersonaAddress.length == 1) {
                            if ($s.PersonaAddress[0].IsPresentAddress == false) {
                                $s.PersonaAddress.push($s.PlotDefault({}, $s.PersonaAddressSchema, 0));
                                $s.PersonaAddress[1].IsPresentAddress = true;
                            } else {
                                $s.PersonaAddress.push($s.PlotDefault({}, $s.PersonaAddressSchema, 0));
                            }
                        }
                        $s.LoadLookuProperties();
                        $s.IsFormReady = true;
                    }
                    $s.$apply();
                })
                //For New Employee, User should Input Personal Details
                //if ($s.RecordID == 0) {
                //    $s.OpenPersonaWizard();   
                //}

            } else {
                $s.LoadTable($s.tblOptions, 'LoadList', $s.myController, { MenuCode: $s.MenuCode }).then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.SetSystemStatus('Ready');
                    }
                    $s.$apply();
                });
            }
        }

        $s.editRecord = function () {
            $s.isEditable = true;
            $s.GetLookUpList();
        }

        $s.setActiveView = function (view) {
            $s.showPersonalInfo = false;
            $s.activeView = view;
            switch (view) {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    $s.LoadWeeklySchedule();
                    break;
            }
        }

        $s.LoadWeeklySchedule = function () {
            $s.Request('LoadLookup', { MenuCode: 'PeopleEmployeeRecord', ID_WeeklySchedule: $s.EmployeeRecord.ID_WeeklySchedule, LookupName: 'LoadShiftSchedule' }, $s.myController)
                .then(function (ret) {
                    console.log(ret);
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.ShiftSchedule = ret.Data;
                    }
                });
        }

        $s.backView = function () {
            $s.showPersonalInfo = true;
        }

        $s.AddressOldValues = {};
        $s.DoExist = function (ColumnName, num) { //pang disable
            if (ColumnName == null || typeof ColumnName === "undefined") {
                $s.NullWhenChanged(num);
            } else {
                switch (num) {
                    case 1:
                        if ($s.AddressOldValues.PermanentID_Province == null || typeof $s.AddressOldValues.PermanentID_Province === "undefined") { //if null to not null
                            $s.AddressOldValues.PermanentID_Province = ColumnName;
                        } else if ($s.AddressOldValues.PermanentID_Province != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PermanentID_Province = ColumnName;
                        }
                        console.log($s.AddressOldValues.PermanentID_Province, ColumnName);
                        break;
                    case 2:
                        if ($s.AddressOldValues.PermanentID_City == null || typeof $s.AddressOldValues.PermanentID_City === "undefined") { //if null to not null
                            $s.AddressOldValues.PermanentID_City = ColumnName;
                        } else if ($s.AddressOldValues.PermanentID_City != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PermanentID_City = ColumnName;
                        }
                        console.log($s.AddressOldValues.PermanentID_City, ColumnName);
                        break;
                    case 3:
                        if ($s.AddressOldValues.PresentID_Province == null || typeof $s.AddressOldValues.PresentID_Province === "undefined") { //if null to not null
                            $s.AddressOldValues.PresentID_Province = ColumnName;
                        } else if ($s.AddressOldValues.PresentID_Province != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PresentID_Province = ColumnName;
                        }
                        console.log($s.AddressOldValues.PresentID_Province, ColumnName);
                        break;
                    case 4:
                        if ($s.AddressOldValues.PresentID_City == null || typeof $s.AddressOldValues.PresentID_City === "undefined") { //if null to not null
                            $s.AddressOldValues.PresentID_City = ColumnName;
                        } else if ($s.AddressOldValues.PresentID_City != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PresentID_City = ColumnName;
                        }
                        console.log($s.AddressOldValues.PresentID_City, ColumnName);
                        break;
                }
            }

            return (ColumnName == null || typeof ColumnName === "undefined") ? true : false; //pang disable
        }

        $s.NullWhenChanged = function (num) { // pang blank
            switch (num) {
                case 1:
                    $s.PermanentPersonaAddress.ID_City = null;
                    $s.PermanentPersonaAddress.City = null;
                    $s.PermanentPersonaAddress.ID_Barangay = null;
                    $s.PermanentPersonaAddress.Barangay = null;
                    $s.AddressOldValues.PermanentID_Province = null;
                    break;
                case 2:
                    $s.PermanentPersonaAddress.ID_Barangay = null;
                    $s.PermanentPersonaAddress.Barangay = null;
                    $s.AddressOldValues.PermanentID_City = null;
                    break;
                case 3:
                    $s.PresentPersonaAddress.ID_City = null;
                    $s.PresentPersonaAddress.ID_Barangay = null;
                    break;
                case 4:
                    $s.PresentPersonaAddress.ID_Barangay = null;
            }
        }

        $s.GetLookUpList = function () {
            $s.CityList = { Permanent: [], Present: [] };
            $s.BarangayList = { Permanent: [], Present: [] };
            $s.GenderLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Gender' }
                    ]
                },
             method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'Gender', MenuCode: $s.MenuCode }
            }
            $s.WeeklyScheduleLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Weekly Schedule' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'WeeklySchedule', MenuCode: $s.MenuCode }
            }
            $s.Nationalitylookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Nationality' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'Nationality', MenuCode: $s.MenuCode }
            }

            $s.CivilStatusLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'CivilStatus' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'CivilStatus', MenuCode: $s.MenuCode }
            }

            $s.ReligionLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Religion' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'Religion', MenuCode: $s.MenuCode }
            }

            $s.BloodTypeLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'BloodType' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'BloodType', MenuCode: $s.MenuCode }
            }

            $s.EducationalAttainmentLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'EducationalAttainment' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'EducationalAttainment', MenuCode: $s.MenuCode }
            }

            $s.CitizenshipLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Citizenship' }
                    ]
                },
                method: 'LoadLookup',
                controller: $s.myController,
                parameter: { LookupName: 'Citizenship', MenuCode: $s.MenuCode }
            }
            $s.Request('LoadLookup', { LookupName: 'Gender', MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.GenderList = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });




            //$s.Request('LoadLookup', { LookupName: 'Nationality', MenuCode: $s.MenuCode }, $s.myController ).then(function (ret) {
            //    if (ret.Type == 2) {
            //        $s.SetSystemStatus(ret.Message, 'error');
            //    } else {
            //        $s.NationalityList = ret.Data.Rows;
            //        $s.SetSystemStatus('Ready');
            //    }
            //    $s.$apply();
            //});

            //$s.Request('LoadLookup', { LookupName: 'Citizenship', MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
            //    if (ret.Type == 2) {
            //        $s.SetSystemStatus(ret.Message, 'error');
            //    } else {
            //        $s.CitizenshipList = ret.Data.Rows;
            //        $s.SetSystemStatus('Ready');
            //    }
            //    $s.$apply();
            //});
            //$s.Request('LoadLookup', { LookupName: 'CivilStatus', MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord').then(function (ret) {
            //    if (ret.Type == 2) {
            //        $s.SetSystemStatus(ret.Message, 'error');
            //    } else {
            //        $s.CivilStatusList = ret.Data.Rows;
            //        $s.SetSystemStatus('Ready');
            //    }
            //    $s.$apply();
            //});

            //$s.Request('LoadLookup', { LookupName: 'Religion', MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord').then(function (ret) {
            //    if (ret.Type == 2) {
            //        $s.SetSystemStatus(ret.Message, 'error');
            //    } else {
            //        $s.ReligionList = ret.Data.Rows;
            //        $s.SetSystemStatus('Ready');
            //    }
            //    $s.$apply();
            //});
            //$s.Request('LoadLookup', { LookupName: 'BloodType', MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord').then(function (ret) {
            //    if (ret.Type == 2) {
            //        $s.SetSystemStatus(ret.Message, 'error');
            //    } else {
            //        $s.BloodTypeList = ret.Data.Rows;
            //        $s.SetSystemStatus('Ready');
            //    }
            //    $s.$apply();
            //});
            $s.Request('LoadLookup', { LookupName: 'EducationalAttainment', MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.EducationalAttainment = ret.Data.Rows;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
            });
        }
        $s.UpdateCity = function (data, IsPresent) {
            if (IsPresent) {
                $s.cityLookup2.parameter.ID_Province = data.ID;
                $s.PresentPersonaAddress.ID_City = null;
                $s.PresentPersonaAddress.City = null;
            }
            else {
                $s.cityLookup.parameter.ID_Province = data.ID;
                $s.PermanentPersonaAddress.ID_City = null;
                $s.PermanentPersonaAddress.City = null;
            }

        }
        $s.UpdateBarangay = function (data, IsPresent) {
            if (IsPresent) $s.barangayLookup2.parameter.ID_City = data.ID;
            else $s.barangayLookup.parameter.ID_City = data.ID;
        }

        $s.GovBlur = function () {
            $s.Schema.SSSNo = $s.format271($s.Schema.SSSNo);
            $s.Schema.PhilHealthNo = $s.format444($s.Schema.PhilHealthNo);
            $s.Schema.HDMFNo = $s.format444($s.Schema.HDMFNo);
            $s.Schema.TINNo = $s.format3333($s.Schema.TINNo);
            $s.Schema.GSISNo = $s.format11($s.Schema.GSISNo);
        }

        $s.formatSSS = function (ColumnName) {
            var regex = RegExp(/^([\d]{2})-([\d]{7})-([\d]{1})$/);
            if (ColumnName) {
                if (ColumnName.length > 10 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.replace(/-/g, "");
                    ColumnName = ColumnName.slice(0, 10);
                    ColumnName = ColumnName.replace(/^([\d]{2})([\d]{7})([\d]{1})$/, "$1-$2-$3");
                } else {
                    ColumnName = ColumnName.replace(/^([\d]{2})([\d]{7})([\d]{1})$/, "$1-$2-$3");
                }
            }
            return ColumnName;
        }
        $s.format444 = function (ColumnName) {
            var regex = RegExp(/^([\d]{4})-([\d]{4})-([\d]{4})$/);
            if (ColumnName) {
                if (ColumnName.length > 12 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.replace(/-/g, "");
                    ColumnName = ColumnName.slice(0, 12);
                    ColumnName = ColumnName.replace(/^([\d]{4})([\d]{4})([\d]{4})$/, "$1-$2-$3");
                } else {
                    ColumnName = ColumnName.replace(/^([\d]{4})([\d]{4})([\d]{4})$/, "$1-$2-$3");
                }
            }
            return ColumnName;
        }
        $s.format3333 = function (ColumnName) {
            var regex = RegExp(/^([\d]{3})-([\d]{3})-([\d]{3})-([\d]{3})$/);
            if (ColumnName) {
                if (ColumnName.length > 12 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.replace(/-/g, "");
                    ColumnName = ColumnName.slice(0, 12);
                    ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                    console.log(222222);
                } else {
                    ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                }
            }
            return ColumnName;
        }
        $s.format11 = function (ColumnName) {
            var regex = RegExp(/^([\d]{11})$/);
            if (ColumnName) {
                if (ColumnName.length > 11 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.slice(0, 11);
                    //ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                } else {
                    //ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                }
            }
            return ColumnName;
        }

        $s.LoadForm = function () {
            return $s.Request('LoadForm', { ID: $s.RecordID, MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord');
        }

        $s.saveForm = function () {
            //console.log($s.PersonaAddress);
            //$s.PersonaAddress = [];
            //$s.PersonaAddress.push($s.PermanentPersonaAddress, $s.PresentPersonaAddress);
            //console.log($s.PersonaAddress);
            //return;

            if (!$s.IsTabsValid('form.employeerecord', $s.TableSchema, 'personalinfo')) { console.log(11); return; }
            if (!$s.IsTabsValid('form.employeerecord', $s.EducationalSchema, 'educational')) { console.log(22); return; }
            if (!$s.IsTabsValid('form.employeerecord', $s.EmploymentSchema, 'employment')) { console.log(33); return; }
            if (!$s.IsTabsValid('form.employeerecord', $s.LicencesSchema, 'licenses')) { console.log(44); return; }
            if (!$s.IsTabsValid('form.employeerecord', $s.ReferencesSchema, 'references')) { console.log(55); return; }

            if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                $s.Prompt('You are not allowed to update this record.');
                return;
            }

            if ($s.GetAge($s.Schema.BirthDate) < 15) {
                console.log("age must be atleast 15");
                $s.SetSystemStatus('Age must be at least 15', 'warning');
                return;
            }
            
            $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
            $s.PersonaAddress.push($s.PermanentPersonaAddress, $s.PresentPersonaAddress);
            $s.RequestSave();
        }

        $s.RequestSave = function () {
            $s.Request('SaveForm', { ID: $s.RecordID, Data: $s.Schema, PersonaAddress: $s.PersonaAddress, Educational: $s.EducationalRecord, Employment: $s.EmploymentHistory, CharacterReference: $s.References, LicensesCertificates: $s.Licences, EmployeeRecord: $s.EmployeeRecord, MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord')
                .then(function (ret) {
                    console.log("save", ret);
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.SetSystemStatus('Successfully Saved.', 'success', true);
                        $s.SetDirtyFormToFalse($s.form);
                        $st.go($st.current.name, { ID: ret.Data }, { reload: true });
                    }
                });
        }

        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

        $s.Today = new Date();
        
        //education region
        $s.EducationalRecord = [];
        $s.DelEduParams = {};
        $s.fDelEduParams = function () {
            return {
                Schema: $s.EducationalRecord,
                SchemaName: 'EducationalRecord',
                tableName: 'tPersonaEducationalBackGround'
            };
        }
        $s.loadEducationalRecord = function () {
            if ($s.EducationalRecord.length == 0) {
                $s.SetSystemStatus('Loading educational record.', 'loading');
                $s.Request('LoadLookup', { MenuCode: 'PeopleEmployeeRecord', ID: $s.Schema.ID, LookupName: 'educationalrecord' }, 'EmployeeRecord').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.EducationalRecord = ret.Data;
                        $s.DelEduParams = $s.fDelEduParams();
                        $s.SetSystemStatus('Ready');
                    }
                })
            }
        }
        //education end

        //employment history region
        $s.EmploymentHistory = [];
        $s.DelEmpHisParams = {};
        $s.fDelEmpHisParams = function () {
            return {
                Schema: $s.EmploymentHistory,
                SchemaName: 'EmploymentHistory',
                tableName: 'tPersonaEmployment'
            };
        }
        $s.loadEmploymentHistory = function () {
            if ($s.EmploymentHistory.length == 0) {
                $s.SetSystemStatus('Loading employment history.', 'loading');
                $s.Request('LoadLookup', { MenuCode: 'PeopleEmployeeRecord', ID: $s.Schema.ID, LookupName: 'employmenthistory' }, 'EmployeeRecord').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.EmploymentHistory = ret.Data;
                        $s.DelEmpHisParams = $s.fDelEmpHisParams();
                        $s.SetSystemStatus('Ready');
                    }
                })
            }
        }
        //employment history region end

        //licences region
        $s.Licences = [];
        $s.DelLiceParams = {};
        $s.fDelLiceParams = function () {
            return {
                Schema: $s.Licences,
                SchemaName: 'Licences',
                tableName: 'tPersonaLicensesAndCertificates'
            };
        }
        $s.LoadLicences = function () {
            if ($s.Licences.length == 0) {
                $s.SetSystemStatus('Loading Licences and Certificates.', 'loading');
                $s.Request('LoadLookup', { MenuCode: 'PeopleEmployeeRecord', ID: $s.Schema.ID, LookupName: 'LoadLicences' }, 'EmployeeRecord').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.Licences = ret.Data;
                        $s.DelLiceParams = $s.fDelLiceParams();
                        $s.SetSystemStatus('Ready');
                    }
                })
            }
        }
        //licences region end
        
        //references region
        $s.References = [];
        $s.DelRefParams = {};
        $s.fDelRefParams = function () {
            return {
                Schema: $s.References,
                SchemaName: 'References',
                tableName: 'tPersonaCharacterReference'
            };
        }
        $s.LoadReferences = function () {
            if ($s.References.length == 0) {
                $s.SetSystemStatus('Loading Character References.', 'loading');
                $s.Request('LoadLookup', { MenuCode: 'PeopleEmployeeRecord', ID: $s.Schema.ID, LookupName: 'LoadReferences' }, 'EmployeeRecord').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.References = ret.Data;
                        $s.DelRefParams = $s.fDelRefParams();
                        $s.SetSystemStatus('Ready');
                    }
                })
            }
        }
        //references region end

        $s.LoadLookuProperties = function () {
            $s.provinceLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Province' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'Province', MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'Province', Type: 9 }]
            };
            $s.cityLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'City' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'City', ID_Province: $s.GetAddress(false).ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'City', Type: 9 }]
            };
            $s.barangayLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Barangay' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'Barangay', ID_City: $s.GetAddress(false).ID_City, MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'Barangay', Type: 9 }]
            };

            $s.provinceLookup2 = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Province' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'Province', MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'Province', Type: 9 }]
            };
            $s.cityLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'City' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'City', ID_Province: $s.GetAddress(false).ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'City', Type: 9 }]
            };
            $s.barangayLookup = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Barangay' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'Barangay', ID_City: $s.GetAddress(false).ID_City, MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'Barangay', Type: 9 }]
            };

            $s.provinceLookup2 = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Province' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'Province', MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'Province', Type: 9 }]
            };
            $s.cityLookup2 = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'City' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'City', ID_Province: $s.GetAddress(true).ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'City', Type: 9 }]
            };
            $s.barangayLookup2 = {
                tblOptions: {
                    Columns: [
                        { Name: 'ID', Label: '#' },
                        { Name: 'Name', Label: 'Barangay' }
                    ]
                },
                method: 'LoadLookup',
                controller: 'EmployeeRecord',
                parameter: { LookupName: 'Barangay', ID_City: $s.GetAddress(true).ID_City, MenuCode: 'PeopleEmployeeRecord' },
                Filters: [{ Name: 'Barangay', Type: 9 }]
            };
           
        }

    }]).controller('dlgPersonaWizard', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });

        var PersonaData = {
            Persona: ""
            , PersonaAddress: ""
            , EducationalBackGround: ""
            , CharacterReference: ""
            , LicensesAndCertificates: ""
            , PreviousEmployment: ""
        }

        //$s.Schema = $dlgData.Schema;
        $s.currentEmpTab = 0;

        //LookUp 1
        $s.CivilStatusList = [];
        $s.BloodTypeList = [];
        $s.GenderList = [];
        $s.ReligionList = [];
        $s.CitizenshipList = [];

        //page 2 lookup
        $s.Province = [];

        //page 3 lookup
        $s.PersonaAddress = [];

        //Page 4 LookUp
        $s.EducationalAttaintment = [];

        //Schema
        $s.tPersona = {};
        $s.SchemaEducationalBackGround = [];
        $s.SchemaCharacterReference = [];
        $s.SchemaLicensesCertificates = [];
        $s.SchemaPreviousEmployment = [];
        $s.TableSchemaEducationalBackGround = [];
        $s.TableSchemaCharacterReference = [];
        $s.TableSchemaLicensesCertificates = [];
        $s.TableSchemaPreviousEmployment = [];
        $s.TableSchema = [];
        $s.SchemaPersonaAddress = []; //bba
        $s.TableSchemaPersonaAddress = []; //bba
        $s.PresentPersonaAddress = { IsPresentAddress: 1 }; //bba
        $s.PermanentPersonaAddress = { IsPresentAddress: 0 }; //bba

        $s.IsPage0Clicked = 0;
        $s.IsPage1Clicked = 0;
        $s.IsPage2Clicked = 0;
        $s.IsPage3Clicked = 0;
        $s.IsPage4Clicked = 0;
        $s.IsPage5Clicked = 0;
        $s.IsPage6Clicked = 0;
        $s.Schema = {};
        $s.MenuCode = 'PeopleEmployeeRecord';
        $s.myController = 'EmployeeRecord';
        $s.SameAsPermanent = 0;

        $s.CheckBoxChange = function () {
            if ($s.SameAsPermanent === true) {
                $s.PresentPersonaAddress.HouseNo = $s.PermanentPersonaAddress.HouseNo;
                $s.PresentPersonaAddress.StreetName = $s.PermanentPersonaAddress.StreetName;
                $s.PresentPersonaAddress.ID_Province = $s.PermanentPersonaAddress.ID_Province;
                $s.PresentPersonaAddress.Province = $s.PermanentPersonaAddress.Province;
                $s.PresentPersonaAddress.ID_City = $s.PermanentPersonaAddress.ID_City;
                $s.PresentPersonaAddress.City = $s.PermanentPersonaAddress.City;
                $s.PresentPersonaAddress.ID_Barangay = $s.PermanentPersonaAddress.ID_Barangay;
                $s.PresentPersonaAddress.Barangay = $s.PermanentPersonaAddress.Barangay;
            }
        }

        $s.$watch('PresentPersonaAddress', function (newval) {
            var condi = [
                newval.HouseNo !== $s.PermanentPersonaAddress.HouseNo,
                newval.StreetName !== $s.PermanentPersonaAddress.StreetName,
                newval.ID_Province !== $s.PermanentPersonaAddress.ID_Province,
                newval.Province !== $s.PermanentPersonaAddress.Province,
                newval.ID_City !== $s.PermanentPersonaAddress.ID_City,
                newval.City !== $s.PermanentPersonaAddress.City,
                newval.ID_Barangay !== $s.PermanentPersonaAddress.ID_Barangay,
                newval.Barangay !== $s.PermanentPersonaAddress.Barangay
            ];
            if (condi.includes(true))
                $s.SameAsPermanent = 0;
            //if (condi.every((val, i, arr) => val === arr[0]))
            //    $s.SameAsPermanent = 1;
        }, true);

        $s.$watch('PermanentPersonaAddress', function (newval) {
            var condi = [
                newval.HouseNo !== $s.PresentPersonaAddress.HouseNo,
                newval.StreetName !== $s.PresentPersonaAddress.StreetName,
                newval.ID_Province !== $s.PresentPersonaAddress.ID_Province,
                newval.Province !== $s.PresentPersonaAddress.Province,
                newval.ID_City !== $s.PresentPersonaAddress.ID_City,
                newval.City !== $s.PresentPersonaAddress.City,
                newval.ID_Barangay !== $s.PresentPersonaAddress.ID_Barangay,
                newval.Barangay !== $s.PresentPersonaAddress.Barangay
            ];
            if (condi.includes(true))
                $s.SameAsPermanent = 0;
            //if (condi.every((val, i, arr) => val === arr[0]))
            //    $s.SameAsPermanent = 1;
        }, true);

        $s.LoadLookUp = function () {

            if ($s.IsPage0Clicked == 0) {

                $s.provinceLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Province' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: 'EmployeeRecord',
                    parameter: { LookupName: 'Province', MenuCode: 'PeopleEmployeeRecord' },
                    Filters: [{ Name: 'Province', Type: 9 }]
                };
                $s.cityLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'City' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: 'EmployeeRecord',
                    parameter: { LookupName: 'City', ID_Province: $s.PermanentPersonaAddress.ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                    Filters: [{ Name: 'City', Type: 9 }]
                };
                $s.barangayLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Barangay' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: 'EmployeeRecord',
                    parameter: { LookupName: 'Barangay', ID_City: $s.PermanentPersonaAddress.ID_City, MenuCode: 'PeopleEmployeeRecord' },
                    Filters: [{ Name: 'Barangay', Type: 9 }]
                };

                $s.provinceLookup2 = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Province' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: 'EmployeeRecord',
                    parameter: { LookupName: 'Province', MenuCode: 'PeopleEmployeeRecord' },
                    Filters: [{ Name: 'Province', Type: 9 }]
                };
                $s.cityLookup2 = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'City' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: 'EmployeeRecord',
                    parameter: { LookupName: 'City', ID_Province: $s.PresentPersonaAddress.ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                    Filters: [{ Name: 'City', Type: 9 }]
                };
                $s.barangayLookup2 = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Barangay' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: 'EmployeeRecord',
                    parameter: { LookupName: 'Barangay', ID_City: $s.PresentPersonaAddress.ID_City, MenuCode: 'PeopleEmployeeRecord' },
                    Filters: [{ Name: 'Barangay', Type: 9 }]
                };

                $s.Nationalitylookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Nationality' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'Nationality', MenuCode: $s.MenuCode }
                }

                $s.CitizenshipLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Citizenship' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'Citizenship', MenuCode: $s.MenuCode }
                }

                $s.CivilStatusLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'CivilStatus' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'CivilStatus', MenuCode: $s.MenuCode }
                }

                //$s.Request('LoadLookup', { LookupName: 'Gender', MenuCode: 'PeopleEmployeeRecord' }, 'EmployeeRecord').then(function (ret) {
                //    if (ret.Type == 2) {
                //        $s.SetSystemStatus(ret.Message, 'error');
                //    } else {
                //        $s.GenderList = ret.Data.Rows;
                //        $s.SetSystemStatus('Ready');
                //    }
                //    $s.$apply();
                //});

                $s.GenderLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Gender' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'Gender', MenuCode: $s.MenuCode }
                }

                $s.ReligionLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'Religion' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'Religion', MenuCode: $s.MenuCode }
                }

                $s.BloodTypeLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'BloodType' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'BloodType', MenuCode: $s.MenuCode }
                }

                $s.EducationalAttainmentLookup = {
                    tblOptions: {
                        Columns: [
                            { Name: 'ID', Label: '#' },
                            { Name: 'Name', Label: 'EducationalAttainment' }
                        ]
                    },
                    method: 'LoadLookup',
                    controller: $s.myController,
                    parameter: { LookupName: 'EducationalAttainment', MenuCode: $s.MenuCode }
                }

                //$s.cityLookup = {
                //    tblOptions: {
                //        Columns: [
                //            { Name: 'ID', Label: '#' },
                //            { Name: 'Name', Label: 'City' }
                //        ]
                //    },
                //    method: 'LoadLookup',
                //    controller: 'EmployeeRecord',
                //    parameter: { LookupName: 'City', ID_Province: $s.GetAddress(false).ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                //    Filters: [{ Name: 'City', Type: 9 }]
                //};
                //$s.barangayLookup = {
                //    tblOptions: {
                //        Columns: [
                //            { Name: 'ID', Label: '#' },
                //            { Name: 'Name', Label: 'Barangay' }
                //        ]
                //    },
                //    method: 'LoadLookup',
                //    controller: 'EmployeeRecord',
                //    parameter: { LookupName: 'Barangay', ID_City: $s.GetAddress(false).ID_City, MenuCode: 'PeopleEmployeeRecord' },
                //    Filters: [{ Name: 'Barangay', Type: 9 }]
                //};

                //$s.provinceLookup2 = {
                //    tblOptions: {
                //        Columns: [
                //            { Name: 'ID', Label: '#' },
                //            { Name: 'Name', Label: 'Province' }
                //        ]
                //    },
                //    method: 'LoadLookup',
                //    controller: 'EmployeeRecord',
                //    parameter: { LookupName: 'Province', MenuCode: 'PeopleEmployeeRecord' },
                //    Filters: [{ Name: 'Province', Type: 9 }]
                //};
                //$s.cityLookup2 = {
                //    tblOptions: {
                //        Columns: [
                //            { Name: 'ID', Label: '#' },
                //            { Name: 'Name', Label: 'City' }
                //        ]
                //    },
                //    method: 'LoadLookup',
                //    controller: 'EmployeeRecord',
                //    parameter: { LookupName: 'City', ID_Province: $s.GetAddress(true).ID_Province, MenuCode: 'PeopleEmployeeRecord' },
                //    Filters: [{ Name: 'City', Type: 9 }]
                //};
                //$s.barangayLookup2 = {
                //    tblOptions: {
                //        Columns: [
                //            { Name: 'ID', Label: '#' },
                //            { Name: 'Name', Label: 'Barangay' }
                //        ]
                //    },
                //    method: 'LoadLookup',
                //    controller: 'EmployeeRecord',
                //    parameter: { LookupName: 'Barangay', ID_City: $s.GetAddress(true).ID_City, MenuCode: 'PeopleEmployeeRecord' },
                //    Filters: [{ Name: 'Barangay', Type: 9 }]
                //};

                $s.IsPage0Clicked = 1;
            }

        }

        $s.UpdateCity = function (data, IsPresent) {
            if (IsPresent) $s.cityLookup2.parameter.ID_Province = data.ID;
            else $s.cityLookup.parameter.ID_Province = data.ID;

        }
        $s.UpdateBarangay = function (data, IsPresent) {
            if (IsPresent) $s.barangayLookup2.parameter.ID_City = data.ID;
            else $s.barangayLookup.parameter.ID_City = data.ID;
        }

        $s.Next = function () {
            var TabId = 'ew-tc-' + $s.currentEmpTab;
            //Validate Tab First
            //if ($s.currentEmpTab == 2) {
            //    if ($s.TableSchemaEducationalBackGround.length > 0 && (!$s.IsTabsValid('form.educationalbackground', $s.TableSchemaEducationalBackGround, TabId))) return;
            //} else if ($s.currentEmpTab == 3) {
            //    if ($s.TableSchemaCharacterReference.length > 0 && (!$s.IsTabsValid('form.educationalbackground', $s.TableSchemaCharacterReference, TabId))) return;
            //} else if ($s.currentEmpTab == 4) {
            //    if ($s.TableSchemaLicensesCertificates.length > 0 && (!$s.IsTabsValid('form.characterreference', $s.TableSchemaLicensesCertificates, TabId))) return;
            //} else if ($s.currentEmpTab == 5) {
            //    if ($s.TableSchemaPreviousEmployment.length > 0 && (!$s.IsTabsValid('form.licenseandcertificates', $s.TableSchemaPreviousEmployment, TabId))) return;
            //} else if ($s.currentEmpTab == 6) {
            //    if ($s.TableSchemaLicensesCertificates.length > 0 && (!$s.IsTabsValid('form.previousemployment', $s.TableSchemaLicensesCertificates, TabId))) return;
            //}

            if ($s.currentEmpTab < 6) {
                $s.currentEmpTab = $s.currentEmpTab + 1;
                $('.ew-tab-item').removeClass('active');
                $('.ew-tab-content').removeClass('active');
                $('#ew-tc-' + $s.currentEmpTab).addClass('active');
                $('#ew-ti-' + $s.currentEmpTab).addClass('active');

                $s.Request('LoadPersonaSchema', { LookupName: "tPersona", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                    console.log(ret);
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Schema = ret.Data.Schema;
                        $s.SetSystemStatus('Ready');
                    }
                });
                //Get Details Data.
                if ($s.IsPage2Clicked == 0) {
                    $s.Request('LoadPersonaSchema', { LookupName: "tPersonaAddress", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.TableSchemaPersonaAddress = ret.Data.Schema;
                            $s.SetupInputField($s.TableSchemaPersonaAddress);
                            $s.SetSystemStatus('Ready');
                        }
                    });
                    $s.IsPage2Clicked = 1;
                } else if ($s.IsPage3Clicked == 0) {
                    //LookUp
                    $s.Request('LoadLookUp', { LookupName: "educationalattainment", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.EducationalAttaintment = ret.Data.Rows;
                            $s.SetSystemStatus('Ready');
                        }
                    });
                    $s.Request('LoadPersonaSchema', { LookupName: "tPersonaEducationalBackGround", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.TableSchemaEducationalBackGround = ret.Data.Schema;
                            $s.SetupInputField($s.TableSchemaEducationalBackGround);
                            $s.SetSystemStatus('Ready');
                        }
                    });
                    $s.IsPage3Clicked = 1;
                } else if ($s.IsPage4Clicked == 0) {

                    $s.Request('LoadPersonaSchema', { LookupName: "tPersonaCharacterReference", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.TableSchemaCharacterReference = ret.Data.Schema;
                            $s.SetupInputField($s.TableSchemaCharacterReference);
                            $s.SetSystemStatus('Ready');
                        }
                    });
                    $s.IsPage4Clicked = 1;
                } else if ($s.IsPage5Clicked == 0) {

                    $s.Request('LoadPersonaSchema', { LookupName: "tPersonaLicensesAndCertificates", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.TableSchemaLicensesCertificates = ret.Data.Schema;
                            $s.SetupInputField($s.TableSchemaLicensesCertificates);
                            $s.SetSystemStatus('Ready');
                        }
                    });
                    $s.IsPage5Clicked = 1;
                } else if ($s.IsPage6Clicked == 0) {

                    $s.Request('LoadPersonaSchema', { LookupName: "tPersonaEmployment", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                        if (ret.Type == 2) {
                            $s.SetSystemStatus(ret.Message, 'error');
                        } else {
                            $s.TableSchemaPreviousEmployment = ret.Data.Schema;
                            $s.SetupInputField($s.TableSchemaPreviousEmployment);
                            $s.SetSystemStatus('Ready');
                        }
                    });
                    $s.IsPage6Clicked = 1;
                }
            } else {
                $s.SchemaPersonaAddress.push($s.PresentPersonaAddress); //bba
                $s.SchemaPersonaAddress.push($s.PermanentPersonaAddress); //bba
                PersonaData.PersonaAddress = $s.SchemaPersonaAddress; //bba
                PersonaData.Schema = $s.Schema;
                PersonaData.Persona = $s.tPersona;
                PersonaData.EducationalBackGround = $s.SchemaEducationalBackGround;
                PersonaData.CharacterReference = $s.SchemaCharacterReference;
                PersonaData.LicensesAndCertificates = $s.SchemaLicensesCertificates;
                PersonaData.PreviousEmployment = $s.SchemaPreviousEmployment;
                PersonaData.TableSchemaEducationalBackGround = $s.TableSchemaEducationalBackGround;
                PersonaData.TableSchemaCharacterReference = $s.TableSchemaCharacterReference;
                PersonaData.TableSchemaLicensesCertificates = $s.TableSchemaLicensesCertificates;
                PersonaData.TableSchemaPreviousEmployment = $s.TableSchemaPreviousEmployment;

                //console.log($s.RecordID);
                //console.log($s.tPersona);
                //console.log($s.SchemaPersonaAddress);
                //console.log($s.SchemaEducationalBackGround);
                //console.log($s.SchemaCharacterReference);
                //console.log($s.SchemaLicensesCertificates);
                //console.log($s.SchemaPreviousEmployment);
                // Final Posting

                $s.Request('SaveForm', {
                    ID: $s.RecordID, Data: $s.tPersona,
                    PersonaAddress: $s.SchemaPersonaAddress,
                    Educational: $s.SchemaEducationalBackGround,
                    CharacterReference: $s.SchemaCharacterReference,
                    LicensesCertificates: $s.SchemaLicensesCertificates,
                    Employment: $s.SchemaPreviousEmployment,
                    MenuCode: 'PeopleEmployeeRecord'
                }, 'EmployeeRecord').then(function (ret) {
                    console.log(ret);
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                        $s.$apply();
                    } else {
                        $s.Prompt('Save successful.');
                        $s.SetDirtyFormToFalse($s.form);
                        //$st.go($st.current.name, { ID: ret.Data }, { reload: true });
                        $mi.close(PersonaData);
                    }
                });

            }
        }

        $s.Prev = function () {
            $('.ew-body').scrollTop(0);
            if ($s.currentEmpTab > 0) {
                $s.currentEmpTab = $s.currentEmpTab - 1;
                $('.ew-tab-item').removeClass('active');
                $('.ew-tab-content').removeClass('active');
                $('#ew-tc-' + $s.currentEmpTab).addClass('active');
                $('#ew-ti-' + $s.currentEmpTab).addClass('active');
            }
        }

        $s.Init = function () {
            $s.SetSystemStatus('Loading record #' + $s.RecordID, 'loading');
            $s.Request('LoadPersonaSchema', { LookupName: "tPersona", MenuCode: $s.MenuCode }, $s.myController).then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    $s.Schema = $s.PlotDefault(ret.Data.Form, ret.Data.Schema, $s.RecordID);
                    $s.TableSchema = ret.Data.Schema;
                    $s.SetSystemStatus('Ready');
                }
                $s.$apply();
                if (ret.Type != 2)
                    $s.SetupInputField(ret.Data.Schema);
            })
            $s.LoadLookUp();
        }

        $s.close = function () {
            //Create vlidation that all changes will be cleared.
            $mi.close();
        }

        $s.AddressOldValues = {};

        $s.DoExist = function (ColumnName, num) { //pang disable
            if (ColumnName == null || typeof ColumnName === "undefined") {
                $s.NullWhenChanged(num);
            } else {
                switch (num) {
                    case 1:
                        if ($s.AddressOldValues.PermanentID_Province == null || typeof $s.AddressOldValues.PermanentID_Province === "undefined") { //if null to not null
                            $s.AddressOldValues.PermanentID_Province = ColumnName;
                        } else if ($s.AddressOldValues.PermanentID_Province != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PermanentID_Province = ColumnName;
                        }
                        console.log($s.AddressOldValues.PermanentID_Province, ColumnName);
                        break;
                    case 2:
                        if ($s.AddressOldValues.PermanentID_City == null || typeof $s.AddressOldValues.PermanentID_City === "undefined") { //if null to not null
                            $s.AddressOldValues.PermanentID_City = ColumnName;
                        } else if ($s.AddressOldValues.PermanentID_City != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PermanentID_City = ColumnName;
                        }
                        console.log($s.AddressOldValues.PermanentID_City, ColumnName);
                        break;
                    case 3:
                        if ($s.AddressOldValues.PresentID_Province == null || typeof $s.AddressOldValues.PresentID_Province === "undefined") { //if null to not null
                            $s.AddressOldValues.PresentID_Province = ColumnName;
                        } else if ($s.AddressOldValues.PresentID_Province != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PresentID_Province = ColumnName;
                        }
                        console.log($s.AddressOldValues.PresentID_Province, ColumnName);
                        break;
                    case 4:
                        if ($s.AddressOldValues.PresentID_City == null || typeof $s.AddressOldValues.PresentID_City === "undefined") { //if null to not null
                            $s.AddressOldValues.PresentID_City = ColumnName;
                        } else if ($s.AddressOldValues.PresentID_City != ColumnName) {
                            $s.NullWhenChanged(num);
                            $s.AddressOldValues.PresentID_City = ColumnName;
                        }
                        console.log($s.AddressOldValues.PresentID_City, ColumnName);
                        break;
                }
            }

            return (ColumnName == null || typeof ColumnName === "undefined") ? true : false; //pang disable
        }

        $s.NullWhenChanged = function (num) { // pang blank
            switch (num) {
                case 1:
                    $s.PermanentPersonaAddress.ID_City = null;
                    $s.PermanentPersonaAddress.City = null;
                    $s.PermanentPersonaAddress.ID_Barangay = null;
                    $s.PermanentPersonaAddress.Barangay = null;
                    $s.AddressOldValues.PermanentID_Province = null;
                    break;
                case 2:
                    $s.PermanentPersonaAddress.ID_Barangay = null;
                    $s.PermanentPersonaAddress.Barangay = null;
                    $s.AddressOldValues.PermanentID_City = null;
                    break;
                case 3:
                    $s.PresentPersonaAddress.ID_City = null;
                    $s.PresentPersonaAddress.ID_Barangay = null;
                    break;
                case 4:
                    $s.PresentPersonaAddress.ID_Barangay = null;
            }
        }

        $s.GetAddress = function (IsPresent) {
            return $s.PersonaAddress.filter(x => x.IsPresentAddress == IsPresent)[0];
            /*Enumerable.From($s.PersonaAddress).Where(function (x) { console.log(x); return (x.ColumnName == IsPresent) }).FirstOrDefault();*/
        }

        $s.GovBlur = function () {
            $s.tPersona.SSSNo = $s.format271($s.tPersona.SSSNo);
            $s.tPersona.PhilHealthNo = $s.format444($s.tPersona.PhilHealthNo);
            $s.tPersona.HDMFNo = $s.format444($s.tPersona.HDMFNo);
            $s.tPersona.TINNo = $s.format3333($s.tPersona.TINNo);
            $s.tPersona.GSISNo = $s.format11($s.tPersona.GSISNo);
        }

        $s.format271 = function (ColumnName) {
            var regex = RegExp(/^([\d]{2})-([\d]{7})-([\d]{1})$/);
            if (ColumnName) {
                if (ColumnName.length > 10 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.slice(0, 10);
                    ColumnName = ColumnName.replace(/^([\d]{2})([\d]{7})([\d]{1})$/, "$1-$2-$3");
                } else {
                    ColumnName = ColumnName.replace(/^([\d]{2})([\d]{7})([\d]{1})$/, "$1-$2-$3");
                }
            }
            return ColumnName;
        }
        $s.format444 = function (ColumnName) {
            var regex = RegExp(/^([\d]{4})-([\d]{4})-([\d]{4})$/);
            if (ColumnName) {
                if (ColumnName.length > 12 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.slice(0, 12);
                    ColumnName = ColumnName.replace(/^([\d]{4})([\d]{4})([\d]{4})$/, "$1-$2-$3");
                } else {
                    ColumnName = ColumnName.replace(/^([\d]{4})([\d]{4})([\d]{4})$/, "$1-$2-$3");
                }
            }
            return ColumnName;
        }
        $s.format3333 = function (ColumnName) {
            var regex = RegExp(/^([\d]{3})-([\d]{3})-([\d]{3})-([\d]{3})$/);
            if (ColumnName) {
                if (ColumnName.length > 12 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.slice(0, 12);
                    ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                    console.log(222222);
                } else {
                    ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                }
            }
            return ColumnName;
        }
        $s.format11 = function (ColumnName) {
            var regex = RegExp(/^([\d]{11})$/);
            if (ColumnName) {
                if (ColumnName.length > 11 && !regex.test(ColumnName)) {
                    ColumnName = ColumnName.slice(0, 11);
                    //ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                } else {
                    //ColumnName = ColumnName.replace(/^([\d]{3})([\d]{3})([\d]{3})([\d]{3})$/, "$1-$2-$3-$4");
                }
            }
            return ColumnName;
        }


        ///////////EDUCATIONAL BACKGROUD
        $s.SelectAllEducationalBackGround = false;
        $s.CheckAllDetails = function () {
            $s.SelectAllEducationalBackGround = !$s.SelectAllEducationalBackGround;
            Enumerable.From($s.SchemaEducationalBackGround).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllEducationalBackGround;
            });
        }


        $s.NewEducationBackGround = function () {
            var EducationBackGround = {
                ID: 0
                , ID_Persona: 0
                , ID_EducationAttainmentStatus: null
                , SchoolAttended: null
                , CourseDegree: null
                , IsActive: true
                , DatetimeCreated: new Date()
                , ID_UserCreatedBy: 0
                , From: null
                , To: null
            };
            $s.SchemaEducationalBackGround.push(EducationBackGround);
            $s.SetupInputField($s.TableSchemaEducationalBackGround);
        }

        $s.DelEducationBackGround = function () {
            var rows = Enumerable
                .From($s.SchemaEducationalBackGround)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray();

            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rows.length; y++) {
                        var Index = $s.SchemaEducationalBackGround.indexOf(rows[y]);
                        $s.SchemaEducationalBackGround.splice(Index, 1);
                    }
                    $s.SelectAllEducationalBackGround = 0;
                });
            }
        }

        ////////////////CHARACTER REFERENCE
        $s.SelectAllCharacterReference = false;
        $s.CheckAllCharReference = function () {
            $s.SelectAllCharacterReference = !$s.SelectAllCharacterReference;
            Enumerable.From($s.SchemaCharacterReference).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllCharacterReference;
            });
        }

        $s.NewCharReference = function () {
            var CharReference = {
                ID: 0
                , ID_Persona: 0
                , Name: null
                , Position: null
                , Company: null
                , ContactNo: null
                , IsActive: true
                , DatetimeCreated: new Date()
                , ID_UserCreatedBy: 0
            };

            $s.SchemaCharacterReference.push(CharReference);
        }

        $s.DelCharReference = function () {
            var rows = Enumerable
                .From($s.SchemaCharacterReference)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray();

            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rows.length; y++) {
                        var Index = $s.SchemaCharacterReference.indexOf(rows[y]);
                        $s.SchemaCharacterReference.splice(Index, 1);
                    }
                });
            }
        }

        ////////////////LICENSES AND CERTIFICATES
        $s.SelectAllLicensesAndCertificates = false;
        $s.CheckAllLicensesAndCertificates = function () {
            $s.SelectAllLicensesAndCertificates = !$s.SelectAllLicensesAndCertificates;
            Enumerable.From($s.SchemaLicensesCertificates).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllLicensesAndCertificates;
            });
        }

        $s.NewLicensesAndCertificates = function () {
            var LicensesAndCertificates = {
                ID: 0
                , ID_Persona: 0
                , Name: null
                , LicenseNo: null
                , Description: null
                , ValidityDate: null
                , IsActive: true
                , DatetimeCreated: new Date()
                , ID_UserCreatedBy: 0
            };

            $s.SchemaLicensesCertificates.push(LicensesAndCertificates);
        }

        $s.DelLicensesAndCertificates = function () {
            var rows = Enumerable
                .From($s.SchemaLicensesCertificates)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray();

            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rows.length; y++) {
                        var Index = $s.SchemaLicensesCertificates.indexOf(rows[y]);
                        $s.SchemaLicensesCertificates.splice(Index, 1);
                    }
                });
            }
        }

        ////////////////PREVIOUS EMPLOYMENT
        $s.SelectAllPreviousEmployment = false;
        $s.CheckAllPreviousEmployment = function () {
            $s.SelectAllPreviousEmployment = !$s.SelectAllPreviousEmployment;
            Enumerable.From($s.SchemaPreviousEmployment).ForEach(function (rowDetails) {
                rowDetails.IsChecked = $s.SelectAllPreviousEmployment;
            });
        }

        $s.NewPreviousEmployment = function () {
            var PreviousEmployment = {
                ID: 0
                , ID_Persona: 0
                , Company: null
                , Position: null
                , Address: null
                , From: null
                , To: null
                , Salary: null
                , ReasonForLeaving: null
                , IsActive: true
                , DatetimeCreated: new Date()
                , ID_UserCreatedBy: 0
            };

            $s.SchemaPreviousEmployment.push(PreviousEmployment);
        }

        $s.DelPreviousEmployment = function () {
            var rows = Enumerable
                .From($s.SchemaPreviousEmployment)
                .Where(function (x) { return x.IsChecked == true })
                .Select(function (x) { return x }).ToArray();

            if (rows.length > 0) {
                $s.Confirm('Are you sure you want to delete (' + rows.length + (rows.length > 1 ? ') records?' : ') record?'), 'Delete Record').then(function (r) {
                    for (var y = 0; y < rows.length; y++) {
                        var Index = $s.SchemaPreviousEmployment.indexOf(rows[y]);
                        $s.SchemaPreviousEmployment.splice(Index, 1);
                    }
                });
            }
        }
        $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();
    }]).controller('dlgPersonaSelect', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('insysTableController', { $scope: $s });

        $s.MenuCode = 'PeopleEmployeeRecord';
        $s.myController = 'EmployeeRecord';

        $s.PersonaWithoutEmploymentList = [];
        $s.Schema = {};
        $s.IsSelectEmployee = false;

        $s.PersonaLookUp = {
            tblOptions: {
                Columns: [
                    { Name: 'ID', Label: '#' }
                    , { Name: 'Name', Label: 'Employee' }
                    , { Name: 'FirstName', Label: 'Firstname' }
                    , { Name: 'LastName', Label: 'Lastname' }
                ]
            },
            method: 'LoadPersonaWithoutEmployment',
            controller: $s.myController,
            parameter: { MenuCode: $s.MenuCode }
        };

        $s.getSchema = function (ret) {
            $s.Schema = ret;
        }

        $s.Select = function () {
            $mi.close($s.Schema);
        }

        $s.CreateNew = function () {
            $mi.close(0);
        }

        $s.close = function () {
            $mi.close(-1);
        }

    }]);