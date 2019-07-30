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
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddressUri)
            };
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));
            return client;
        }

        internal static HttpResponseMessage MakeRequestToServer(HttpClient client, HttpMethod httpMethod, string uriRequest, HttpContent httpContent = null, bool toValidateStatusCode = true)
        {
            Task<HttpResponseMessage> responseTask = null;
            System.Net.HttpStatusCode expectedHttpStatusCode = System.Net.HttpStatusCode.OK;
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

            responseTask.Wait();

            var httpResponseMessage = responseTask.Result;

            if (toValidateStatusCode)
                ValidateStatusCode(httpMethod, expectedHttpStatusCode, httpResponseMessage);

            if (httpResponseMessage.IsSuccessStatusCode == false)
                Console.WriteLine($"Failure in Api Method {httpMethod.Method}, StatusCode failed: {httpResponseMessage.StatusCode}");
            if (httpResponseMessage.StatusCode == expectedHttpStatusCode)
                Console.WriteLine($"Failure in Api Method {httpMethod.Method}, Expected StatusCode = {expectedHttpStatusCode}, Actual StatusCode = {httpResponseMessage.StatusCode}");

            return httpResponseMessage;
        }

        private static void ValidateStatusCode(HttpMethod httpMethod, System.Net.HttpStatusCode expectedHttpStatusCode, HttpResponseMessage httpResponseMessage)
        {
            Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode, $"Failure in Api Method {httpMethod.Method}, StatusCode failed: {httpResponseMessage.StatusCode}");
            Assert.IsTrue(httpResponseMessage.StatusCode == expectedHttpStatusCode, $"Failure in Api Method {httpMethod.Method}, Expected StatusCode = {expectedHttpStatusCode}, Actual StatusCode = {httpResponseMessage.StatusCode}");
        }

        internal static HttpResponseMessage EnsureObjectIsNotFound(HttpClient client, string uriRequest)
        {
            Task<HttpResponseMessage> responseTask = client.GetAsync(uriRequest);
            System.Net.HttpStatusCode expectedHttpStatusCode = System.Net.HttpStatusCode.NotFound;

            responseTask.Wait();

            var httpResponseMessage = responseTask.Result;

            Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode == false, httpResponseMessage.StatusCode.ToString());
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
            return responseUserAfterPost.UserId;
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



        public static Exception CreateExceptionFromResponseErrors(HttpResponseMessage response)
        {
            var httpErrorObject = response.Content.ReadAsStringAsync().Result;

            // Create an anonymous object to use as the template for deserialization:
            var anonymousErrorObject = new { message = "", ModelState = new Dictionary<string, string[]>() };

            // Deserialize:
            var deserializedErrorObject = JsonConvert.DeserializeAnonymousType(httpErrorObject, anonymousErrorObject);

            // Now wrap into an exception which best fullfills the needs of your application:
            var ex = new ApiException(response);

            // Sometimes, there may be Model Errors:
            if (deserializedErrorObject.ModelState != null)
            {
                var errors =
                    deserializedErrorObject.ModelState.Select(kvp => string.Join(". ", kvp.Value));
                for (int i = 0; i < errors.Count(); i++)
                {
                    // Wrap the errors up into the base Exception.Data Dictionary:
                    ex.Data.Add(i, errors.ElementAt(i));
                }
            }
            // Othertimes, there may not be Model Errors:
            else
            {
                var error = JsonConvert.DeserializeObject<Dictionary<string, string>>(httpErrorObject);
                foreach (var kvp in error)
                {
                    // Wrap the errors up into the base Exception.Data Dictionary:
                    ex.Data.Add(kvp.Key, kvp.Value);
                }
            }
            return ex;
        }

    }
}