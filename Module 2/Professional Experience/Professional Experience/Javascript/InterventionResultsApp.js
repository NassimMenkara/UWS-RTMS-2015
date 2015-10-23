onStart();

function onStart()
{
    // displays divs on start
    var div = document.getElementById('main');
    var divs = div.getElementsByTagName('div');
    for (var i = 0; i < divs.length; i++) {
        divs[i].style.display = "block";
    }
}
//--------------------------------------------------AngularJS--------------------------------------------------
var InterventionResultsApp = angular.module('InterventionResultsApp', []);

InterventionResultsApp.controller('InterventionResultsController', function ($scope, $http) {
    $scope.index = 0;

    // compares current index with div's index to determine if div should show
    $scope.check = function (i) {
        if ($scope.index == i)
            return true;
        else
            return false;
    };

    // sets page index
    $scope.setIndex = function (i) {
        $scope.index = i;
    };

    // set active class for li
    $scope.setActive = function (i) {
        if ($scope.index == i) {
            return "active";
        } else {
            return "";
        }
    }
});
//-------------------------------------------------------------------------------------------------------------