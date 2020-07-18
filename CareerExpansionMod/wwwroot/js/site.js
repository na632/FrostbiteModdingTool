function FormatCurrencyNumber(num)
{
    num = parseFloat(num);
    num = num.toFixed(2);

    if (num >= 1000000) {

        return "£" + (num / 1000000).toFixed(2) + "M";

    }
    else if (num >= 100000)
        return FormatCurrencyNumber(num / 1000);
    else if (num >= 10000) {
        return "£" + (num / 1000).toFixed(2) + "K";
    }
    return "£" + num;
}


//var app = angular.module('InfiniteLearning', ['ui.router']);

//app.config(function ($stateProvider, $urlRouterProvider) {

//    // For any unmatched URL redirect to dashboard URL
//    $urlRouterProvider.otherwise("YouthSystem");

//    $stateProvider

//        .state('YouthSystem', {
//            url: "/YouthSystem",
//            template: "<h1>Fuck you!</h1>"
//        })
//        .state('YouthSystem.Team', {
//            url: "/Team",
//            template: "<h1>Fuck you Team!</h1>"

//        })

//        // --------------------------------
//        // Finances


//        .state('Finances', {
//            url: "/Finances",
//            templateUrl: "YouthSystem"
//        })
//        .state('Finances.Prices', {
//            url: "/Prices",
//            templateUrl: "Finances/Prices"
//        })


//        ;
//});

var app_finances = angular.module('Finances', ['ui.router']);
var app_youth_system = angular.module('YouthSystem', ['ui.router']);

//app.config(function ($stateProvider, $urlRouterProvider) {

//    // For any unmatched URL redirect to dashboard URL
//    $urlRouterProvider.otherwise("YouthSystem");

//    $stateProvider

//        .state('YouthSystem', {
//            url: "/YouthSystem",
//            template: "<h1>Fuck you!</h1>"
//        })
//        .state('YouthSystem.Team', {
//            url: "/Team",
//            template: "<h1>Fuck you Team!</h1>"

//        })

//        // --------------------------------
//        // Finances


//        .state('Finances', {
//            url: "/Finances",
//            templateUrl: "YouthSystem"
//        })
//        .state('Finances.Prices', {
//            url: "/Prices",
//            templateUrl: "Finances/Prices"
//        })


//        ;
//});
var app = angular.module("myApp", ["ngRoute", "ui.bootstrap"]);
app.config(function ($routeProvider) {
    $routeProvider
        .when("/Home", {
            templateUrl: "/Home/Roadmap"
        })
        .when("/Finances", {
            templateUrl: "/Finances/Index/"
        })
        .when("/YouthSystem", {
            templateUrl: "/YouthSystem"
        })
        .when("/Squad", {
            templateUrl: "/Squad/Squad"
        })
        .when("/Transfers", {
            templateUrl: "/Transfers/Index"
        })
        .when("/Settings", {
            templateUrl: "/Settings/Index"
        })
        .when("/FinancesLoansAndDebts", {
            templateUrl: "/Finances/LoansAndDebts"
        })
        .when("/Finances/Sponsors", {
            templateUrl: "/Finances/Sponsors"
        })
        .when("/Finances/Prices", {
            templateUrl: "/Finances/Prices"
        })
        .when("/Transfers/ManualTransfer", {
            templateUrl: "/Transfers/ManualTransfer"
        })
        .otherwise({ redirectTo: "/Home" })
});


function Notify(user, message) {
    $.notify(message);
}

var ctrlLoanAndDebts = app.controller('ctrlLoanAndDebts', ['$scope', function ($scope) {

    $scope.IsLoanAvailable = true;
    $.getJSON("Finances/IsLoanAvailable", function (d) {
        $scope.IsLoanAvailable = true;
    });
    $scope.AcceptLoan = function (cpm, months) {

        $.post("Finances/AcceptLoan", null, function (d) {
            $scope.IsLoanAvailable = false;
            $scope.apply();



        }).fail(function (a, b, c) {


        });


        $.notify('Unable to Accept Loans in this Version');
        //$.notify('accepted ' + cpm + ' for ' + months + ' months');

    };

}]);


