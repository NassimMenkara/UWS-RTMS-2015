window.onload = function () {
    $(function () {
        $("#startDate").datepicker();
        $("#endDate").datepicker();
        $("#startDate").datepicker("option", "dateFormat", "dd/mm/yy");
        $("#endDate").datepicker("option", "dateFormat", "dd/mm/yy");

    });
}

//--------------------------------------------------AngularJS--------------------------------------------------
var InterventionResultsApp = angular.module('ReportingApp', []);

InterventionResultsApp.controller('ReportingController', function ($scope, $http) {
    $scope.Report = {};
    $scope.Report.FilterSelection = 1;
    getInterventions();
    $scope.generatePDF = function () {
        $scope.Report.StartDate = document.getElementById('startDate').value;
        $scope.Report.EndDate = document.getElementById('endDate').value;
        console.log($scope.Report);
        $http.post('/Administrator/GenerateReport', JSON.stringify($scope.Report)).then(function (response) {
            if (response.data != "fail") {
                //Get reporting data from server and structure it into PDF
                console.log(JSON.stringify(response.data));
                var tests = response.data;
                var doc = new jsPDF('p', 'pt');
                doc.setFont("helvetica");
                doc.setFontType("bold");
                doc.setFontSize(30);
                doc.text(60, 60, 'RTMS - Intervention Reporting');
                doc.setFontSize(15);
                doc.text(60, 90, 'This report was generated for the following intervention: ');
                var w = doc.getStringUnitWidth('This report was generated for the following intervention: ') * 15; // Where 12 is the chosen font size
                console.log("hey", w); // 21.480000000000004
                doc.setFontSize(20);
                var index = getInterventionIndex($scope.Report.Intervention);
                doc.text(60, 120, $scope.interventions[index].Name);
                doc.setFontType("normal");
                doc.text(60, 150, doc.splitTextToSize("Description: " + $scope.interventions[index].Description, 500));
                doc.setFontType("bold");
                doc.text(60, 280, "Table of Contents:");
                doc.setFontType("normal");
                var y = 280;
                for (var i = 0; i < tests.length; i++) {
                    y += 30;
                    doc.text(60, y, "> " + tests[i].Name);
                    doc.text(480, y, "(" + (i + 1) + ")");
                }
                doc.text(60, 800, "Page: 1");
                //doc.text(20, 40, JSON.stringify(response.data));
                for (var i = 0; i < tests.length; i++) {
                    doc.addPage();
                    doc.setFontType("bold");
                    doc.setFontSize(30);
                    doc.text(60, 60, tests[i].Name);
                    doc.setFontType("normal");
                    doc.setFontSize(20);
                    doc.text(60, 90, "ID: " + tests[i].Id);
                    doc.text(60, 120, doc.splitTextToSize("Description: " + tests[i].Description, 500));
                    doc.text(60, 210, "Completion rate: " + tests[i].CompletionCount);
                    doc.text(60, 800, "Page: " + (i + 2));
                }
                doc.addPage();
                var splitTitle = doc.splitTextToSize("Hey this is a description how are you today mate ive been waiting ages for this, matey?", 500);
                doc.text(60, 60, splitTitle);
                document.getElementById('pdfPreview').style.display = "block";
                document.getElementById('pdfPreview').src = doc.output('datauristring');
            } else {
                alert("Failed: all fields are required")
            }
                

        }, function (error) {
        });

    }

    function getInterventions() {
        $http.get('/Administrator/GetInterventions').
          then(function (results) {
              var interventions = results.data;
              $scope.interventions = interventions;
              var select = document.getElementById("interventionSelect");
              for (var i = 0; i < interventions.length; i++) {
                  var option = document.createElement("option");
                  option.text = "Id:" + interventions[i].Id + " Name:" + interventions[i].Name;
                  option.value = interventions[i].Id;
                  select.appendChild(option);
              }
          }, function (error) {
          });
    }

    $scope.interventionSelected = function () {
        var i = getInterventionIndex($scope.Report.Intervention);
        document.getElementById('intervention').innerHTML = "<b>Id:</b>" + $scope.interventions[i].Id + "<br> <b>Name:</b>" + $scope.interventions[i].Name + "<br> <b>Description:</b>" + $scope.interventions[i].Description;
    }

    $scope.radioSelected = function (i) {
        if ($scope.Report.FilterSelection == i)
            return "radio-div-selected";
        else
            return "radio-div";
    }

    $scope.setRadio = function (i) {
        $scope.Report.FilterSelection = i;
    }

    function getInterventionIndex(id) {
        for(var i = 0; i < $scope.interventions.length; i++){
            if ($scope.interventions[i].Id == id)
                return i;
        }
    }
});
//-------------------------------------------------------------------------------------------------------------