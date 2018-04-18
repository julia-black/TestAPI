using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        private const string APP_PATH = "http://localhost:27397";
        private static string token;

        static void Main(string[] args)
        {

            Console.WriteLine("Если вы хотите зарегаться - нажмите 1, если войти - 2");
            string temp = Console.ReadLine();
            Console.WriteLine("Введите логин:");
            string userName = Console.ReadLine();

            Console.WriteLine("Введите пароль:");
            string password = Console.ReadLine();
            if (temp == "1")
            {
                var registerResult = Register(userName, password);
                Console.WriteLine("Статусный код регистрации: {0}", registerResult);

                Console.WriteLine("Вход...");
                Dictionary<string, string> tokenDictionary = Login(userName, password);
                token = tokenDictionary["access_token"];    
            }
            else if (temp == "2")
            {
                Dictionary<string, string> tokenDictionary = Login(userName, password);
                token = tokenDictionary["access_token"];

            }
            Console.WriteLine();
            Console.WriteLine("Access Token:");
            Console.WriteLine(token);
            temp = "0";

            while (temp != "q")
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1 - показать values");
                Console.WriteLine("2 - показать user info");
                Console.WriteLine("q - выход");

                temp = Console.ReadLine();

                if (temp == "1")
                {
                    Console.WriteLine();
                    string values = GetValues(token);
                    Console.WriteLine("Values:");
                    Console.WriteLine(values);
                }
                else if (temp == "2")
                {
                    Console.WriteLine();
                    string userInfo = GetUserInfo(token);
                    Console.WriteLine("Пользователь:");
                    Console.WriteLine(userInfo);
                }
            }
        }
        static string Register(string userName, string password)
        {
            var registerModel = new
            {
                UserName = userName,
                Password = password,
                ConfirmPassword = password
            };
            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(APP_PATH + "/api/Account/Register", registerModel).Result;
                return response.StatusCode.ToString();
            }
        }

        static Dictionary<string, string> Login(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "grant_type", "password" ), 
                    new KeyValuePair<string, string>( "username", userName ), 
                    new KeyValuePair<string, string> ( "Password", password )
                };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var response =
                    client.PostAsync(APP_PATH + "/Token", content).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                // Десериализация полученного JSON-объекта
                Dictionary<string, string> tokenDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                return tokenDictionary;
            }
        }
      
        // создаем http-клиента с токеном 
        static HttpClient CreateClient(string accessToken = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
            return client;
        }

        // получаем информацию о клиенте 
        static string GetUserInfo(string token)
        {
            using (var client = CreateClient(token))
            {
                var response = client.GetAsync(APP_PATH + "/api/Account/UserInfo").Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        // обращаемся по маршруту api/values 
        static string GetValues(string token)
        {
            using (var client = CreateClient(token))
            {
                var response = client.GetAsync(APP_PATH + "/api/values").Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
