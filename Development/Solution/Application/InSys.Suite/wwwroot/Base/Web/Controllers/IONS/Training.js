angular.module('app')
	.controller('IONSTraining', ['$scope', '$controller', '$priv', '$state', function ($s, $c, $priv, $st) {
	    $c('insysTableController', { $scope: $s });

	    $s.MenuPrivileges = $priv.Data;
	    $s.RecordID = $st.params.ID == undefined ? '' : $st.params.ID;
        $s.Schema = {};
        $s.TableSchema = [];

        //FOR TABS
	    $s.TabsCount = [0, 0];

	    $s.tblOptions = {
	        Columns: [
                //{ Name: 'ID', Label: '#' },
                { Name: 'Title' },
                { Name: 'ReferenceNumber', Label: 'Reference #' },
                { Name: 'Company' },
	        ],
	        HasNew: $s.MenuPrivileges.HasNew,
            HasDelete: $s.MenuPrivileges.HasDelete,
            HasEdit: $s.MenuPrivileges.HasEdit,
	        Filters: [
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
	            $s.LoadTable($s.tblOptions, 'LoadList', 'Training').then(function (ret) {
	                if (ret.Type == 2) {
	                    $s.SetSystemStatus(ret.Message);
	                } else {
	                    $s.SetSystemStatus('Ready');
                        $s.TabsCount = [ret.Data.Total, ret.Data.TrainingApprovalCount];
	                    $s.InitTabs();
	                }
	                $s.$apply();
	            });
	        }
	    }

	    $s.LoadForm = function () {
	        return $s.Request('LoadForm', { ID: $s.RecordID, ID_Employee: $st.params.ID_Employee || $s.Session('ID_Employee') }, 'Training');
	    }

        $s.saveForm = function () {
            if ($s.IsTabsValid('form.training')) {
                if (!$s.tblOptions.HasEdit && $s.RecordID > 0) {
                    $s.Prompt('You are not allowed to update this record.');
                    return;
                }
                $s.SetSystemStatus('Saving record #' + $s.RecordID, 'loading');
                $s.Request('SaveForm', { Data: $s.Schema }, 'Training').then(function (ret) {
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
	        
	    }

	    $s.saveTrainingStatus = function (status) {
	        if (!status) {
	            if ($s.Schema.ReasonForNotAttending == null || $s.Schema.ReasonForNotAttending == undefined || $s.Schema.ReasonForNotAttending == '') {
	                $s.Prompt('Please write a reason for not attending on the Others tab.');
	                return;
	            }
	            $s.Confirm('Are you sure you want to discard this Training Application?').then(function (response) {
	                $s.Request('SaveForm', { Data: $s.Schema, Status: status }, 'Training').then(function (ret) {
	                    if (ret.Type == 2) {
	                        $s.SetSystemStatus(ret.Message, 'error');
	                        $s.$apply();
	                    } else {
	                        $s.Prompt('Training Application discarded.');
	                        $st.go($st.$current.parent.self.name, { reload: true });
	                    }
	                })
	            })
	        } else {
                $s.Request('SaveForm', { Data: $s.Schema, Status: status }, 'Training').then(function (ret) {
	                if (ret.Type == 2) {
	                    $s.SetSystemStatus(ret.Message, 'error');
	                    $s.$apply();
	                } else {
	                    $s.Prompt('You have successfully accepted the Training Application. Please wait for the approval process.');
	                    $st.reload();
	                }
	            })
	        }
	    }

	    $s.delete = function () {
	        if ($s.MenuPrivileges.HasDelete != true) {
	            $s.SetSystemStatus('You have no privilege to delete this record.', 'warning');
	            return;
	        }
	        $s.deleteRow('DeleteRecord', 'Training')
	    }

	    $s.SetSystemStatus('Loading table', 'loading');
        $s.Init();

	    //tabs
	    $s.Tabs = [];
	    $s.CurrentTab = 0;
	    $s.InitTabs = function () {
            //applicationTab
	        $s.Tabs.push(angular.copy($s.tblOptions));
	        //approvalTab
	        var approvalTab = {
	            Columns: [
                    { Name: 'Employee' },
                    { Name: 'Title' },
                    { Name: 'ReferenceNumber', Label: 'Reference #' },
                    { Name: 'Company' },
	            ],
	            HasNew: $s.MenuPrivileges.HasNew,
	            HasDelete: $s.MenuPrivileges.HasDelete,
	            Filters: [
	            ],
	            openForm: function (row) {
	                $st.go($st.current.name + '.ApprovalForm', { ID: row.ID, ID_Employee: row.ID_Employee }, { reload: true })
	            }
	        };
	        $s.Tabs.push(approvalTab);
	    }
	    $s.changeTab = function (idx) {
	        $('.lt-item').removeClass('active');
	        $('#lt-' + idx).addClass('active');
	        $('.content-tab').removeClass('active');
	        $('#ct-' + idx).addClass('active');
	        $('.dp-item').removeClass('active');
	        $('#dp-lt-' + idx).addClass('active');
	        $('#d-lt-' + idx).addClass('active');
	        $s.CurrentTab = idx;
	        $s.LoadTable($s.Tabs[idx], (idx == 1 ? 'LoadTrainingApprovals' : 'LoadList'), 'Training').then(function (ret) {
	            if (ret.Type == 2) {
	                $s.SetSystemStatus(ret.Message, 'error');
	            } else {
	                $s.SetSystemStatus('Ready');
	                $s.Tabs[idx] = angular.copy($s.tblOptions);
	                $s.TabsCount = ret.Data.TabsCount;
	            }
	            $s.$apply();
	        });
	    }
	    $s.Approve = function () {
	        $s.Request('Approve', { data: $s.Schema }, 'Training').then(function (ret) {
	            if (ret.Type == 2) {
	                $s.SetSystemStatus(ret.Message, 'error');
	            } else {
	                $st.go($st.$current.parent.name, { ID: '' }, { reload: true });
	            }
	            $s.$apply();
	        })
	    }
	    $s.Disapprove = function () {
	        if ($s.Schema.ReasonForDisapproving == undefined || $s.Schema.ReasonForDisapproving == null || $s.Schema.ReasonForDisapproving == '') {
	            $s.Prompt('Please write a reason for disapproving on the Others tab.');
	            return;
	        }
	        $s.Confirm('Are you sure you want to disapprove this record?').then(function () {
	            $s.Request('Disapprove', { data: $s.Schema, ReasonForDisapproving: $s.Schema.ReasonForDisapproving }, 'Training').then(function (ret) {
	                if (ret.Type == 2) {
	                    $s.SetSystemStatus(ret.Message, 'error');
	                } else {

	                    $st.go($st.$current.parent.name, { ID: '' }, { reload: true });
	                }
	                $s.$apply();
	            })
	        })
	    }
	    $s.ApprovalHistory = [];
	    $s.getApprovalHistory = function (ID_Menus, ReferenceID, TargetEmployee) {
	        if ($s.ApprovalHistory.length == 0)
	            $s.Request('LoadApprovalHistory', { ID_Menus: ID_Menus, ID: ReferenceID, Employee: TargetEmployee }, 'Training').then(function (ret) {
	                if (ret.Type == 2) {
	                    $s.SetSystemStatus(ret.Message, 'error');
	                } else {
	                    $s.ApprovalHistory = ret.Data;
	                }
	                $s.$apply();
	            });
	    }
	}]);