using Client.Controllers;
using Client.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    public partial class MainWindow : Window
    {
        public ClientController clientMain;
        public UserModel user;
        public string testResults;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Login
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;

            // Создаем пользователя и назначаем данные
            user = new UserModel();

            string name = NameTextBox.Text;
            string age = AgeTextBox.Text;
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Проверка на заполнение данных
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(age) || string.IsNullOrEmpty(gender))
            {
                ErrorLabel.Content = "Пожалуйста, заполните все поля";
                ErrorLabel.Visibility = Visibility.Visible;
            }
            else
            {
                // Попытка подключения и отправки своих данных
                try
                {
                    user.Name = name;
                    user.Age = int.Parse(age);
                    user.Gender = gender;

                    clientMain = new ClientController();
                    clientMain.ConnectToServer(user);

                    ShowTestSelection();
                }
                catch (Exception ex)
                {
                    ErrorLabel.Content = "Произошла ошибка при входе\n" + ex.Message;
                    ErrorLabel.Visibility = Visibility.Visible;
                }
            }
        }

        private void AgeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion

        #region TestSelect
        void ShowTestSelection()
        {
            // Переключение на вид выбора теста
            ErrorLabel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Collapsed;
            TestSelection.Visibility = Visibility.Visible;
            TestComboBox.Items.Clear();

            try
            {
                // Получаем список тестов
                List<string> tests = clientMain.GetListTestFromServer();

                // Добавляем в ComboBox
                foreach (var test in tests)
                {
                    TestComboBox.Items.Add(test);
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = "Произошла ошибка при получении тестов\n" + ex.Message;
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;

            // Проверка на выбор теста
            if (string.IsNullOrEmpty(TestComboBox.Text))
            {
                ErrorLabel.Content = "Выберите тест";
                ErrorLabel.Visibility = Visibility.Visible;
                return;
            }

            // Попытка получить тест и открыть окно тестирования
            try
            {
                TestModel test = clientMain.GetTestFromServer(TestComboBox.SelectedItem.ToString());
                TestingWindow window = new TestingWindow(this);
                window.Test = test;
                window.Show();
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = "Произошла ошибка при получении теста\n" + ex.Message;
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            // Отключение от сервера и переключение на вид входа
            ErrorLabel.Visibility = Visibility.Collapsed;

            clientMain.Disconnect();
            LoginPanel.Visibility = Visibility.Visible;
            TestSelection.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
