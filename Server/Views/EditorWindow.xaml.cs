using Server.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Server
{
    public partial class EditorWindow : Window
    {
        public TestModel test; // Для использования данных выбраного теста

        private int currentQuestion = 1; // Переменная для навигации по вопросам

        MainWindow window; // Для получения ссылки на DatabaseController

        public EditorWindow(MainWindow window)
        {
            this.window = window;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Отображаем первый вопрос
            NameOfTestTextBox.Text = test.Title;
            if (test.Questions.Count > 0)
            {
                RenderTest(test.Questions[currentQuestion - 1]);
                RefreshListBox();
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestion > 1)
            {
                currentQuestion--;
                RenderTest(test.Questions[currentQuestion - 1]);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestion < test.Questions.Count)
            {
                currentQuestion++;
                RenderTest(test.Questions[currentQuestion - 1]);
            }
        }

        private void SaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполненость полей
            if (string.IsNullOrEmpty(QuestionTextBox.Text) ||
                string.IsNullOrEmpty(QuestionTypeComboBox.Text) ||
                string.IsNullOrEmpty(Rb1.Text) ||
                string.IsNullOrEmpty(Rb2.Text) ||
                string.IsNullOrEmpty(Rb3.Text) ||
                string.IsNullOrEmpty(Rb4.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            // Собираем данные выбора
            string Type = QuestionTypeComboBox.Text;
            string Text = QuestionTextBox.Text;
            List<string> Options = new List<string> { Rb1.Text, Rb2.Text, Rb3.Text, Rb4.Text };

            // Если вопросов нет - добавляем
            if (test.Questions.Count < 1)
            {
                test.Questions.Add(new Question(Type, Text, Options));
                RefreshListBox();
                RenderTest(test.Questions[currentQuestion - 1]);
                return;
            }

            // Сохраняем
            test.Questions[currentQuestion - 1] = new Question(Type, Text, Options);
        }

        private void ResetQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (test.Questions.Count < 1)
                return;
            RenderTest(test.Questions[currentQuestion - 1]);
        }

        private void RefreshListBox()
        {
            ListBoxQuestions.Items.Clear();

            for (int i = 1; i < test.Questions.Count + 1; i++)
            {
                ListBoxQuestions.Items.Add($"Вопрос {i}");
            }
        }

        private void RenderTest(Question question)
        {
            QuestionTextBox.Text = question.Text;
            CounterLabel.Text = $"Вопрос {currentQuestion}/{test.Questions.Count}";

            Rb1.Text = question.Options[0];
            Rb2.Text = question.Options[1];
            Rb3.Text = question.Options[2];
            Rb4.Text = question.Options[3];

            foreach (ComboBoxItem item in QuestionTypeComboBox.Items)
            {
                if (item.Content.ToString() == question.Type)
                {
                    QuestionTypeComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ListBoxQuestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int questionNumber = ListBoxQuestions.SelectedIndex;
            if (questionNumber == -1)
                return;

            currentQuestion = questionNumber + 1;
            RenderTest(test.Questions[currentQuestion - 1]);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            test.Questions.Add(new Question("SingleChoice", "Text", new List<string> { "Op1", "Op2", "Op3", "Op4" }));

            RefreshListBox();

            RenderTest(test.Questions[currentQuestion - 1]);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Если вопросов нет - ничего не удаляем
            if (test.Questions.Count < 1)
                return;

            // Удаляем текущий вопрос
            test.Questions.Remove(test.Questions[currentQuestion - 1]);
            RefreshListBox();

            // Обновление счётчика и переменной currenQuestion
            // При удалении вопроса currentQuestion остается на месте
            if (currentQuestion > test.Questions.Count)
                currentQuestion = test.Questions.Count;
            if (currentQuestion < 1)
            {
                currentQuestion = 1;
                return;
            }

            RenderTest(test.Questions[currentQuestion - 1]);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            test.Title = NameOfTestTextBox.Text;

            try
            {
                window.databaseManager.SaveTable(test);
                window.RefreshComboBox();
                MessageBox.Show("Сохранено успешно");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

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
