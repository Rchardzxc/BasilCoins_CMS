angular.module('app')
    .config(['$stateProvider', '$urlRouterProvider',
        function ($stateProvider, $urlRouterProvider) {
            var c = localStorage.getItem("HtmlTemplatePath") + "/Web/Controllers/",
                v = localStorage.getItem("HtmlTemplatePath") + "/Web/Views/";

            $stateProvider.state({
                name: 'IONSDashboard',
                url: '/',
                controller: 'IONSDashboard',
                templateUrl: v + 'IONSDashboard.html',
                resolve: {}
            });
            $urlRouterProvider.otherwise('/');
        }]);