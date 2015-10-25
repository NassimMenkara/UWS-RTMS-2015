//--------------------------------------------------AngularJS--------------------------------------------------
var ExternalTestApp = angular.module('ExternalTestApp', []);

ExternalTestApp.controller('ExternalTestController', function ($scope, $http) {

    //sends retrieve request using HTTP POST request
    $scope.retrieveRequest = function (trialId, testId) {
        var testObj = { testDetails: {}, testAnswers: {} };
        var testDetails = { Trial_Id: trialId, Intervention_Area_Test_Id: testId };
        testObj.testDetails = testDetails;
        testObj.testAnswers = $scope.test;
        $http.post('/Participant/ExternalTestResultRetrieval?trialId=' + trialId + '&testId=' + testId).
            then(function (response) {
                if (response.data == "Result retrieval succeeded") {
                    alert("Successfully retrieved results from External test provider!");
                } else {
                    alert("Failed!");
                }
            }, function (error) {
            });
    };
});
//-------------------------------------------------------------------------------------------------------------