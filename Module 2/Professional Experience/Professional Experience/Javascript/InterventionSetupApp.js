onStart();
var intervention = { "Intervention_Name": "", "Intervention_Description": "", "Investigators": [], "Tests": [] };

var Test = function () {
    this.Test_Id = 0;
    this.Test_Name = "";
    this.Test_Description = "";
    this.Questions = [];
};

var Question = function () {
    this.Question_Id = 0;
    this.Question_Title = "";
    this.Answer_Type = "";
    this.Answers = [];
};
var testCount = 0;
var questionCount = 0;
var currentTestId = 0;
var currentQuestionId = 0;
var editingTest = false;
var editingQuestion = false;
var externalTest = 0;
var tempTest = new Test();

var tempQuestion = new Question();

var testSelected = false;
var questionSelected = false;
var answerSelected = false;

function onStart() {
    // runs on start and sets some default values 
    // display divs
    document.getElementById('Investigators').style.display = "block";
    document.getElementById('Tests').style.display = "block";
    document.getElementById('ExternalTest').style.display = "block";
    document.getElementById('AddQuestion').style.display = "block";
    document.getElementById("message").style.display = "block";
    // hide measurement dropdown
    document.getElementById('measurementType').style.display = "none";
    // enable select menus
    document.getElementById('testSelect').disabled = false;
    document.getElementById('investigatorSelect').disabled = false;
    document.getElementById('questionSelect').disabled = false;
    document.getElementById('answersSelect').disabled = false;
    printMessage("Intervention setup has started!", "success");
}

function questionTypeChange(selectedOption) {
    // runs when question type is changed and modifies UI accordingly
    if (selectedOption == null) {
        var select = document.getElementById("answerType");
        selectedOption = select.options[select.selectedIndex].value;
    }
    var testIndex = getTestIndex(currentTestId);
    var questionIndex = getQuestionIndex(testIndex, currentQuestionId);
    if (selectedOption == "3") { //Measurement type
        document.getElementById("answer").style.display = "none";
        document.getElementById("addAnswerBtn").style.display = "none";
        document.getElementById("measurementType").style.display = "block";
        emptySelect(document.getElementById("answersSelect"));
        intervention.Tests[testIndex].Questions[questionIndex].Answers = []; //empty answers
    } else if (selectedOption == "4") { //Text type
        document.getElementById("answer").style.display = "none";
        document.getElementById("addAnswerBtn").style.display = "none";
        document.getElementById("measurementType").style.display = "none";
        emptySelect(document.getElementById("answersSelect"));
        intervention.Tests[testIndex].Questions[questionIndex].Answers = []; //empty answers
    } else { //Multiple choice or Multiple answer type
        document.getElementById("answer").style.display = "block";
        document.getElementById("addAnswerBtn").style.display = "block";
        document.getElementById("measurementType").style.display = "none";
    }
}

function testSelectChange() {
    // runs when test select list changes and configures UI
    testSelected = true;
    var testSelect = document.getElementById("testSelect");
    if (testSelect.options[testSelect.selectedIndex].text == "Simply Brain Training") {
        document.getElementById("editTestBtn").disabled = true;
    } else {
        document.getElementById("editTestBtn").disabled = false;
    }
    toggleTestButtons();
}

function questionSelectChange() {
    // runs when question select list changes
    questionSelected = true;
    toggleQuestionButtons();
}

function answerSelectChange() {
    // runs when answer select list changes
    answerSelected = true;
    toggleAnswerButton();
}

function emptySelect(select) {
    //empty select from options
    while (select.options.length > 0)
        select.remove(0);
}

function getTestIndex(testId) {
    // find a test's index in tests array
    for (var i = 0; i < intervention.Tests.length; i++) {
        if (intervention.Tests[i].Test_Id == testId) {
            return i;
        }
    }
}

function getQuestionIndex(testIndex, questionId) {
    // find a question's index in questions array
    for (var i = 0; i < intervention.Tests[testIndex].Questions.length; i++) {
        if (intervention.Tests[testIndex].Questions[i].Question_Id == questionId) {
            return i;
        }
    }
}

function resetTestView() {
    // reset UI for test view
    document.getElementById("testName").value = "";
    document.getElementById("testDescription").value = "";
    emptySelect(document.getElementById("questionSelect"));
}

function resetQuestionView() {
    // reset UI for question view
    document.getElementById("questionTitle").value = "";
    document.getElementById("answerType").selectedIndex = 0;
    emptySelect(document.getElementById("answersSelect"));
    document.getElementById("measurementType").selectedIndex = 0;
    document.getElementById("answer").value = "";
    document.getElementById("answer").style.display = "block";
    document.getElementById("addAnswerBtn").style.display = "block";
    document.getElementById("measurementType").style.display = "none";
}

function checkSelect(options, id) {
    // check to see if id is a select option
    for (var i = 0; i < options.length; i++) {
        if (options[i].value == id) {
            return i;
        }
    }
    return -1;
}

function printMessage(message) {
    // prints message
    var messageElement = document.getElementById('message');
    messageElement.style.display = "inline";
    messageElement.innerHTML = message;
    setTimeout(function () {
        messageElement.innerHTML = "";
        messageElement.style.display = "none";
    }, 5000);
}

function clone(obj) {
    // clones object by value
    var newObj = JSON.parse(JSON.stringify(obj))
    return newObj;
}

function deselectMenu(selectMenu) {
    // deselect a select menu
    document.getElementById(selectMenu).selectedIndex = -1;
}

function toggleTestButtons() {
    // toggle test's edit and delete buttons
    if (testSelected) { //show
        document.getElementById("editTestBtn").style.display = "block";
        document.getElementById("deleteTestBtn").style.display = "block";
    } else { //hide
        document.getElementById("editTestBtn").style.display = "none";
        document.getElementById("deleteTestBtn").style.display = "none";
    }
}

function toggleQuestionButtons() {
    // toggle question's edit and delete buttons
    if (questionSelected) { //show
        document.getElementById("editQuestionBtn").style.display = "block";
        document.getElementById("deleteQuestionBtn").style.display = "block";
    } else { //hide
        document.getElementById("editQuestionBtn").style.display = "none";
        document.getElementById("deleteQuestionBtn").style.display = "none";
    }
}

function toggleAnswerButton() {
    // toggle answer's delete button
    if (answerSelected) { //show
        document.getElementById("deleteAnswerBtn").style.display = "block";
    } else { //hide
        document.getElementById("deleteAnswerBtn").style.display = "none";
    }
}

//--------------------------------------------------AngularJS--------------------------------------------------
var InterventionSetupApp = angular.module('InterventionSetupApp', []);

InterventionSetupApp.controller('InterventionSetupController', function ($scope, $http) {
    $scope.index = 1;
    $scope.submitSuccessful = false;
    $scope.interventionObj = intervention;
    getInvestigators();

    window.onbeforeunload = function () {
        //if the form has been changed, prompt user to confirm leaving from page
        if ($scope.intervention_name != undefined || $scope.intervention_description != undefined || document.getElementById("testSelect").length >= 1) {
            if(!$scope.submitSuccessful)
                return "Are you sure you wish to leave Intervention Setup? an intervention which hasn't been saved will be lost";
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
        $scope.index = i;
    };

    // submit intervention object to server
    $scope.submitIntervention = function () {
        createIntervention();
        $http.post('/Administrator/SubmitIntervention', JSON.stringify(intervention)).
            then(function (response) {
                if (response.data == "success") {
                    $scope.submitSuccessful = true;
                    printMessage("The intervention was saved successfully")
                } else {
                    printMessage("Response:" + response.data);
                }
            }, function (error) {
        });
    };

    // retrieves investigators from server
    function getInvestigators() {
        $http.get('/Administrator/GetInvestigators').
          then(function (results) {
               $scope.investigators = results.data;
          }, function (error) {
        });
    }

    // validate intervention object
    $scope.validateIntervention = function () {
        var interventionInvalid = false;
        if ($scope.intervention_name == undefined || $scope.intervention_name == "") {
            interventionInvalid = true;
        }
        if ($scope.intervention_description == undefined || $scope.intervention_description == "") {
            interventionInvalid = true;
        }
        if (document.getElementById("testSelect").length < 1) { //an intervention must have at least one test
            interventionInvalid = true;
        }
        if ($scope.submitSuccessful) {
            interventionInvalid = true;
        }
        return interventionInvalid;
    }

    // validate test object
    $scope.validateTest = function () {
        var testInvalid = false;
        if ($scope.test_name == undefined || $scope.test_name == "") {
            testInvalid = true;
        }
        if ($scope.test_description == undefined || $scope.test_description == "") {
            testInvalid = true;
        }
        if (document.getElementById("questionSelect").length < 1) { //a test must have at least a question
            testInvalid = true;
        }
        return testInvalid;
    }

    // validate question object
    $scope.validateQuestion = function () {
        var questionInvalid = false;
        if ($scope.question_title == undefined || $scope.question_title == "") {
            questionInvalid = true;
        }
        switch(document.getElementById("answerType").selectedIndex) { 
            case 0:
                if (document.getElementById("answersSelect").length < 2) { //a test must have at least a question
                    questionInvalid = true;
                }
                break;
            case 1:
                if (document.getElementById("answersSelect").length < 2) { //a test must have at least a question
                    questionInvalid = true;
                }
                break;
            default:
                break;
        }
        return questionInvalid;
    }

    // create a new test
    $scope.createTest = function () {
        var test = new Test(); //create test object
        $scope.test_name = '';
        $scope.test_description = '';
        testCount++;
        test.Test_Id = testCount; //Id is +1 of length, starting id = 1
        currentTestId = test.Test_Id; //the current test's id
        intervention.Tests.push(test); //push new test object to intervention object
        printMessage("New test has been created");
        if (testSelected) {
            testSelected = false;
            toggleTestButtons();
        }
        deselectMenu("testSelect");
        $scope.setIndex(3);
    }

    // create a new external test
    $scope.createExternalTest = function () {
        if (externalTest == 0) {
            var test = new Test(); //create test object
            testCount++;
            test.Test_Id = testCount; //Id is +1 of length, starting id = 1
            test.Test_Name = "Simply Brain Training";
            test.Test_Description = "Simply Brain Training is an external brain training exercises provider that provides simple & effective brain testing.";
            currentTestId = test.Test_Id; //the current test's id
            intervention.Tests.push(test); //push new test object to intervention object
            printMessage("New external test has been created");
        } else {
            currentTestId = externalTest;
            printMessage("You are now editing the external test");
        }
        if (testSelected) {
            testSelected = false;
            toggleTestButtons();
        }
        deselectMenu("testSelect");
        $scope.setIndex(4);
    }

    // save test
    $scope.saveTest = function() {
        var testName = document.getElementById("testName").value;
        var select = document.getElementById("testSelect");
        var options = select.options;
        var testIndex = getTestIndex(currentTestId);
        var optionIndex = checkSelect(options, currentTestId);
        if (optionIndex != -1) { //select already has test id
            select.options[optionIndex].text = testName; //set test name again in case name was edited 
        } else {                 //select doesn't have test id
            var option = document.createElement("option");
            option.value = currentTestId
            option.text = testName;
            select.add(option); //add new
        }
        intervention.Tests[testIndex].Test_Name = testName;
        intervention.Tests[testIndex].Test_Description = document.getElementById("testDescription").value;
        currentTestId = 0;
        printMessage("Test has been saved");
        resetTestView();
        if (questionSelected) {
            questionSelected = false;
            toggleQuestionButtons();
        }
        $scope.setIndex(1);
    }

    // edit a test
    $scope.editTest = function() {
        var testSelect = document.getElementById("testSelect");
        if (testSelect.selectedIndex < 0) {
            printMessage("No test has been selected for editing");
        } else {
            editingTest = true;
            currentTestId = testSelect.options[testSelect.selectedIndex].value; //set currentTestId to test selected from list
            testIndex = getTestIndex(currentTestId);
            tempTest = clone(intervention.Tests[testIndex]);
            $scope.test_name = intervention.Tests[testIndex].Test_Name;
            document.getElementById("testName").value = intervention.Tests[testIndex].Test_Name;
            $scope.test_description = intervention.Tests[testIndex].Test_Description;
            document.getElementById("testDescription").value = intervention.Tests[testIndex].Test_Description;
            var questionSelect = document.getElementById("questionSelect");
            for (var i = 0; i < intervention.Tests[testIndex].Questions.length; i++) {
                var option = document.createElement("option");
                option.text = intervention.Tests[testIndex].Questions[i].Question_Title;
                option.value = intervention.Tests[testIndex].Questions[i].Question_Id;
                questionSelect.add(option);
            }
            printMessage("You are now editing an existing test");
            $scope.setIndex(3);
        }
    }

    // delete a test
    $scope.deleteTest = function() {
        var testSelect = document.getElementById("testSelect");
        if (testSelect.selectedIndex < 0) {
            printMessage("No test has been selected for deleting");
        } else {
            testId = testSelect.options[testSelect.selectedIndex].value; //testId to be deleted
            var testIndex = getTestIndex(testId);
            intervention.Tests.splice(testIndex, 1);
            testSelect.remove(testSelect.selectedIndex);
            deselectMenu("testSelect");
            testSelected = false;
            toggleTestButtons();
            printMessage("Test has been deleted");
        }
    }

    // create a new question
    $scope.createQuestion = function() {
        var question = new Question(); //create question object
        $scope.question_title = '';
        questionCount++;
        var testIndex = getTestIndex(currentTestId);
        question.Question_Id = questionCount; //Id is +1 of length, starting id = 1
        currentQuestionId = question.Question_Id; //the current question's id
        intervention.Tests[testIndex].Questions.push(question);
        printMessage("New question has been created");
        if (questionSelected) {
            questionSelected = false;
            toggleQuestionButtons();
        }
        deselectMenu("questionSelect");
        $scope.setIndex(5);
    }

    // edit a question
    $scope.editQuestion = function() {
        if (questionSelect.selectedIndex < 0) {
            printMessage("No question has been selected for editing");
        } else {
            editingQuestion = true;
            currentQuestionId = questionSelect.options[questionSelect.selectedIndex].value;
            testIndex = getTestIndex(currentTestId);
            questionIndex = getQuestionIndex(testIndex, currentQuestionId);
            tempQuestion = clone(intervention.Tests[testIndex].Questions[questionIndex]);
            $scope.question_title = intervention.Tests[testIndex].Questions[questionIndex].Question_Title;
            document.getElementById("questionTitle").value = intervention.Tests[testIndex].Questions[questionIndex].Question_Title; //prefill questionTitle
            document.getElementById("answerType").value = intervention.Tests[testIndex].Questions[questionIndex].Answer_Type; //prefill answerType
            questionTypeChange(intervention.Tests[testIndex].Questions[questionIndex].Answer_Type);
            var answersSelect = document.getElementById("answersSelect");
            answers = intervention.Tests[testIndex].Questions[questionIndex].Answers.slice();
            for (var i = 0; i < intervention.Tests[testIndex].Questions[questionIndex].Answers.length; i++) { //prefill answers list
                var option = document.createElement("option");
                option.text = intervention.Tests[testIndex].Questions[questionIndex].Answers[i];
                answersSelect.add(option);
            }
            printMessage("You are now editing an existing question");
            $scope.setIndex(5);
        }
    }

    // delete a question
    $scope.deleteQuestion = function() {
        var questionSelect = document.getElementById("questionSelect");
        if (questionSelect.selectedIndex < 0) {
            printMessage("No question has been selected for deleting");
        } else {
            questionId = questionSelect.options[questionSelect.selectedIndex].value; //questionId to be deleted
            var testIndex = getTestIndex(currentTestId);
            var questionIndex = getQuestionIndex(testIndex, questionId);
            intervention.Tests[testIndex].Questions.splice(questionIndex, 1);
            questionSelect.remove(questionSelect.selectedIndex);
            deselectMenu("questionSelect");
            questionSelected = false;
            toggleQuestionButtons();
            printMessage("Question has been deleted");
        }
    }

    // save external test
    $scope.saveExternalTest = function() {
        var questions = [];
        if (document.getElementById("externalQuestion1").checked) {
            var question = new Question();
            question.Question_Id = questions.length + 1;
            question.Question_Title = document.getElementById("externalQuestion1").value;
            question.Answer_Type = 5; //external test answer type
            questions.push(question);
        }
        if (document.getElementById("externalQuestion2").checked) {
            var question = new Question();
            question.Question_Id = questions.length + 1;
            question.Question_Title = document.getElementById("externalQuestion2").value;
            question.Answer_Type = 5; //external test answer type
            questions.push(question);
        }
        if (document.getElementById("externalScore1").checked) {
            var question = new Question();
            question.Question_Id = questions.length + 1;
            question.Question_Title = document.getElementById("externalScore1").value;
            question.Answer_Type = 5; //external test answer type
            questions.push(question);
        }
        if (document.getElementById("externalScore2").checked) {
            var question = new Question();
            question.Question_Id = questions.length + 1;
            question.Question_Title = document.getElementById("externalScore2").value;
            question.Answer_Type = 5; //external test answer type
            questions.push(question);
        }
        if (externalTest > 0) { //already an existing external test
            testIndex = getTestIndex(externalTest);
            intervention.Tests[testIndex].Questions = questions.slice();
            printMessage("External test has been edited");
        } else {                //save new external test
            externalTest = currentTestId;
            testIndex = getTestIndex(currentTestId);
            intervention.Tests[testIndex].Questions = questions.slice();
            var testSelect = document.getElementById("testSelect");
            var option = document.createElement("option");
            option.value = currentTestId
            option.text = "Simply Brain Training";
            testSelect.add(option); //add new
            printMessage("New external test has been saved");
        }
        $scope.setIndex(1);
    }

    // save question
    $scope.saveQuestion = function(){
        var questionTitle = document.getElementById("questionTitle").value;
        var select = document.getElementById("questionSelect");
        var answerType = document.getElementById("answerType");
        var testIndex = getTestIndex(currentTestId);
        var questionIndex = getQuestionIndex(testIndex, currentQuestionId);
        var options = select.options;
        var optionIndex = checkSelect(options, currentQuestionId);
        if (optionIndex != -1) { //select already has question id
            select.options[optionIndex].text = questionTitle;
        } else {                 //select doesn't have question id
            var option = document.createElement("option");
            option.value = currentQuestionId;
            option.text = questionTitle;
            select.add(option); //add new
        }
        intervention.Tests[testIndex].Questions[questionIndex].Question_Title = questionTitle
        intervention.Tests[testIndex].Questions[questionIndex].Answer_Type = answerType.options[answerType.selectedIndex].value;
        currentQuestionId = 0;
        printMessage("Question has been saved");
        $scope.question_title = '';
        resetQuestionView();
        if (answerSelected) {
            answerSelected = false;
            toggleAnswerButton();
        }
        $scope.setIndex(3);
    }

    // initialise intervention
    function createIntervention() {
        intervention.Intervention_Name = document.getElementById("interventionName").value;
        intervention.Intervention_Description = document.getElementById("interventionDescription").value;
    }

    // adds investigators selected to intervention
    $scope.addInvestigators = function () {
        var investigatorList = [];
        for (var i = 0; i < $scope.investigator.multiSelect.length; i++) {
            investigatorList.push($scope.investigator.multiSelect[i].Id);
        }
        intervention.Investigators = investigatorList.slice();
        printMessage("Investigators successfully added");
        $scope.setIndex(1);
    }

    // adds answer to the current question
    $scope.addAnswer = function() {
        if (document.getElementById("answer").value != "") { //answer cannot be blank
            var select = document.getElementById("answersSelect");
            var answer = document.getElementById("answer").value;
            //document.getElementById("answer").value = "";
            $scope.add_answer = '';
            var option = document.createElement("option");
            option.text = answer;
            select.add(option);
            var testIndex = getTestIndex(currentTestId);
            var questionIndex = getQuestionIndex(testIndex, currentQuestionId);
            intervention.Tests[testIndex].Questions[questionIndex].Answers.push(answer);
        }
    }

    // deletes currently selected answer
    $scope.deleteAnswer = function() {
        var answersSelect = document.getElementById("answersSelect");
        if (answersSelect.selectedIndex < 0) {
            printMessage("No answer has been selected for deleting");
        } else {
            answer = answersSelect.options[answersSelect.selectedIndex].value; //questionId to be deleted
            var testIndex = getTestIndex(currentTestId);
            var questionIndex = getQuestionIndex(testIndex, currentQuestionId);
            var answerIndex = intervention.Tests[testIndex].Questions[questionIndex].Answers.indexOf(answer);
            intervention.Tests[testIndex].Questions[questionIndex].Answers.splice(answerIndex, 1);
            answersSelect.remove(answersSelect.selectedIndex);
            deselectMenu("answersSelect");
            answerSelected = false;
            toggleAnswerButton();
            printMessage("Answer has been deleted");
        }
    }

    // action for back button in test view
    $scope.backTest = function() {
        if (editingTest) {
            $scope.test_name = '';
            $scope.test_description = '';
            resetTestView();
            var testIndex = getTestIndex(currentTestId);
            intervention.Tests[testIndex] = clone(tempTest);
            currentTestId = 0;
            printMessage("Test was not edited");
            editingTest = false;
        } else {
            $scope.test_name = '';
            $scope.test_description = '';
            discardTest();
        }
        if (questionSelected) {
            questionSelected = false;
            toggleQuestionButtons();
        }
        $scope.setIndex(1);
    }

    // discard current test
    function discardTest() {
        resetTestView();
        var testIndex = getTestIndex(currentTestId);
        intervention.Tests.splice(testIndex, 1);
        currentTestId = 0;
        printMessage("Test has been discarded");
    }

    // discard external test
    $scope.discaredExternalTest = function() {
        if (externalTest == 0) {
            var testIndex = getTestIndex(currentTestId);
            intervention.Tests.splice(testIndex, 1);
            printMessage("External test has been discarded");
        } else {
            printMessage("External test was not edited")
        }
        $scope.setIndex(1);
    }

    // action for back button in question view
    $scope.backQuestion = function() {
        if (editingQuestion) {
            resetQuestionView();
            var testIndex = getTestIndex(currentTestId);
            var questionIndex = getQuestionIndex(testIndex, currentQuestionId);
            intervention.Tests[testIndex].Questions[questionIndex] = clone(tempQuestion);
            currentQuestionId = 0;
            printMessage("Question was not edited");
            editingQuestion = false;
        } else {
            discardQuestion();
        }
        if (answerSelected) {
            answerSelected = false;
            toggleAnswerButton();
        }
        $scope.setIndex(3);
    }

    // discard current question
    function discardQuestion() {
        resetQuestionView();
        var testIndex = getTestIndex(currentTestId);
        var questionIndex = getQuestionIndex(testIndex, currentQuestionId);
        intervention.Tests[testIndex].Questions.splice(questionIndex, 1);
        currentQuestionId = 0;
        printMessage("Question has been discarded");
    }

    $scope.checkAnswerSelect = function (i) {
        if (answerSelected)
            return true;
        else
            return false;
    };

    $scope.checkSelectForOptions = function (select) {
        if (document.getElementById(select).length == 0)
            return true;
        else
            return false;
    }

    $scope.checkFieldValidity = function (field, type) {
        if (field != undefined && field != '')
            if (type == 'color') {
                return 'has-success has-feedback';
            } else {
                return 'glyphicon glyphicon-ok form-control-feedback';
            }
        else
            return '';
    }
});
//-------------------------------------------------------------------------------------------------------------
