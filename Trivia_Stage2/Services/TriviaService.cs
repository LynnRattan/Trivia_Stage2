using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using Trivia_Stage2.Models;
using System.Net;

namespace Trivia_Stage2.Services
{
    public class TriviaService
    {
        HttpClient httpClient;//אובייקט לשליחת בקשות וקבלת תשובות מהשרת

        JsonSerializerOptions options;//פרמטרים שישמשו אותנו להגדרות הjson

        const string URL = $@"https://qsc714b9-7128.euw.devtunnels.ms/";//כתובת השרת

        public TriviaService()
        {
            //http client
            httpClient = new HttpClient();

            //options when doing serialization/deserialization
            options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<Question> GetRandomQuestion()
        {
            Question q = null;
            HttpResponseMessage response = await httpClient.GetAsync($"{URL}question/Any?safe-mode");

            //if(response.StatusCode==System.Net.HttpStatusCode.OK)

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();

                q = JsonSerializer.Deserialize<Question>(jsonString, options);


                JsonNodeOptions nodeOptions = new JsonNodeOptions() { PropertyNameCaseInsensitive = true };
                JsonNode node = JsonNode.Parse(jsonString, nodeOptions);
                {
                    if (node["error"].GetValue<bool>() == true)
                    {
                        q = new Question()
                        {
                            ServiceError = JsonSerializer.Deserialize<ServiceError>(jsonString)
                        };
                    }
                    else
                        q = JsonSerializer.Deserialize<Question>(jsonString, options);


                }

            }
            return q;
        }

        public async Task<bool> SubmitQuestionAsync(MyQuestion q)
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            //serialize the object
            string jsonString = JsonSerializer.Serialize(q, options);//serialize

            //insert it into a HttpString Container
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            //send it to the server
            var response = await httpClient.PostAsync($"{URL}Submit?dry-run", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return true;
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return false;
            return false;

        }

        public async Task<Player> LogPlayer(string username, string password)
        {
            var login = new Login() { Username = username, Password = password };
            try
            {
                string jsonContent = JsonSerializer.Serialize(login, options);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application");
                var response = await httpClient.PostAsync($@"{URL}Login", content);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        jsonContent = await response.Content.ReadAsStringAsync();
                        var player = JsonSerializer.Deserialize<Player>(jsonContent, options);
                        return player;
                        break;
                    case HttpStatusCode.Unauthorized:
                        return null;

                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }
    }

}
