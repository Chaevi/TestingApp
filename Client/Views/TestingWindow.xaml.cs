using Client.Models;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Client
{
    public partial class TestingWindow : Window
    {
        MainWindow mainWindow;

        public TestModel Test;

        private int currentQuestion = 1;

        Dictionary<int, List<int>> currentAnswers;

        List<int> options;

        TestResultModel testResult;

        public TestingWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            currentAnswers = new Dictionary<int, List<int>>();
            NameOfTestLabel.Text = Test.Title;
            FillDinctionaryOfAnswers();
            RenderTest(Test.Questions[currentQuestion - 1]);
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestion > 1)
            {
                SaveChoice();
                currentQuestion--;
                RenderTest(Test.Questions[currentQuestion - 1]);
                SetChoice();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestion < Test.Questions.Count)
            {
                SaveChoice();
                currentQuestion++;
                RenderTest(Test.Questions[currentQuestion - 1]);
                SetChoice();
            }
        }

        private void FillDinctionaryOfAnswers()
        {
            options = new List<int> { 0, 0, 0, 0 };
            for (int i = 0; i < Test.Questions.Count; i++)
            {
                currentAnswers.Add(i, options);
            }
        }

        private void SetChoice()
        {
            if (Test.Questions[currentQuestion - 1].Type == "SingleChoice")
            {
                options = currentAnswers[currentQuestion - 1];

                Rb1.IsChecked = Convert.ToBoolean(options[0]);
                Rb2.IsChecked = Convert.ToBoolean(options[1]);
                Rb3.IsChecked = Convert.ToBoolean(options[2]);
                Rb4.IsChecked = Convert.ToBoolean(options[3]);
            }
            if (Test.Questions[currentQuestion - 1].Type == "MultipleChoice")
            {
                options = currentAnswers[currentQuestion - 1];

                Cb1.IsChecked = Convert.ToBoolean(options[0]);
                Cb2.IsChecked = Convert.ToBoolean(options[1]);
                Cb3.IsChecked = Convert.ToBoolean(options[2]);
                Cb4.IsChecked = Convert.ToBoolean(options[3]);
            }
        }

        private void SaveChoice()
        {
            if (Test.Questions[currentQuestion - 1].Type == "SingleChoice")
            {
                int a, b, c, d;
                a = Rb1.IsChecked.Value ? 1 : 0;
                b = Rb2.IsChecked.Value ? 1 : 0;
                c = Rb3.IsChecked.Value ? 1 : 0;
                d = Rb4.IsChecked.Value ? 1 : 0;

                options = new List<int> { a, b, c, d };
            }
            if (Test.Questions[currentQuestion - 1].Type == "MultipleChoice")
            {
                int a, b, c, d;
                a = Cb1.IsChecked.Value ? 1 : 0;
                b = Cb2.IsChecked.Value ? 1 : 0;
                c = Cb3.IsChecked.Value ? 1 : 0;
                d = Cb4.IsChecked.Value ? 1 : 0;

                options = new List<int> { a, b, c, d };
            }
            currentAnswers[currentQuestion - 1] = options;
        }

        private void RenderTest(Question question)
        {
            RadioButtonGroup.Visibility = Visibility.Collapsed;
            CheckBoxGroup.Visibility = Visibility.Collapsed;

            QuestionLabel.Text = question.Text;
            CounterLabel.Text = $"Вопрос {currentQuestion}/{Test.Questions.Count}";

            switch (question.Type)
            {
                case "SingleChoice":
                    {
                        Rb1.Content = question.Options[0];
                        Rb2.Content = question.Options[1];
                        Rb3.Content = question.Options[2];
                        Rb4.Content = question.Options[3];

                        RadioButtonGroup.Visibility = Visibility.Visible;
                    }
                    break;
                case "MultipleChoice":
                    {
                        Cb1.Content = question.Options[0];
                        Cb2.Content = question.Options[1];
                        Cb3.Content = question.Options[2];
                        Cb4.Content = question.Options[3];

                        CheckBoxGroup.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    break;
            }
        }

        private void SaveResult(Dictionary<int, List<int>> answers)
        {
            string answersString = "";
            foreach (var answer in answers)
            {
                string options = "";
                for (int i = 0; i < answer.Value.Count; i++)
                {
                    if (answer.Value[i] == 1)
                        options += i + 1;
                }
                answersString += $"{answer.Key + 1}:{options}|";
            }
            testResult = new TestResultModel { user = mainWindow.user, NameOfTest = Test.Title, Date = DateTime.Now.ToString(), Answers = answersString };
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            SaveChoice();
            SaveResult(currentAnswers);
            MessageBox.Show(mainWindow.clientMain.SendTestResult(testResult));
            this.Close();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}
