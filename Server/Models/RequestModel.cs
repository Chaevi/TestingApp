namespace Server.Models
{
    internal class RequestModel
    {
        public RequestType RequestType { get; set; }
        public string NameOfTest { get; set; }
        public TestResultModel TestResultData { get; set; }
    }

    public enum RequestType
    {
        GetListOfTests,
        GetTest,
        TestResult
    }
}
