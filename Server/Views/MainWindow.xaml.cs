using Server.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Server
{
    public partial class MainWindow : Window
    {
        private ServerController serverMain = new ServerController();

        public DatabaseController databaseManager;

        public MainWindow()
        {
            InitializeComponent();

            databaseManager = serverMain.databaseManager;

            RefreshComboBox();
        }

        public void RefreshComboBox()
        {
            SelectionTests.Items.Clear();

            List<string> items = databaseManager.GetListOfTests();

            foreach (string item in items)
            {
                SelectionTests.Items.Add(item);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Запуск сервера
            StartButton.IsEnabled = false;
            Task.Run(() => serverMain.Start());
            StatusLabel.Foreground = Brushes.Green;
            StatusLabel.Content = "Запущен";

            // Подписка к логам сервера
            serverMain.LogReceived += Server_LogReceived;
        }

        private void Server_LogReceived(object sender, string log)
        {
            // Используем Dispatcher для обновления элементов пользовательского интерфейса из потока UI
            Dispatcher.Invoke(() =>
            {
                // Обновляем содержимое TextBox с логами
                LogTextBox.AppendText(log + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TableNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста введите название в поле");
                return;
            }

            try
            {
                databaseManager.CreateTestTable(TableNameTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            SelectionTests.Items.Clear();

            List<string> items = databaseManager.GetListOfTests();

            foreach (string item in items)
            {
                SelectionTests.Items.Add(item);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectionTests.Text))
            {
                MessageBox.Show("Пожалуйста выберите тест");
                return;
            }
            EditorWindow editorWindow = new EditorWindow(this);
            editorWindow.test = databaseManager.GetTest(SelectionTests.Text);
            editorWindow.Show();
        }
    }
}
