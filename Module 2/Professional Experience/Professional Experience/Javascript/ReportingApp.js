window.onload = function () {
    $(function () {
        $("#startDate").datepicker();
        $("#endDate").datepicker();
        $("#startDate").datepicker("option", "dateFormat", "dd/mm/yy");
        $("#endDate").datepicker("option", "dateFormat", "dd/mm/yy");

    });
}
document.getElementById("startDate").style.cursor = "pointer";
document.getElementById("endDate").style.cursor = "pointer";



function selected() {
    alert($('#interventionSelect').val());
    var scope = angular.element($("#outer")).scope();
    scope.$apply(function () {
        scope.Report.intervention = scope.interventions[$('#interventionSelect').val()];

    });
}

//--------------------------------------------------AngularJS--------------------------------------------------
var ReportingApp = angular.module('ReportingApp', []);
angular.module('yourModule', []);

ReportingApp.controller('ReportingController', function ($scope, $http) {
    var y = 0;
    var pages = 0;
    $scope.interventions = [];
    $scope.Report = {};
    $scope.Report.FilterSelection = 1;
    getInterventions();
    $scope.generatePDF = function () {
        console.log($scope.Report);
        $http.post('/Administrator/GenerateReport', JSON.stringify($scope.Report)).then(function (response) {
            if (response.data != "fail") {
                //Get reporting data from server and structure it into PDF
                console.log(response.data);
                var tests = response.data;
                console.log(JSON.stringify(tests[0].Questions[0].Answers));
                var doc = createPDF(tests);
                document.getElementById('pdfPreview').style.display = "block";
                document.getElementById('pdfPreview').src = doc.output('datauristring');
            } else {
                alert("Failed: all fields are required")
            }
                

        }, function (error) {
        });
    }

    $scope.downloadPDF = function () {
        console.log($scope.Report);
        $http.post('/Administrator/GenerateReport', JSON.stringify($scope.Report)).then(function (response) {
            if (response.data != "fail") {
                //Get reporting data from server and structure it into PDF
                console.log(response.data);
                var tests = response.data;
                console.log(JSON.stringify(tests[0].Questions[0].Answers));
                var doc = createPDF(tests);
                //document.getElementById('pdfPreview').style.display = "block";
                doc.output('save', 'RTMS-Intervention-Report.pdf');
                //document.getElementById('pdfPreview').src = doc.output('datauristring');
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
                 // $('#interventionSelect').append("<option value='" + interventions[i].Id + "'>Id:" + interventions[i].Id + " Name:" + interventions[i].Name + "</option>");
              }
              
          }, function (error) {
          });
    }

    $scope.interventionSelected = function () {
        document.getElementById('intervention').innerHTML = "<b>Id:</b>" + $scope.Report.Intervention.Id + "<br> <b>Name:</b>" + $scope.Report.Intervention.Name + "<br> <b>Description:</b>" + $scope.Report.Intervention.Description;
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

    $scope.validateForm = function () {
        var invalidForm = false;
        if ($scope.Report.FilterSelection == 1) { //All
            if ($scope.Report.Intervention == undefined || $scope.Report.Intervention == "")
                invalidForm = true;
        } else { //DateRange
            if ($scope.Report.Intervention == undefined || $scope.Report.Intervention == "")
                invalidForm = true;
            if (document.getElementById('startDate').value == "" || document.getElementById('endDate').value == "")
                invalidForm = true;
        }
        return invalidForm;
    }

    function getInterventionIndex(id) {
        for(var i = 0; i < $scope.interventions.length; i++){
            if ($scope.interventions[i].Id == id)
                return i;
        }
    }

    function createPDF(tests) {
        pages = 1;
        var doc = new jsPDF('p', 'pt');
        doc.setFont("helvetica");
        doc.setFontType("bold");
        doc.setFontSize(30);
        doc.text(60, 60, 'RTMS - Intervention Reporting');
        doc.setFontSize(15);
        doc.text(60, 90, 'This report was generated for the following intervention: ');
        if($scope.Report.FilterSelection == 1)
            doc.text(60, 120, 'Filter selection: NONE (All)')
        else {
            doc.text(60, 120, 'Filter selection: Date Range - StartDate:' + $scope.Report.StartDate + ', EndDate:' + $scope.Report.EndDate);
        }

        doc.setFontSize(20);
        doc.text(60, 150, $scope.Report.Intervention.Name);
        doc.setFontType("normal");
        doc.text(60, 180, doc.splitTextToSize("Description: " + $scope.Report.Intervention.Description, 500));
        doc.setFontType("bold");
        doc.text(60, 310, "Tests:");
        doc.setFontType("normal");
        y = 310;
        for (var i = 0; i < tests.length; i++) {
            y += 30;
            doc.text(60, y, "> " + tests[i].Name);
        }
        doc.text(60, 800, "Page:" + pages);
        //doc.text(20, 40, JSON.stringify(response.data));
        for (var i = 0; i < tests.length; i++) {
            y = 0;
            doc.addPage();
            pages++;
            doc.setFontType("bold");
            doc.setFontSize(30);
            y += 60;
            doc.text(60, y, tests[i].Name);
            doc.setFontType("normal");
            doc.setFontSize(20);
            y += 30;
            doc.text(60, y, "ID: " + tests[i].Id);
            y += 30;
            doc.text(60, y, doc.splitTextToSize("Description: " + tests[i].Description, 500));
            y += 30;
            doc.text(60, y, "This test has been completed: " + tests[i].CompletionCount + " times");
            y += 30;
            doc.text(60, y, doc.splitTextToSize("This report lists all multiple-choice & multi-answer questions in the pages below.", 500));
            y += 60;
            doc.setFontSize(15);
            doc.text(60, y, "*Other question types such as text answers cannot be usefully displayed.");
            doc.setFontSize(20);
            for (var j = 0; j < tests[i].Questions.length; j++) {
                if (tests[i].Questions[j].Id != 0) {
                    doc.addPage();
                    pages++;
                    y = 60;
                    doc.text(60, y, "Question ID: " + tests[i].Questions[j].Id);
                    y += 30;
                    doc.text(60, y, doc.splitTextToSize("Question:" + tests[i].Questions[j].Question, 500));
                    y += 30;
                    doc.text(60, y, doc.splitTextToSize("Values:" + objsToArrs(tests[i].Questions[j].Answers), 500));
                    y += 60;
                    doc.text(60, y, "Chart:");
                    y += 60;
                    doc.addImage(createChart(objsToArrs(tests[i].Questions[j].Answers)), 'PNG', 60, y, 400, 400);
                    doc.text(60, 800, "Page:" + pages);
                }
            }
        }
        
        return doc;
    }

    function checkPages(doc) {
        var pageHeight = doc.internal.pageSize.height;
        // Before adding new content
        if (y >= pageHeight) {
            doc.text(60, 800, "Page:" + pages);
            doc.addPage();
            pages++;
            y = 0; // Restart height position
        }
        return doc;
    }

    function createChart(data) {
        //var s1 = [['Sony', 7], ['Samsumg', 13.3], ['LG', 14.7], ['Vizio', 5.2], ['Insignia', 1.2]];

        var plot8 = $.jqplot('chartdiv', [data], {
            grid: {
                drawBorder: false,
                drawGridlines: false,
                background: '#ffffff',
                shadow: false
            },
            axesDefaults: {

            },
            seriesDefaults: {
                renderer: $.jqplot.PieRenderer,
                rendererOptions: {
                    showDataLabels: true
                }
            },
            legend: {
                show: true,
                rendererOptions: {
                    numberRows: 1
                },
                location: 's'
            }
        });
        var imgData = $('#chartdiv').jqplotToImageStr({}); // given the div id of your plot, get the img data
        return imgData;
    }

    function objsToArrs(objs) {
        var array = []
        for (var i = 0; i < objs.length; i++) {
            var tempArray = [];
            tempArray.push(objs[i].Key);
            tempArray.push(objs[i].Value);
            array.push(tempArray);
        }
        return array;
    }
});

ReportingApp.directive('datepicker', function () {
    return {
        require: 'ngModel',
        link: function (scope, el, attr, ngModel) {
            $(el).datepicker({
                onSelect: function (dateText) {
                    scope.$apply(function () {
                        ngModel.$setViewValue(dateText);
                    });
                }
            });
        }
    };
});


//-------------------------------------------------------------------------------------------------------------