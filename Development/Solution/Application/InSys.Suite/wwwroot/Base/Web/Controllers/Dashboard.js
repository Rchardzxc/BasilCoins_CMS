angular.module('app')
    .controller('Dashboard', ['$scope', '$controller', function ($s, $c) {
        $c('BaseController', { $scope: $s });
        $s.ToDo = {};
        $s.ToInterview = [];
        $s.ToExam = [];

        $s.UpdateToDo = function () {
            $s.ToInterview = Enumerable.From($s.ToDo.ToInterview).Where(function (x) { return x.IsDone == false }).ToArray();
            $s.ToExam = Enumerable.From($s.ToDo.ToExam).Where(function (x) { return x.IsDone == false }).ToArray();
        }

        //$s.Request('LoadToDo', {}, 'Dashboard').then(function (ret) {
        //    if (ret.Type == 2) {
        //        $s.SetSystemStatus(ret.Message, 'error');
        //    } else {
        //        $s.ToDo = ret.Data;
        //        $s.UpdateToDo();
        //    }
        //    $s.$apply();
        //});
        $s.GetApplicant = function (id) {
            return Enumerable.From($s.ToDo.Applicant).Where(function (x) { return x.ID == id }).FirstOrDefault();
        }
        $s.OpenInterview = function (ID) {
            $s.Request('CheckInterview', { ID: ID }, 'Dashboard').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    var applicant = $s.GetApplicant(ret.Data.ID_ApplicationForm)
                    if (ret.Data.IsDone == true) {
                        $s.Prompt('You have already submitted the result for this Interview Schedule.');
                    } else {
                        $s.Dialog({
                            template: 'InterviewerApplicantUI',
                            controller: 'dlgInterviewerApplicantUI',
                            size: 'md',
                            animation: false,
                            windowClass: 'for-interview-dlg',
                            data: { Applicant: applicant, Interview: ret.Data }
                        }).result.then(function () {
                            $s.UpdateToDo();
                        });
                    }
                }
            });
        }
        $s.OpenExam = function (ID) {
            $s.Request('CheckExam', { ID: ID }, 'Dashboard').then(function (ret) {
                if (ret.Type == 2) {
                    $s.SetSystemStatus(ret.Message, 'error');
                } else {
                    var applicant = $s.GetApplicant(ret.Data.ID_ApplicationForm)
                    if (ret.Data.IsDone == true) {
                        $s.Prompt('You have already submitted the result for this Exam Schedule.');
                    } else {
                        $s.Dialog({
                            template: 'ExaminerApplicantUI',
                            controller: 'dlgExaminerApplicantUI',
                            size: 'md',
                            animation: false,
                            windowClass: 'for-exam-dlg',
                            data: { Applicant: applicant, Exam: ret.Data }
                        }).result.then(function () {
                            $s.UpdateToDo();
                        });
                    }
                }
            });
        }

        //ResumeBankAndSearchInterview
        $s.NotificationOnClickEvent[1] = {
            Trigger: function (row) {
                $s.OpenInterview(row.ReferenceID);
            }
        };
        //ResumeBankAndSearchExam
        $s.NotificationOnClickEvent[2] = {
            Trigger: function (row) {
                $s.OpenExam(row.ReferenceID);
            }
        };
    }])
    .controller('dlgInterviewerApplicantUI', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });
        $s.Interview = $dlgData.Interview;
        $s.Applicant = $dlgData.Applicant;
        $s.ExamAndInterviewStatus = [];

        $s.Request('LoadResumeLookUp', { Name: 'ExaminationAndInterviewStatus' }, 'GetResumeLookUp').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message, 'error');
            } else {
                $s.ExamAndInterviewStatus = ret.Data.LookUpData;
            }
            $s.$apply();
        });

        $s.submitInterview = function () {

            //Do not allow status "Pending" to be submitted.
            if ($s.Interview.ID_ExamAndInterviewStatus == 3) {
                $s.Prompt("Status 'Pending' cannot be submitted.");
                return;
            }

            $s.Confirm('Once submitted the result for this interview could not be change. Are you sure you want to proceed?').then(function () {
                $s.Request('SaveInterview', { Data: $s.Interview }, 'Dashboard').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Interview.IsDone = true;
                        $s.Prompt('Result submitted successfully!').then(function () {
                            $mi.close();
                        })
                    }
                });
            });
        }
        $s.close = function () {
            $mi.close();
        }
    }])
    .controller('dlgExaminerApplicantUI', ['$scope', '$uibModalInstance', '$controller', 'dData', function ($s, $mi, $c, $dlgData) {
        $c('BaseController', { $scope: $s });
        $s.Exam = $dlgData.Exam;
        $s.Applicant = $dlgData.Applicant;
        $s.ExamAndInterviewStatus = [];

        $s.Request('LoadResumeLookUp', { Name: 'ExaminationAndInterviewStatus' }, 'GetResumeLookUp').then(function (ret) {
            if (ret.Type == 2) {
                $s.SetSystemStatus(ret.Message, 'error');
            } else {
                $s.ExamAndInterviewStatus = ret.Data.LookUpData;
            }
            $s.$apply();
        });

        $s.submitExam = function () {

            //Do not allow status "Pending" to be submitted.
            if ($s.Exam.ID_ExamAndInterviewStatus == 3) {
                $s.Prompt("Status 'Pending' cannot be submitted.");
                return;
            }

            $s.Confirm('Once submitted the result for this exam could not be change. Are you sure you want to proceed?').then(function () {
                $s.Request('SaveExam', { Data: $s.Exam }, 'Dashboard').then(function (ret) {
                    if (ret.Type == 2) {
                        $s.SetSystemStatus(ret.Message, 'error');
                    } else {
                        $s.Exam.IsDone = true;
                        $s.Prompt('Result submitted successfully!').then(function () {
                            $mi.close();
                        })
                    }
                });
            });
        }
        $s.close = function () {
            $mi.close();
        }
    }])