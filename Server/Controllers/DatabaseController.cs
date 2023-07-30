using Microsoft.Data.Sqlite;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Controllers
{
    public class DatabaseController
    {
        private const string dbPath = "Data Source=ServerDB.db"; // Заменить на вашу строку подключения к SQLite

        public List<string> GetListOfTests()
        {
            // Создаем возвращаемую переменную списка тестов
            List<string> nameTests = new List<string>();

            // Подключение к базе данных
            SqliteConnection connection = new SqliteConnection(dbPath);
            connection.Open();

            // Запрос на получение списка тестов (названия таблиц кроме *_Results и sqlite_sequence)
            SqliteCommand sqliteCommand = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table' AND (SELECT instr(name,'sqlite_sequence') <= 0) AND (SELECT instr(name,'Results') <= 0)", connection);

            // Считывание данных выборки и занесение в возвращаемый список
            SqliteDataReader dataReader = sqliteCommand.ExecuteReader();

            while (dataReader.Read())
            {
                nameTests.Add(dataReader.GetString(0));
            }

            connection.Close();

            return nameTests;
        }

        public void CreateTestTable(string name)
        {
            // Подключение к базе данных
            SqliteConnection connection = new SqliteConnection(dbPath);
            connection.Open();

            // Ищем имеющуюся таблицу в базе данных
            SqliteCommand sqliteCommand = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}'", connection);
            SqliteDataReader reader = sqliteCommand.ExecuteReader();

            // Если такая имеется, удаляем и записываем новую
            if (reader.HasRows)
            {
                reader.Close();
                sqliteCommand = new SqliteCommand($"DROP TABLE IF EXISTS {name}", connection);
                sqliteCommand.ExecuteNonQuery();
            }

            sqliteCommand = new SqliteCommand($"CREATE TABLE {name} (Id INTEGER NOT NULL UNIQUE, Type TEXT NOT NULL, Question TEXT NOT NULL, Op1 TEXT NOT NULL, Op2 TEXT NOT NULL, Op3 TEXT, Op4 TEXT, PRIMARY KEY( Id AUTOINCREMENT));", connection);

            // Попытка выполнения запроса. В случае чего вывод ошибки
            try
            {
                sqliteCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            connection.Close();
        }

        public void CreateResultTable(string name)
        {
            // Подключение к базе данных
            SqliteConnection connection = new SqliteConnection(dbPath);
            connection.Open();

            // Запрос на создание таблицы результатов
            SqliteCommand sqliteCommand = new SqliteCommand($@"
                CREATE TABLE ""{name}_Results"" (
	            ""Id""	INTEGER NOT NULL UNIQUE,
	            ""FullName""	TEXT NOT NULL,
	            ""Age""	TEXT NOT NULL,
	            ""Gender""	TEXT NOT NULL,
	            ""Date""	TEXT NOT NULL,
	            ""Answers""	TEXT NOT NULL,
	            PRIMARY KEY(""Id"" AUTOINCREMENT)
                )", connection);

            // Проверка на ошибки
            try
            {
                sqliteCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            connection.Close();
        }

        public TestModel GetTest(string tableName)
        {
            // Создаем тест
            TestModel test;
            List<Question> questions = new List<Question>();

            // Открываем соединение с SQL
            SqliteConnection connection = new SqliteConnection(dbPath);
            connection.Open();

            // Выбераем таблицу по имени
            string query = $"SELECT * FROM {tableName}";
            SqliteCommand command = new SqliteCommand(query, connection);
            SqliteDataReader reader = command.ExecuteReader();

            // Считываем данные и заносим в переменную tests
            try
            {
                while (reader.Read())
                {
                    questions.Add(new Question(
                        reader["Type"].ToString(),
                        reader["Question"].ToString(),
                        new List<string>
                        {
                                reader["Op1"].ToString(),
                                reader["Op2"].ToString(),
                                reader["Op3"].ToString(),
                                reader["Op4"].ToString()
                        }.Where(o => !string.IsNullOrEmpty(o)).ToList()));
                }
                test = new TestModel
                {
                    Title = tableName,
                    Questions = questions
                };
                connection.Close();
                return test;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SaveTable(TestModel test)
        {
            // Подключение к базе данных
            SqliteConnection connection = new SqliteConnection(dbPath);
            connection.Open();

            // Создаем новую чистую таблицу
            CreateTestTable(test.Title);

            // Записываем новые данные
            SqliteCommand sqliteCommand = new SqliteCommand("", connection);
            try
            {
                foreach (var item in test.Questions)
                {
                    string command = $"INSERT INTO {test.Title} (Type, Question, Op1, Op2, Op3, Op4) " +
                                     $"VALUES ('{item.Type}', '{item.Text}', '{item.Options[0]}', '{item.Options[1]}', '{item.Options[2]}', '{item.Options[3]}')";

                    sqliteCommand.CommandText = command;
                    sqliteCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception)
            {
                connection.Close();
                throw;
            }
        }

        public void SaveResult(TestResultModel result)
        {
            // Подключение к базе данных
            SqliteConnection connection = new SqliteConnection(dbPath);
            connection.Open();

            // Ищем имеющуюся таблицу в базе данных
            SqliteCommand sqliteCommand = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{result.NameOfTest}_Results'", connection);
            SqliteDataReader reader = sqliteCommand.ExecuteReader();

            // Если такой нет, создаем
            if (!reader.HasRows)
            {
                reader.Close();
                CreateResultTable(result.NameOfTest);
            }
            reader.Close();

            // Записываем результаты
            try
            {
                string command = $"INSERT INTO {result.NameOfTest}_Results (FullName, Age, Gender, Date, Answers) " +
                             $"VALUES ('{result.User.Name}', '{result.User.Age}', '{result.User.Gender}', '{result.Date}', '{result.Answers}')";
                sqliteCommand.CommandText = command;
                sqliteCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                connection.Close();
                throw;
            }
        }
    }
}
