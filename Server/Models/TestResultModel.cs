namespace Server.Models
{
    public class TestResultModel
    {
        public UserModel User { get; set; }
        public string NameOfTest { get; set; }
        public string Date { get; set; }
        public string Answers { get; set; }
    }
}
