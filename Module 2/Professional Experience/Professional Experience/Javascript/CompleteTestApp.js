var pageCount = 0;
onStart();

function onStart() {
    // on start loops through all divs inside main div and sets them to be displayed
    var div = document.getElementById('pages');
    var divs = div.getElementsByTagName('div');
    for (var i = 0; i < divs.length; i++) {
        if (divs[i].id.substr(0, 4) == 'page') {
            pageCount++;
            divs[i].style.display = "block";
        }
    }
    console.log(pageCount);
}

//--------------------------------------------------AngularJS--------------------------------------------------
var CompleteTestApp = angular.module('CompleteTestApp', []);

CompleteTestApp.controller('CompleteTestController', function ($scope, $http) {
    $scope.test = {};
    $scope.multis = {};
    $scope.answers = [];
    $scope.index = 1;
    $scope.submitted = false;

    window.onbeforeunload = function () {
        //if the form has been changed, prompt user to confirm leaving from page
        if (JSON.stringify($scope.test) != '{}' && !$scope.submitted) {
            return "Are you sure you wish to leave this test? your answers will be lost!";
        }
    }

    // compares current index with div's index to determine if div should show
    $scope.check = function (i) {
        if ($scope.index == i)
            return true;
        else
            return false;
    };

    // sets page index
    $scope.setIndex = function (i) {
        if(i != 0 && i <= pageCount)
            $scope.index = i;
    };

    // sets paging button to active
    $scope.isActive = function (i) {
        if (i == $scope.index)
            return "active";
    }
    
    // converts angular generated objects into array
    $scope.objToArr = function (obj, objId) {
        $scope.answers.length = 0;
        var keys = Object.keys(obj);
        for (var i = 0; i < keys.length; i++) {
            var val = obj[keys[i]];
            if (val != undefined) {
                $scope.answers.push(val);
            }
            $scope.test[objId] = $scope.answers.slice();
        }
    }

    //submits test to server using HTTP POST request
    $scope.submitTest = function (trialId, testId) {
        var testObj = {testDetails:{}, testAnswers:{}};
        var testDetails = {Trial_Id: trialId, Intervention_Area_Test_Id: testId};
        testObj.testDetails = testDetails;
        testObj.testAnswers = $scope.test;
        $http.post('/Participant/SubmitTest', JSON.stringify(testObj)).
            then(function (response) {
                if (response.data == "success") {
                    alert("Successfully completed test!");
                    $scope.submitted = true;
                    var oldURL = document.referrer;
                    location.href = oldURL;
                } else {
                    alert("failed");
                }
            }, function (error) {
        });
    };

    $scope.checkFieldValidity = function (field, type) {
        if (field != undefined)
            if(type == 'color'){
                return 'input-group has-success has-feedback';
            } else {
                return 'glyphicon glyphicon-ok form-control-feedback';
            }
        else
            return '';
    }
});
//-------------------------------------------------------------------------------------------------------------