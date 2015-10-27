//--------------------------------------------------AngularJS--------------------------------------------------
var LinkInterventionApp = angular.module('LinkInterventionApp', []);

LinkInterventionApp.controller('LinkInterventionController', function ($scope, $http) {
    getInterventions();
    getParticipantGroups();
    function getInterventions() {
        $http.get('/Administrator/GetInterventions').
          then(function (results) {
              $scope.interventions = results.data;
          }, function (error) {
          });
    }

    function getParticipantGroups() {
        $http.get('/Administrator/GetParticipantGroups').
          then(function (results) {
              $scope.participantGroups = results.data;
          }, function (error) {
          });
    }

    $scope.submit = function () {
        console.log($scope.selected);
        var obj = { InterventionId: $scope.selected.intervention.Id, ParticipantGroupId: $scope.selected.participantGroup.Id };
        $http.post('/Administrator/LinkInterventionToParticipantGroup', JSON.stringify(obj)).then(function (response) {
            if (response.data == "success") {
                //Get reporting data from server and structure it into PDF
                alert("successfully linked intervention to participant group");
            } else {
                alert("Failed to link!");
            }
        }, function (error) {
        });
    }
});
//-------------------------------------------------------------------------------------------------------------