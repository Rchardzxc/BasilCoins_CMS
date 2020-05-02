angular.module('app')
    .config(['$stateProvider', '$urlRouterProvider',
        function ($stateProvider, $urlRouterProvider) {
            var c = localStorage.getItem("HtmlTemplatePath") + "/Web/Controllers/",
                v = localStorage.getItem("HtmlTemplatePath") + "/Web/Views/";

            $stateProvider.state({
                name: 'Dashboard',
                url: '/Dashboard',
                controller: 'Dashboard',
                templateUrl: v + 'Dashboard.html',
                resolve: {}
            });
            $urlRouterProvider.otherwise('/');
        }]);