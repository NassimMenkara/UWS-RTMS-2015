using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Professional_Experience.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Professional_Experience.Models;


namespace Professional_Experience.Controllers
{
    [Authorize(Roles="Participant")]
    public class ParticipantController : UIController
    {
        // GET: Participant
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult InterventionResults()
        {
            String username = System.Web.HttpContext.Current.User.Identity.Name;
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "SELECT * FROM Trial INNER JOIN Trial_Participant ON Trial_Participant.Trial_Id = Trial.Id INNER JOIN Participant ON Trial_Participant.Participant_Id = Participant.Id INNER JOIN Person ON Participant.Person_Id = Person.Id AND Person.Username = '" + username + "';";
            SqlCommand cmd = new SqlCommand(sql, conn);
            SqlDataReader nwReader = cmd.ExecuteReader();
            while (nwReader.Read())
            {
                ViewBag.TrialId = Convert.ToInt32(nwReader["Id"].ToString());
                ViewBag.TrialName = nwReader["Name"].ToString();
                ViewBag.TrialDescription = nwReader["Description"].ToString();
                ViewBag.TrialStartDate = nwReader["Start_Date"].ToString();
                ViewBag.TrialEndDate = nwReader["End_Date"].ToString();
            }
            nwReader.Close();
            List<InterventionModels.Intervention> interventions = new List<InterventionModels.Intervention>();
            sql = "SELECT * FROM Intervention_Area;";
            cmd = new SqlCommand(sql, conn);
            nwReader = cmd.ExecuteReader();
            while (nwReader.Read())
            {
                String interventionId = nwReader["Id"].ToString();
                String name = nwReader["Name"].ToString();
                String description = nwReader["Description"].ToString();
                List<InterventionModels.Test> tests = new List<InterventionModels.Test>();
                SqlConnection testConn = new SqlConnection(connectionString);
                testConn.Open();
                sql = "SELECT * FROM Intervention_Area_Test WHERE Intervention_Area_Id = '" + interventionId + "';";
                SqlCommand testCmd = new SqlCommand(sql, testConn);
                SqlDataReader testReader = testCmd.ExecuteReader();
                while (testReader.Read())
                {
                    int testId = Convert.ToInt32(testReader["Id"].ToString());
                    String testName = testReader["Name"].ToString();
                    String testDescription = testReader["Description"].ToString();
                    tests.Add(new InterventionModels.Test(testId, testName, testDescription, null));
                }
                interventions.Add(new InterventionModels.Intervention(name, description, null, tests));
                testReader.Close();
                testConn.Close();
            }
            nwReader.Close();
            conn.Close();

            ViewBag.Username = username;
            ViewBag.Interventions = interventions;
            return View();
        }

        public ActionResult CompleteTest(int trialId, String testName, int testId)
        {
            List<InterventionModels.Question> questions = new List<InterventionModels.Question>();
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "SELECT * FROM Intervention_Area_Test_Question WHERE Intervention_Area_Test_Id = '" + testId + "' ORDER BY Sequence;";
            SqlCommand cmd = new SqlCommand(sql, conn);
            SqlDataReader nwReader = cmd.ExecuteReader();
            while (nwReader.Read())
            {
                int questionId = Convert.ToInt32(nwReader["Id"].ToString());
                String questionName = nwReader["Question"].ToString();
                int answerType = Convert.ToInt32(nwReader["Question_Type"].ToString());
                List<String> answers = new List<String>();
                SqlConnection answerConn = new SqlConnection(connectionString);
                answerConn.Open();
                sql = "SELECT * FROM Intervention_Area_Test_Question_Option WHERE Intervention_Area_Test_Question_Id = '" + questionId + "';";
                SqlCommand answerCmd = new SqlCommand(sql, answerConn);
                SqlDataReader answerReader = answerCmd.ExecuteReader();
                while (answerReader.Read())
                {
                    String answer = answerReader["Opt"].ToString();
                    answers.Add(answer);
                }
                questions.Add(new InterventionModels.Question(questionId, questionName, answerType, answers));
                answerReader.Close();
                answerConn.Close();
            }
            nwReader.Close();
            conn.Close();
            ViewBag.Questions = questions;
            ViewBag.TestName = testName;
            ViewBag.TestId = testId;
            ViewBag.TrialId = trialId;
            
            return View();
        }

        /* Is the end point function which the client sends a POST request to containing the participant's answers
         * and other test related fields. This data is then used to get the necessary Ids needed to validate and then
           insert the completed test into the database */
        public JsonResult SubmitTest()
        {       
            String username = System.Web.HttpContext.Current.User.Identity.Name;
            HttpContext.Request.InputStream.Position = 0;
            var result = new System.IO.StreamReader(HttpContext.Request.InputStream).ReadToEnd().ToString();
            dynamic test = JsonConvert.DeserializeObject(result);
            dynamic testDetails = test.testDetails;
            dynamic testAnswers = test.testAnswers;
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "SELECT Trial_Participant.Id FROM Trial_Participant INNER JOIN Trial ON Trial_Participant.Trial_Id = Trial.Id INNER JOIN Participant ON Trial_Participant.Participant_Id = Participant.Id INNER JOIN Person ON Participant.Person_Id = Person.Id WHERE Trial.Id = '" + testDetails.Trial_Id + "' AND Person.Username = '" + username + "';";
            SqlCommand cmd = new SqlCommand(sql, conn);
            int trialParticipantId = (int)cmd.ExecuteScalar();
            conn.Close();
            List<String> questionIds = new List<String>();
            conn = new SqlConnection(connectionString);
            conn.Open();
            sql = "SELECT * FROM Intervention_Area_Test_Question WHERE Intervention_Area_Test_Id = '" + testDetails.Intervention_Area_Test_Id + "' ORDER BY Sequence;";
            cmd = new SqlCommand(sql, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                questionIds.Add(reader["Id"].ToString());
            }
            conn.Close();
            int Trial_Participant_Intervention_Area_Test_Id = insertTestCompleted(trialParticipantId, Convert.ToInt32(testDetails.Intervention_Area_Test_Id)); //inserts test details
            for(int i = 0; i < questionIds.Count; i++)
            {
                if (testAnswers[questionIds[i]].GetType() == typeof(JArray))
                {
                    //inserts a list of answers (Multi answer)
                    dynamic[] answers = testAnswers[questionIds[i]].ToObject<dynamic[]>();
                    for (int j = 0; j < answers.Length; j++)
                    {
                        insertAnswer(Convert.ToInt32(questionIds[i]), Trial_Participant_Intervention_Area_Test_Id, answers[j]);
                    }
                }
                else
                {
                    //inserts a single answer (Multiple choice, Measurement, Text)
                    insertAnswer(Convert.ToInt32(questionIds[i]), Trial_Participant_Intervention_Area_Test_Id, testAnswers[questionIds[i]].ToString());
                }
            }

            return Json("success", JsonRequestBehavior.AllowGet);
            //return Json("fail", JsonRequestBehavior.AllowGet);      
        }

        /* Inserts into Trial_Participant_Intervention_Area_Test 
         * to record details of a completed test */
        private int insertTestCompleted(int Trial_Participant_Id, int Intervention_Area_Test_Id)
        {
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "INSERT INTO Trial_Participant_Intervention_Area_Test (Trial_Participant_Id, Intervention_Area_Test_Id, DateTaken) OUTPUT INSERTED.ID values (@Trial_Participant_Id, @Intervention_Area_Test_Id, @DateTaken)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Trial_Participant_Id", SqlDbType.Int).Value = Trial_Participant_Id;
            cmd.Parameters.Add("@Intervention_Area_Test_Id", SqlDbType.Int).Value = Intervention_Area_Test_Id;
            cmd.Parameters.Add("@DateTaken", SqlDbType.DateTime).Value = DateTime.Now;
            return (int)cmd.ExecuteScalar();
        }
        /* Inserts into Intervention_Area_Test_Question_Answer to record participant's answer */
        private void insertAnswer(int Intervention_Area_Test_Question_Id, int Trial_Participant_Intervention_Area_Test_Id, String Answer)
        {
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "INSERT INTO Intervention_Area_Test_Question_Answer (Intervention_Area_Test_Question_Id, Trial_Participant_Intervention_Area_Test_Id, Answer) values (@Intervention_Area_Test_Question_Id, @Trial_Participant_Intervention_Area_Test_Id, @Answer)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Intervention_Area_Test_Question_Id", SqlDbType.Int).Value = Intervention_Area_Test_Question_Id;
            cmd.Parameters.Add("@Trial_Participant_Intervention_Area_Test_Id", SqlDbType.Int).Value = Trial_Participant_Intervention_Area_Test_Id;
            cmd.Parameters.Add("@Answer", SqlDbType.VarChar).Value = Answer;
            cmd.ExecuteNonQuery();
        }
    }
}