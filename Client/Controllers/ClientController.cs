using Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Controllers
{
    public class ClientController
    {
        private const string serverIP = "127.0.0.1"; // Заменить на IP-адрес вашего сервера
        private const int port = 8888;

        // Буфер для получения длинны сообщения от сервера
        private byte[] lengthBytes = new byte[4];

        private Socket client;

        public void ConnectToServer(UserModel user)
        {
            // Попытка подключения и отправки данных о себе
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Подключаемся к серверу
                client.Connect(new IPEndPoint(IPAddress.Parse(serverIP), port));

                // Отправляем данные серверу
                string json = JsonConvert.SerializeObject(user, Formatting.Indented);
                SendJSON(client, json);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<string> GetListTestFromServer()
        {
            try
            {
                // Отправляем запрос на получение списка тестов
                RequestModel request = new RequestModel { RequestType = RequestType.GetListOfTests };
                string requestJson = JsonConvert.SerializeObject(request);
                SendJSON(client, requestJson);

                // Принимаем ответ
                string responseJson = ReceiveJSON(client);
                ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(responseJson);

                // Проверка на ошибку в отправке
                if (response.ResponseType == ResponseType.Error)
                    throw new Exception(response.ErrorMessage);

                List<string> testsList = response.ListTest;
                return testsList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public TestModel GetTestFromServer(string name)
        {
            try
            {
                // Отправляем запрос на получение теста
                RequestModel request = new RequestModel { RequestType = RequestType.GetTest, NameOfTest = name };
                string requestJson = JsonConvert.SerializeObject(request);
                SendJSON(client, requestJson);

                // Принимаем ответ
                string responseJson = ReceiveJSON(client);
                ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(responseJson);

                // Проверка на ошибку в отправке
                if (response.ResponseType == ResponseType.Error)
                    throw new Exception(response.ErrorMessage);

                return response.Test;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string SendTestResult(TestResultModel testResultData)
        {
            // Отправляем результаты теста
            RequestModel request = new RequestModel { RequestType = RequestType.TestResult, TestResultData = testResultData };
            string requestJson = JsonConvert.SerializeObject(request);
            SendJSON(client, requestJson);

            // Принимаем статус
            string responseJson = ReceiveJSON(client);
            ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(responseJson);

            // Проверка на ошибку в отправке
            if (response.ResponseType == ResponseType.Success)
                return "Тест успешно сохранен";
            else
                return response.ErrorMessage;
        }

        public void Disconnect()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        void SendJSON(Socket socket, string json)
        {
            // Отправляем данные серверу
            byte[] responseData = Encoding.UTF8.GetBytes(json);
            socket.Send(responseData);
        }

        string ReceiveJSON(Socket clientSocket)
        {
            // Принимаем длину сообщения (4 байта)
            clientSocket.Receive(lengthBytes);

            // Преобразуем байты в значение типа int (длина сообщения)
            int messageLength = BitConverter.ToInt32(lengthBytes, 0);

            // Буфер для принимаемых данных с учетом длины сообщения
            byte[] bufferMessage = new byte[messageLength];

            // Читаем данные от сервера
            int bytesRead = clientSocket.Receive(bufferMessage);

            return Encoding.UTF8.GetString(bufferMessage, 0, bytesRead);
        }
    }
}
