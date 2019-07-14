using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestApiTestAutomation.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestApiTestAutomation.Tools
{
    internal class HttpTool
    {
        internal HttpClient CreateClient(string baseAddressUri, string acceptHeader)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddressUri)
            };
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));
            return client;
        }

        internal HttpResponseMessage MakeRequestToServer(HttpClient client, HttpMethod httpMethod, string uriRequest, HttpContent httpContent=null)
        {
            Task<HttpResponseMessage> responseTask = null;
            System.Net.HttpStatusCode expectedHttpStatusCode = System.Net.HttpStatusCode.OK;
            switch (httpMethod)
            {
                case HttpMethod m when m == HttpMethod.Get:
                    responseTask = client.GetAsync(uriRequest);
                    break;
                case HttpMethod m when m == HttpMethod.Post:
                    responseTask = client.PostAsync(uriRequest, httpContent);
                    expectedHttpStatusCode = System.Net.HttpStatusCode.Created;
                    break;
                case HttpMethod m when m == HttpMethod.Put:
                    responseTask = client.PutAsync(uriRequest, httpContent);
                    expectedHttpStatusCode = System.Net.HttpStatusCode.NoContent;
                    break;
                case HttpMethod m when m == HttpMethod.Patch:
                    responseTask = client.PatchAsync(uriRequest, httpContent);
                    expectedHttpStatusCode = System.Net.HttpStatusCode.NoContent;
                    break;
                case HttpMethod m when m == HttpMethod.Delete:
                    responseTask = client.DeleteAsync(uriRequest);
                    break;
                default:
                    break;
            }
            
            responseTask.Wait();

            var httpResponseMessage = responseTask.Result;

            Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode, httpResponseMessage.StatusCode.ToString());
            Assert.IsTrue(httpResponseMessage.StatusCode == expectedHttpStatusCode, httpResponseMessage.StatusCode.ToString());

            return httpResponseMessage;
        }

        internal string CreateAndPostRandomUser(string baseAddressUri, string acceptHeader)
        {
            var randomNumber = new Random().Next(1000, 9999);
            var randomUserName = $"RandomUser{randomNumber}";
            var newUser = new User() { Id = "0", Name = randomUserName, Age = 20, Location = "NY", Work = new Work() { Name = "Sela", Location = "BB", Rating = 5 } };

            string collection = "users";
            string uriRequestPost = $"api/{collection}";

            var client = CreateClient(baseAddressUri, acceptHeader);
            var jsonUser = JsonConvert.SerializeObject(newUser);
            var httpContent = new StringContent(jsonUser.ToString(), Encoding.UTF8, "application/json");

            var httpResponseMessagePost = MakeRequestToServer(client, HttpMethod.Post, uriRequestPost, httpContent);
            var readTask = httpResponseMessagePost.Content.ReadAsStringAsync();
            readTask.Wait();

            var responseBodyAfterPost = JsonConvert.DeserializeObject<JsonResponse>(readTask.Result);
            return responseBodyAfterPost.Id;
        }

        internal User GetUserById(HttpClient client, string userId)
        {
            var uriRequestGet = $"api/users/{userId}";
            var httpResponseMessageGet = MakeRequestToServer(client, HttpMethod.Get, uriRequestGet);
            var readTask = httpResponseMessageGet.Content.ReadAsStringAsync();
            readTask.Wait();
            var userAfterGet = JsonConvert.DeserializeObject<User>(readTask.Result);
            return userAfterGet;
        }

        private static StringContent ConvertJsonToHttpContent(string jsonObject)
        {
            return new StringContent(jsonObject, Encoding.UTF8, "application/json");
        }

        public static HttpContent ConvertObjectToHttpContent(object objectToConvert)
        {
            var jsonObject = JsonConvert.SerializeObject(objectToConvert);
            HttpContent httpContent = ConvertJsonToHttpContent(jsonObject);
            return httpContent;
        }

        public static Task<string> ReadContentFromMessage(HttpResponseMessage httpResponseMessage)
        {
            var readTask = httpResponseMessage.Content.ReadAsStringAsync();
            readTask.Wait();
            return readTask;
        }


    }
}