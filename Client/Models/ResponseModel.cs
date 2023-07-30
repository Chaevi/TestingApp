using System.Collections.Generic;

namespace Client.Models
{
    internal class ResponseModel
    {
        public ResponseType ResponseType { get; set; }
        public List<string> ListTest { get; set; }
        public TestModel Test { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum ResponseType
    {
        ListTest,
        TestData,
        Success,
        Error
    }
}
