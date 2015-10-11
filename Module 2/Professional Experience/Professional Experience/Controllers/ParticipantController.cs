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
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Text;


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
            sql = "SELECT Intervention_Area.Id AS interventionId, Intervention_Area.Name AS interventionName, Intervention_Area.Description AS interventionDescription FROM Intervention_Area INNER JOIN Participant_Group_Intervention_Area ON Intervention_Area.Id = Participant_Group_Intervention_Area.Intervention_Area_Id INNER JOIN Trial_Participant_Participant_Group ON Participant_Group_Intervention_Area.Participant_Group_Id = Trial_Participant_Participant_Group.Participant_Group_Id INNER JOIN Trial_Participant ON Trial_Participant.Id = Trial_Participant_Participant_Group.Trial_Participant_Id INNER JOIN Participant ON Trial_Participant.Participant_Id = Participant.Id INNER JOIN Person ON Person.Id = Participant.Person_Id WHERE username = '" + username + "';";
            cmd = new SqlCommand(sql, conn);
            nwReader = cmd.ExecuteReader();
            while (nwReader.Read())
            {
                String interventionId = nwReader["interventionId"].ToString();
                String name = nwReader["interventionName"].ToString();
                String description = nwReader["interventionDescription"].ToString();
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
            int Trial_Participant_Intervention_Area_Test_Id = insertTestCompleted(trialParticipantId, Convert.ToInt32(testDetails.Intervention_Area_Test_Id), DateTime.Now); //inserts test details
            for(int i = 0; i < questionIds.Count; i++)
            {
                if (testAnswers[questionIds[i]] != null)
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
            }

            return Json("success", JsonRequestBehavior.AllowGet);
            //return Json("fail", JsonRequestBehavior.AllowGet);      
        }

        public string getApiKey(string username, int testId)
        {
            int personId;
            int participantId;
            string apiKey = "";

            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            String sql = "SELECT Id FROM Person WHERE Username = '" + username + "'";
            SqlCommand cmd = new SqlCommand(sql, conn);
            personId = (int)cmd.ExecuteScalar();

            sql = "SELECT Id FROM Participant WHERE Person_Id = " + personId;
            cmd = new SqlCommand(sql, conn);
            participantId = (int)cmd.ExecuteScalar();

            sql = "SELECT API_Key FROM Participant_Intervention_Area_API_Key WHERE Participant_Id = " + participantId + " AND Intervention_Area_Test_Id = " + testId;
            cmd = new SqlCommand(sql, conn);
            apiKey = (string)cmd.ExecuteScalar();

            conn.Close();
            return apiKey;
        }

        public ActionResult ExternalTestIndex(int trialId, int testId)
        {
            ViewBag.trialId = trialId;
            ViewBag.testId = testId;
            String username = System.Web.HttpContext.Current.User.Identity.Name;
            string api_key = getApiKey(username, testId);
            if (string.IsNullOrEmpty(api_key))
                ViewBag.api_key = "false";
            else
                ViewBag.api_key = api_key;
            return View();
        }


        public ActionResult ExternalTestResultRetrieval(int trialId, int testId)
        {
            ExternalResultRequest resultRequest = new ExternalResultRequest();
            resultRequest.request = "getResults";
            resultRequest.date1 = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
            resultRequest.date2 = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            String username = System.Web.HttpContext.Current.User.Identity.Name;
            resultRequest.key = getApiKey(username, testId);


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string predata = serializer.Serialize(resultRequest);
            var data = Encoding.ASCII.GetBytes(predata);
            var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8080/externalapi");
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }


            var response = request.GetResponse();
            var rStream = response.GetResponseStream();
            var sr = new StreamReader(rStream);
            var content = sr.ReadToEnd();
            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            if (result.Success == "true")
            {
                String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                String sql = "SELECT Trial_Participant.Id FROM Trial_Participant INNER JOIN Trial ON Trial_Participant.Trial_Id = Trial.Id INNER JOIN Participant ON Trial_Participant.Participant_Id = Participant.Id INNER JOIN Person ON Participant.Person_Id = Person.Id WHERE Trial.Id = '" + trialId + "' AND Person.Username = '" + username + "';";
                SqlCommand cmd = new SqlCommand(sql, conn);
                int trialParticipantId = (int)cmd.ExecuteScalar();

                List<String> questionIds = new List<String>();
                List<String> questionTitles = new List<String>();
                sql = "SELECT * FROM Intervention_Area_Test_Question WHERE Intervention_Area_Test_Id = '" + testId + "' ORDER BY Sequence;";
                cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    questionIds.Add(reader["Id"].ToString());
                    questionTitles.Add(reader["Question"].ToString());
                }
                for (int i = 0; i < result.Results.Count; i++)
                {

                    DateTime timestamp = DateTime.ParseExact(result.Results[i].Timestamp.ToString(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    int Trial_Participant_Intervention_Area_Test_Id = insertTestCompleted(trialParticipantId, testId, timestamp); //inserts test details
                    for (int j = 0; j < questionIds.Count; j++)
                    {
                        if (questionTitles[j] == "q1")
                        {
                            insertAnswer(Convert.ToInt32(questionIds[j]), Trial_Participant_Intervention_Area_Test_Id, result.Results[i].Question1.ToString());
                        }
                        if (questionTitles[j] == "q2")
                        {
                            insertAnswer(Convert.ToInt32(questionIds[j]), Trial_Participant_Intervention_Area_Test_Id, result.Results[i].Question2.ToString());
                        }
                        if (questionTitles[j] == "s1")
                        {
                            insertAnswer(Convert.ToInt32(questionIds[j]), Trial_Participant_Intervention_Area_Test_Id, result.Results[i].Score1.ToString());
                        }
                        if (questionTitles[j] == "s2")
                        {
                            insertAnswer(Convert.ToInt32(questionIds[j]), Trial_Participant_Intervention_Area_Test_Id, result.Results[i].Score2.ToString());
                        }
                    }
                }

                ViewBag.result = "Result retrieval succeeded";
            }
            else if (result.Sucess == "false")
            {
                ViewBag.result = "Result retrieval failed";
            }

            return View();
        }

        public ActionResult SubmitExternalLogin(ExternalLoginViewModel ExternalLogin, int testId)
        {
            ExternalLogin.request = "getAPIKey";
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string predata = serializer.Serialize(ExternalLogin);
            var data = Encoding.ASCII.GetBytes(predata);
            var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8080/externalapi");
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            var response = request.GetResponse();
            var rStream = response.GetResponseStream();
            var sr = new StreamReader(rStream);
            var content = sr.ReadToEnd();
            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            if (result.Success == "true")
            {
                String username = System.Web.HttpContext.Current.User.Identity.Name;
                String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                String sql = "SELECT Id FROM Person WHERE Username = '" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, conn);
                int personId = (int)cmd.ExecuteScalar();

                sql = "SELECT Id FROM Participant WHERE Person_Id = " + personId;
                cmd = new SqlCommand(sql, conn);
                int participantId = (int)cmd.ExecuteScalar();

                string api_key = result.APIKey.ToObject<string>();

                sql = "INSERT INTO Participant_Intervention_Area_API_Key (Participant_Id, Intervention_Area_Test_Id, API_key) VALUES (" + participantId + ", " + testId + ", '" + api_key + "')";
                cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            else if (result.Success == "false")
            {
                ViewBag.result = "failed";
            }
            return Redirect(Request.UrlReferrer.ToString());
        }


        /* Inserts into Trial_Participant_Intervention_Area_Test 
         * to record details of a completed test */
        private int insertTestCompleted(int Trial_Participant_Id, int Intervention_Area_Test_Id, DateTime timestamp)
        {
            String connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            String sql = "INSERT INTO Trial_Participant_Intervention_Area_Test (Trial_Participant_Id, Intervention_Area_Test_Id, DateTaken) OUTPUT INSERTED.ID values (@Trial_Participant_Id, @Intervention_Area_Test_Id, @DateTaken)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Trial_Participant_Id", SqlDbType.Int).Value = Trial_Participant_Id;
            cmd.Parameters.Add("@Intervention_Area_Test_Id", SqlDbType.Int).Value = Intervention_Area_Test_Id;
            cmd.Parameters.Add("@DateTaken", SqlDbType.DateTime).Value = timestamp;
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