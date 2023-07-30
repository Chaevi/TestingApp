using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    internal class ServerController
    {
        // Событие для отправки логов
        public event EventHandler<string> LogReceived;

        public DatabaseController databaseManager = new DatabaseController();

        private const int port = 8888;

        // Буфер для получения данных клиента
        private byte[] buffer = new byte[1024];

        Socket serverSocket;

        public async Task Start()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // Привязываем серверный сокет к локальной точке и начинаем слушать порт
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen();

                AddLog("Server started. Waiting for connections...");

                while (true)
                {
                    // Принимаем новое подключение от клиента
                    Socket clientSocket = serverSocket.Accept();

                    // Читаем данные от клиента (имя, возраст, пол)
                    string jsonData = await ReceiveJsonAsync(clientSocket);

                    // Конвертируем JSON в класс User
                    UserModel? user = JsonConvert.DeserializeObject<UserModel>(jsonData);

                    AddLog(clientSocket.RemoteEndPoint + "\nПользователь " + user.Name + " подключился" + "\nВозраст: " + user.Age + "\nПол: " + user.Gender);

                    // Запускаем поток для обслуживания клиента
                    HandleClient(clientSocket);
                }
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);
                throw;
            }
            finally
            {
                serverSocket.Close();
            }
        }

        private async Task HandleClient(Socket socket)
        {
            try
            {
                while (socket.Connected)
                {
                    // Принимаем запрос от клиента
                    string requestJson = await ReceiveJsonAsync(socket);

                    if (string.IsNullOrEmpty(requestJson))
                        break;

                    // Конвертируем в класс
                    RequestModel request = JsonConvert.DeserializeObject<RequestModel>(requestJson);

                    // Обрабатываем запрос
                    ResponseModel response = await ProcessRequest(request);

                    // Конвертируем ответ
                    string responseJson = JsonConvert.SerializeObject(response);

                    // Отправляем ответ
                    await SendJsonAsync(socket, responseJson);

                    AddLog(socket.RemoteEndPoint + " Запрос обработан: " + request.RequestType.ToString());
                }
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);
            }
            finally
            {
                AddLog("Client disconnected: " + socket.RemoteEndPoint);
                socket.Close();
            }
        }

        private async Task SendJsonAsync(Socket receiver, string json)
        {
            try
            {
                // Определяем длину данных и преобразуем ее в байты (int - 4 байта)
                byte[] lengthBytes = BitConverter.GetBytes(Encoding.UTF8.GetByteCount(json));

                // Отправляем клиенту количество байтов сообщения
                await receiver.SendAsync(new ArraySegment<byte>(lengthBytes), SocketFlags.None);

                // Отправляем само сообщение клиенту
                byte[] responseData = Encoding.UTF8.GetBytes(json);
                await receiver.SendAsync(new ArraySegment<byte>(responseData), SocketFlags.None);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<string> ReceiveJsonAsync(Socket clientSocket)
        {
            int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private async Task<ResponseModel> ProcessRequest(RequestModel request)
        {
            switch (request.RequestType)
            {
                case RequestType.GetListOfTests:
                    {
                        // Отправляем клиенту список доступных тестов
                        List<string> ListOfTests = databaseManager.GetListOfTests();
                        return new ResponseModel { ResponseType = ResponseType.ListTest, ListTest = ListOfTests };
                    }
                case RequestType.GetTest:
                    {
                        // Отправляем тест клиенту
                        try
                        {
                            TestModel test = databaseManager.GetTest(request.NameOfTest);
                            if (test.Questions.Count == 0)
                            {
                                throw new Exception("В этом тесте нет вопросов");
                            }
                            return new ResponseModel { ResponseType = ResponseType.TestData, Test = test };
                        }
                        catch (Exception ex)
                        {

                            return new ResponseModel { ResponseType = ResponseType.Error, ErrorMessage = "Что то не так с тестом \n" + ex.Message };
                        }

                    }
                case RequestType.TestResult:
                    {
                        // Сохраняем результаты тестирования
                        databaseManager.SaveResult(request.TestResultData);

                        // Возвращаем подтверждение
                        return new ResponseModel { ResponseType = ResponseType.Success };
                    }
                default:
                    {
                        // Неизвестный тип запроса
                        return new ResponseModel { ResponseType = ResponseType.Error, ErrorMessage = "Неизвестный тип запроса." };
                    }
            }
        }

        private void AddLog(string log)
        {
            LogReceived?.Invoke(this, log);
        }
    }
}
