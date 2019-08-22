using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestApiTestAutomation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestApiTestAutomation.Tools
{
    internal static class HttpTool
    {
        internal static HttpClient CreateClient(string baseAddressUri, string acceptHeader)
        {
            // DEMO 03: Setting of Base Address
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddressUri)
            };

            // DEMO 04: Adding default Headers
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));
            return client;
        }

        internal static HttpResponseMessage MakeRequestToServer(HttpClient client, HttpMethod httpMethod, string uriRequest, HttpContent httpContent = null, bool toValidateStatusCode = true)
        {
            Task<HttpResponseMessage> responseTask = null;
            System.Net.HttpStatusCode expectedHttpStatusCode = System.Net.HttpStatusCode.OK;
            // DEMO 09: Determine which Request to send to the URI, by HTTP-Method type, Asynchronous operation
            switch (httpMethod)
            {
                case HttpMethod m when m == HttpMethod.Get:
                    responseTask = client.GetAsync(uriRequest);
                    expectedHttpStatusCode = System.Net.HttpStatusCode.OK;
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
                    expectedHttpStatusCode = System.Net.HttpStatusCode.NoContent;
                    break;
                default:
                    break;
            }

            // DEMO 10: Wait for Asynchronous Operation (Task) to finish
            responseTask.Wait();

            // DEMO 11: Extract of the Response Message
            var httpResponseMessage = responseTask.Result;

            // DEMO 12: Validation of Response Message
            if (toValidateStatusCode)
                ValidateStatusCode(httpMethod, expectedHttpStatusCode, httpResponseMessage);

            PrintToLogOnFailure(httpMethod, expectedHttpStatusCode, httpResponseMessage);

            return httpResponseMessage;
        }

        private static void PrintToLogOnFailure(HttpMethod httpMethod, System.Net.HttpStatusCode expectedHttpStatusCode, HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.IsSuccessStatusCode == false)
                Console.WriteLine($"Failure in Api Method {httpMethod.Method}, StatusCode failed: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}");
            if (httpResponseMessage.StatusCode == expectedHttpStatusCode)
                Console.WriteLine($"Failure in Api Method {httpMethod.Method}, Expected StatusCode = {expectedHttpStatusCode}, Actual StatusCode = {httpResponseMessage.StatusCode}");
        }

        private static void ValidateStatusCode(HttpMethod httpMethod, System.Net.HttpStatusCode expectedHttpStatusCode, HttpResponseMessage httpResponseMessage)
        {
            Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode, $"Failure in Api Method {httpMethod.Method}, StatusCode failed: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}");
            Assert.IsTrue(httpResponseMessage.StatusCode == expectedHttpStatusCode, $"Failure in Api Method {httpMethod.Method}, Expected StatusCode = {expectedHttpStatusCode}, Actual StatusCode = {httpResponseMessage.StatusCode}");
        }

        internal static HttpResponseMessage EnsureObjectIsNotFound(HttpClient client, string uriRequest)
        {
            Task<HttpResponseMessage> responseTask = client.GetAsync(uriRequest);
            System.Net.HttpStatusCode expectedHttpStatusCode = System.Net.HttpStatusCode.NotFound;

            responseTask.Wait();

            var httpResponseMessage = responseTask.Result;

            Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode == false, httpResponseMessage.ReasonPhrase);
            Assert.IsTrue(httpResponseMessage.StatusCode == expectedHttpStatusCode, httpResponseMessage.StatusCode.ToString());

            return httpResponseMessage;
        }

        internal static int CreateAndPostRandomUser(string baseAddressUri, string acceptHeader)
        {
            var randomNumber = new Random().Next(1000, 9999);
            var randomUserName = $"RandomUser{randomNumber}";
            //var newUser = new User() { Id = "0", Name = randomUserName, Age = 20, Location = "NY", Work = new Work() { Name = "Sela", Location = "BB", Rating = 5 } };
            var newUser = new User() { Name = randomUserName, Age = 20, Location = "NY", Work = new Work() { Name = "Sela", Location = "BB", Rating = 5 } };

            string collection = "users";
            string uriRequestPost = $"api/{collection}";

            var client = CreateClient(baseAddressUri, acceptHeader);
            var jsonUser = JsonConvert.SerializeObject(newUser);
            var httpContent = new StringContent(jsonUser.ToString(), Encoding.UTF8, "application/json");

            var httpResponseMessagePost = MakeRequestToServer(client, HttpMethod.Post, uriRequestPost, httpContent);
            var responseUserAfterPost = DeserializeFromResponseMessage<User>(httpResponseMessagePost);
            return responseUserAfterPost.Id;
        }

        public static void DeleteUser(HttpClient client, int userId)
        {
            string uriRequestDelete = $"api/users/{userId}";
            var httpResponseMessagePost = MakeRequestToServer(client, HttpMethod.Delete, uriRequestDelete);
            ReadContentFromMessage(httpResponseMessagePost);
        }

        internal static User GetUserById(HttpClient client, int userId)
        {
            var uriRequestGet = $"api/users/{userId}";
            var httpResponseMessageGet = MakeRequestToServer(client, HttpMethod.Get, uriRequestGet);
            User userAfterGet = DeserializeFromResponseMessage<User>(httpResponseMessageGet);
            return userAfterGet;
        }

        public static T DeserializeFromResponseMessage<T>(HttpResponseMessage httpResponseMessage)
        {
            // DEMO 14: Read Content and Deserialization by specific Class
            Task<string> readTask = ReadContentFromMessage(httpResponseMessage);
            var deserializedObject = JsonConvert.DeserializeObject<T>(readTask.Result);
            return deserializedObject;
        }

        private static StringContent ConvertJsonToHttpContent(string jsonObject)
        {
            return new StringContent(jsonObject, Encoding.UTF8, "application/json");
        }

        public static HttpContent ConvertObjectToHttpContent(object objectToConvert)
        {
            // DEMO 06: Newtonsoft Package - Json serialization
            var jsonObject = JsonConvert.SerializeObject(objectToConvert);
            // DEMO 07: Conversion Json Object to HTTP Content (Entity Body and Content Headers)
            HttpContent httpContent = ConvertJsonToHttpContent(jsonObject);
            return httpContent;
        }

        public static Task<string> ReadContentFromMessage(HttpResponseMessage httpResponseMessage)
        {
            // DEMO 15: Asynchronous operation of reading string text from Content
            var readTask = httpResponseMessage.Content.ReadAsStringAsync();
            readTask.Wait();
            return readTask;
        }

    }
}