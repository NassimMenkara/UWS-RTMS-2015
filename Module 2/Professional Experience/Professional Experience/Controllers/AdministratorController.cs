using Newtonsoft.Json;
using Professional_Experience.Models;
using PX_Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Professional_Experience.Controllers
{
   [Authorize(Roles = "Administrator")]
    public class AdministratorController : Controller
    {
        // GET: Administrator
        public ActionResult Index()
        {
            return View();
        }

       public ActionResult InterventionSetup()
        {
           
            return View();
        }
        public ActionResult Reporting()
        {
            return View();    
        }

       /* Get all interventions */
        public string GetInterventions()
        {
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            String sql = "SELECT * FROM Intervention_Area;";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return ConvertDataTabletoString(dt);
        }

        public JsonResult GenerateReport(ReportViewModel m)
        {
            if (ModelState.IsValid)
            {
                //get all tests associated with the selected intervention
                //For each test taken within the date range, get answers
                //Generate simple statistics, averages etc from data
                //Return data to client
                int interventionId = m.intervention.Id;
                List<ReportTestModel> tests = new List<ReportTestModel>();
                String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                String sql = "SELECT * FROM Intervention_Area_Test WHERE Intervention_Area_Id = '" + interventionId + "';";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader nwReader = cmd.ExecuteReader();

                while (nwReader.Read())
                {
                    ReportTestModel test = new ReportTestModel();
                    test.Id = Convert.ToInt32(nwReader["Id"].ToString());
                    test.Name = nwReader["Name"].ToString();
                    test.Description = nwReader["Description"].ToString();
                    var questionSql = "";
                    if (m.FilterSelection == 1){
                        sql = "SELECT Count(Id) FROM Trial_Participant_Intervention_Area_Test WHERE Intervention_Area_Test_Id = '" + test.Id + "';";
                        questionSql = "SELECT Answer, COUNT(*) AS AnswerCount, Intervention_Area_Test_Id, Intervention_Area_Test_Question.Id, Question, Intervention_Area_Test_Question.Question_Type FROM Intervention_Area_Test_Question_Answer INNER JOIN Intervention_Area_Test_Question ON Intervention_Area_Test_Question_Id = Intervention_Area_Test_Question.Id AND Intervention_Area_Test_Id = '" + test.Id + "' AND (Question_Type = '1' OR Question_Type = '2') GROUP BY Answer, Intervention_Area_Test_Id , Intervention_Area_Test_Question.Id, Question, Intervention_Area_Test_Question.Question_Type;";
                    }
                    else if (m.FilterSelection == 2)
                    {
                        String startDate = DateTime.ParseExact(m.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        String endDate = DateTime.ParseExact(m.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        sql = "SELECT Count(Id) FROM Trial_Participant_Intervention_Area_Test WHERE Trial_Participant_Intervention_Area_Test.DateTaken BETWEEN '" + startDate + "' AND '" + endDate + "' AND Intervention_Area_Test_Id = '" + test.Id + "';";
                        questionSql = "SELECT Answer, COUNT(*) AS AnswerCount, Intervention_Area_Test_Question.Intervention_Area_Test_Id, Intervention_Area_Test_Question.Id, Question, Intervention_Area_Test_Question.Question_Type FROM Intervention_Area_Test_Question_Answer INNER JOIN Intervention_Area_Test_Question ON Intervention_Area_Test_Question_Id = Intervention_Area_Test_Question.Id INNER JOIN Trial_Participant_Intervention_Area_Test ON Trial_Participant_Intervention_Area_Test_Id = Trial_Participant_Intervention_Area_Test.Id WHERE Trial_Participant_Intervention_Area_Test.DateTaken BETWEEN '" + startDate + "' AND '" + endDate + "' AND Intervention_Area_Test_Question.Intervention_Area_Test_Id = '" + test.Id + "' AND (Question_Type = '1' OR Question_Type = '2') GROUP BY Answer, Intervention_Area_Test_Question.Intervention_Area_Test_Id , Intervention_Area_Test_Question.Id, Question, Intervention_Area_Test_Question.Question_Type;";
                    }
                    SqlConnection testConn = new SqlConnection(connectionString);
                    testConn.Open();
                    SqlCommand testCmd = new SqlCommand(sql, testConn);
                    test.CompletionCount = (int)testCmd.ExecuteScalar();
                    testConn.Close();
                    // SQL statement retrieves all multiple choice or multi answers and groups them to get the count for each answer
                    SqlConnection questionConn = new SqlConnection(connectionString);
                    questionConn.Open();
                    SqlCommand questionCmd = new SqlCommand(questionSql, questionConn);
                    SqlDataReader questionReader = questionCmd.ExecuteReader();
                    List<ReportQuestionModel> questions = new List<ReportQuestionModel>();
                    var currentId = 0;
                    var questionAdded = false;
                    ReportQuestionModel question = new ReportQuestionModel();
                    while(questionReader.Read())
                    {
                        if (currentId == Convert.ToInt32(questionReader["Id"].ToString())) //handles multi-answer questions
                        {
                            question.Answers.Add(new KeyValuePair<string, int>(questionReader["Answer"].ToString(), Convert.ToInt32(questionReader["AnswerCount"].ToString())));
                        }
                        else
                        {
                            if (currentId != 0)
                            {
                                questions.Add(question); //Add question
                                question = new ReportQuestionModel();
                                questionAdded = true;
                            }
                            question.Id = Convert.ToInt32(questionReader["Id"].ToString());
                            currentId = question.Id;
                            question.Question = questionReader["Question"].ToString();
                            question.Question_Type = Convert.ToInt32(questionReader["Question_Type"].ToString());
                            question.Answers = new List<KeyValuePair<String, int>>();
                            question.Answers.Add(new KeyValuePair<String, int>(questionReader["Answer"].ToString(), Convert.ToInt32(questionReader["AnswerCount"].ToString())));
                            questionAdded = false;
                        }
                    }
                    if(!questionAdded)
                        questions.Add(question);
                    questionReader.Close();
                    questionConn.Close();
                    test.Questions = new List<ReportQuestionModel>(questions);
                    tests.Add(test);
                }
                nwReader.Close();
                conn.Close();
                return Json(tests, JsonRequestBehavior.AllowGet);
            }
            return Json("fail", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SubmitIntervention()
        {
            HttpContext.Request.InputStream.Position = 0;
            var result = new System.IO.StreamReader(HttpContext.Request.InputStream).ReadToEnd().ToString();
            dynamic intervention = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

            List<string> validInterventionErrors = validIntervention(intervention);
            if (validInterventionErrors.Count == 0)
            {
                insertIntervention(intervention);
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(validInterventionErrors, JsonRequestBehavior.AllowGet);
            }
        }
        public string GetInvestigators()
        {
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            String sql = "SELECT Person.First_Name, Person.Last_Name, Investigator.Id, Investigator.Institution FROM Investigator INNER JOIN Person ON Investigator.Person_id = Person.Id;";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return ConvertDataTabletoString(dt);
        }

        private void insertIntervention(dynamic intervention){
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "INSERT INTO Intervention_Area (Name, Description) OUTPUT INSERTED.ID values (@Intervention_Name, @Intervention_Description)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Intervention_Name", SqlDbType.VarChar).Value = intervention.Intervention_Name;
            cmd.Parameters.Add("@Intervention_Description", SqlDbType.VarChar).Value = intervention.Intervention_Description;
            int interventionId = (int)cmd.ExecuteScalar();
            int[] Investigators = intervention.Investigators.ToObject<int[]>();
            for (int i = 0; i < Investigators.Length; i++) //insert investigators
            {
                sql = "INSERT INTO Investigator_Intervention_Area (Investigator_Id, Intervention_Area_Id) values (@Investigator_Id, @Intervention_Area_Id)";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Investigator_Id", SqlDbType.Int).Value = Investigators[i];
                cmd.Parameters.Add("@Intervention_Area_Id", SqlDbType.Int).Value = interventionId;
                cmd.ExecuteNonQuery();
            }
            dynamic[] Tests = intervention.Tests.ToObject<dynamic[]>();
            for (int i = 0; i < Tests.Length; i++)
            {
                sql = "INSERT INTO Intervention_Area_Test (Intervention_Area_Id, Name, Description) OUTPUT INSERTED.ID values (@Intervention_Area_Id, @Test_Name, @Test_Description)";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Intervention_Area_Id", SqlDbType.Int).Value = interventionId;
                cmd.Parameters.Add("@Test_Name", SqlDbType.VarChar).Value = Tests[i].Test_Name;
                cmd.Parameters.Add("@Test_Description", SqlDbType.VarChar).Value = Tests[i].Test_Description;
                int testId = (int)cmd.ExecuteScalar();
                dynamic[] Questions = intervention.Tests[i].Questions.ToObject<dynamic[]>();
                for (int j = 0; j < Questions.Length; j++)
                {
                    sql = "INSERT INTO Intervention_Area_Test_Question (Intervention_Area_Test_Id, Question, Question_Type, Sequence) OUTPUT INSERTED.ID values (@Intervention_Area_Test_Id, @Question, @Question_Type, @Sequence)";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add("@Intervention_Area_Test_Id", SqlDbType.Int).Value = testId;
                    cmd.Parameters.Add("@Question", SqlDbType.VarChar).Value = Questions[j].Question_Title;
                    cmd.Parameters.Add("@Question_Type", SqlDbType.VarChar).Value = Questions[j].Answer_Type;
                    cmd.Parameters.Add("@Sequence", SqlDbType.Int).Value = j+1;
                    int questionId = (int)cmd.ExecuteScalar();
                    dynamic[] Answers = intervention.Tests[i].Questions[j].Answers.ToObject<dynamic[]>();
                    for(int k = 0; k < Answers.Length; k++)
                    {
                        sql = "INSERT INTO Intervention_Area_Test_Question_Option (Intervention_Area_Test_Question_Id, Opt) values (@Intervention_Area_Test_Question_Id, @Opt)";
                        cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.Add("@Intervention_Area_Test_Question_Id", SqlDbType.Int).Value = questionId;
                        cmd.Parameters.Add("@Opt", SqlDbType.VarChar).Value = Answers[k];
                        cmd.ExecuteNonQuery();
                    }
                }
            }
                conn.Close();
        }
        private List<string> validIntervention(dynamic intervention)
        {
            List<string> formErrors = new List<string>();
            int x = 0;
            int y = 0;
            int a;

            if (intervention.Intervention_Name == "")
                formErrors.Add("Intervention name is empty");
            string Intervention_Name = intervention.Intervention_Name.ToObject<string>();
            bool isNumeric = int.TryParse(Intervention_Name, out a);
            if (isNumeric == true)
            {
                formErrors.Add("Intervention name is a number.");
            }

            if (intervention.Intervention_Description == "")
                formErrors.Add("Intervention description is empty");
            string Intervention_Description = intervention.Intervention_Description.ToObject<string>();
            isNumeric = int.TryParse(Intervention_Description, out a);
            if (isNumeric == true)
            {
                formErrors.Add("Intervention description is a number");
            }

            //int[] Investigators = intervention.Investigators.ToObject<int[]>();
            //if (Investigators == null || Investigators.Length == 0)
            //    validForm = false;

            dynamic[] Tests = intervention.Tests.ToObject<dynamic[]>();
            if (Tests == null || Tests.Length == 0)
            {
                formErrors.Add("No tests assigned to intervention");
            }
            else
            {
                for (int i = 0; i < Tests.Length; i++)
                {
                    x = i + 1;
                    if (Tests[i].Test_Name == "")
                    {
                        formErrors.Add("Test " + x + " name is empty");
                    }
                    string Test_Name = Tests[i].Test_Name.ToObject<string>();
                    isNumeric = int.TryParse(Test_Name, out a);
                    if (isNumeric == true)
                    {
                        formErrors.Add("Test " + x + " name is a number");
                    }


                    if (Tests[i].Test_Description == "")
                    {
                        formErrors.Add("Test " + x + " description is empty");

                    }
                    string Test_Description = Tests[i].Test_Description.ToObject<string>();
                    isNumeric = int.TryParse(Test_Description, out a);
                    if (isNumeric == true)
                    {
                        formErrors.Add("Test " + x + " description is a number");
                    }


                    dynamic[] Questions = intervention.Tests[i].Questions.ToObject<dynamic[]>();
                    if (Questions == null || Questions.Length == 0)
                    {
                        formErrors.Add("No questions assigned to test " + x);
                    }
                    else
                    {
                        for (int j = 0; j < Questions.Length; j++)
                        {
                            y = j + 1;
                            if (Questions[j].Question_Title == "")
                            {
                                formErrors.Add("Question " + y + " of test " + x + " is empty");
                            }
                            string question = Questions[j].Question_Title.ToObject<string>();
                            isNumeric = int.TryParse(question, out a);
                            if (isNumeric == true)
                            {
                                formErrors.Add("Question " + y + " of test " + x + " is a number");
                            }



                            if (Questions[j].Answer_Type == "1" || Questions[j].Answer_Type == "2")
                            {
                                dynamic[] Answers = intervention.Tests[i].Questions[j].Answers.ToObject<dynamic[]>();
                                if (Answers == null || Answers.Length == 0)
                                {
                                    formErrors.Add("Question " + y + " of test " + x + " has no answer choices");
                                }
                            }
                        }
                    }
                }
            }
            return formErrors;
        }
        public string ConvertDataTabletoString(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return serializer.Serialize(rows);
        }
    }
}