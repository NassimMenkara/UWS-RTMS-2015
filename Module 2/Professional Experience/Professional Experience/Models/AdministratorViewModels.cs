using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Professional_Experience.Models
{
    public class ReportViewModel
    {
        [Required]
        public int Intervention { get; set; }
        [Required]
        public int FilterSelection { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class ReportTestModel
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public int CompletionCount { get; set; }
        public List<ReportQuestionModel> Questions { get; set; } 
    }

    public class ReportQuestionModel
    {
        public int Id { get; set; }
        public String Question { get; set; }
        public int Question_Type { get; set; }
        public List<KeyValuePair<String, int>> Answers { get; set; }
    }
    //public class Intervention
    //{
    //    public String Intervention_Name { get; private set; }
    //    public String Intervention_Description { get; private set; }
    //    public int[] Investigators { get; private set; }
    //    public Test[] Tests { get; private set; }
    //}

    //public class Test
    //{
    //    public int Test_Id { get; private set; }
    //    public String Test_Name { get; private set; }
    //    public String Test_Description { get; private set; }
    //    public Question[] Questions { get; private set; }
    //}

    //public class Question
    //{
    //    public int Question_Id { get; private set; }
    //    public String Question_Title { get; private set; }
    //    public String Answer_Type { get; private set; }
    //    public String[] Answers { get; private set; }
    //}
}