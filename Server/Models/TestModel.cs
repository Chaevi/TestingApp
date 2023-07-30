using System.Collections.Generic;

namespace Server.Models
{
    public class TestModel
    {
        public string Title { get; set; }
        public List<Question> Questions { get; set; }
    }

    public class Question
    {
        public Question(string type, string text, List<string> options)
        {
            Type = type;
            Text = text;
            Options = options;
        }

        public string Type { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
    }
}
