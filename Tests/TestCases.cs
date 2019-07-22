using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestApiTestAutomation.Models;
using RestApiTestAutomation.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestApiTestAutomation
{
    [TestClass]
    public class TestCases: TestBase
    {
        private const string BaseAddressUri = "http://localhost:5000/";
        private const string AcceptHeader = "application/json";

        #region Get Method
        [TestCategory("Get Method")]
        [TestMethod]
        public void ResponseContainsCollectionsAfterRequestListOfAllCollections()
        {
            string uriRequest = $"api";
            var expectedCollections = new List<string> { "users", "movies", "families" };

            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var httpResponseMessage = HttpTool.MakeRequestToServer(client, HttpMethod.Get, uriRequest);

            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessage);

            var collections = JsonConvert.DeserializeObject<List<string>>(readTask.Result);

            Assert.IsTrue(collections.DoesIncludeList(expectedCollections), $"Not all expected collections were found in the Response!\nExpected: {string.Join(", ", expectedCollections.ToArray())}");
            collections.PrintAllList();
        }


        [TestCategory("Get Method")]
        [TestMethod]
        public void ResponseContainsUserNamesAfterRequestOfUsers()
        {
            string collection = "users";
            string uriRequest = $"api/{collection}";
            var expectedNames = new List<string> { "Avraham", "Itzhak", "Yaakov" };

            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var httpResponseMessage = HttpTool.MakeRequestToServer(client, HttpMethod.Get, uriRequest);

            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessage);

            var users = JsonConvert.DeserializeObject<List<User>>(readTask.Result);
            var userNames = users.Select(u => u.Name).ToList();

            Assert.IsTrue(userNames.DoesIncludeList(expectedNames), $"Not all expected Names were found in the Response!\nExpected: {string.Join(", ", expectedNames.ToArray())}");
            users.PrintAllList();
        }
        [TestCategory("Get Method")]
        [TestMethod]
        public void ResponseContainsUserDetailsAfterRequestOfUserById()
        {
            string collection = "users";
            int id = 6;
            string uriRequest = $"api/{collection}/{id}";
            var expectedUser = new User() { Id = "6", Name = "Avraham", Age = 100, Location = "LA", Work = new Work() { Name = "Sela", Location = "BB", Rating = 5 } };

            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var httpResponseMessage = HttpTool.MakeRequestToServer(client, HttpMethod.Get, uriRequest);

            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessage);

            var user = JsonConvert.DeserializeObject<User>(readTask.Result);

            Assert.IsTrue(user.Equals(expectedUser), $"Response does not include the expected User by Id = {id}");
            Console.WriteLine(user);
        }

        [TestCategory("Get Method")]
        [TestMethod]
        public void ResponseContainsHeadersAfterHeadersRequest()
        {
            string uriRequest = $"api";
            var expectedContentTypeHeader = "application/json; charset=utf-8";
            DateTime expectedDateHeader;
            var expectedServerHeader = "Kestrel";

            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var httpResponseMessage = HttpTool.MakeRequestToServer(client, HttpMethod.Get, uriRequest);
            expectedDateHeader = DateTime.Now;

            var headers = httpResponseMessage.Headers;
            var headersFromContent = httpResponseMessage.Content.Headers;
            var contentTypeHeader = headersFromContent.GetValues("content-type").FirstOrDefault();
            var dateHeader = headers.GetValues("date").FirstOrDefault();
            var etagHeader = headers.GetValues("etag").FirstOrDefault();
            var serverHeader = headers.GetValues("server").FirstOrDefault();

            var actualDateHeader = DateTime.Parse(dateHeader);
            var timeDiffTotalSeconds = (int)(actualDateHeader - expectedDateHeader).TotalSeconds;

            Assert.AreEqual(contentTypeHeader, expectedContentTypeHeader, "content-type Header should be valid in the Response");
            Assert.IsTrue(timeDiffTotalSeconds < 3, "date Header should be valid in the Response");
            Assert.AreEqual(expectedServerHeader, serverHeader, "server Header should be valid in the Response");
        }
        #endregion Get Method

        #region Post Method
        [TestCategory("Post Method")]
        [TestMethod]
        public void ResponseContainsUserAfterUserIsCreated()
        {
            var randomNumber = new Random().Next(1000, 9999);
            var randomUserName = $"RandomUser{randomNumber}";
            var newUser = new User() { Id = "0", Name = randomUserName, Age = 20, Location = "NY", Work = new Work() { Name = "Sela", Location = "BB", Rating = 5 } };

            string collection = "users";
            string uriRequestPost = $"api/{collection}";

            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            HttpContent httpContent = HttpTool.ConvertObjectToHttpContent(newUser);

            var httpResponseMessagePost = HttpTool.MakeRequestToServer(client, HttpMethod.Post, uriRequestPost, httpContent);

            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessagePost);

            var responseBodyAfterPost = JsonConvert.DeserializeObject<JsonResponse>(readTask.Result);
            var responsedUserId = responseBodyAfterPost.Id;
            AddCleanupAction(() => HttpTool.DeleteUser(client, responsedUserId));

            var uriRequestGet = $"api/{collection}/{responsedUserId}";

            var httpResponseMessageGet = HttpTool.MakeRequestToServer(client, HttpMethod.Get, uriRequestGet);

            readTask = HttpTool.ReadContentFromMessage(httpResponseMessageGet);

            var userAfterGet = JsonConvert.DeserializeObject<User>(readTask.Result);

            Assert.IsTrue(userAfterGet.Equals(newUser), $"The User in the Response is not the expected one!");
        }


        #endregion Post Method

        #region Put Method
        [TestCategory("Put Method")]
        [TestMethod]
        public void UserWasUpdatedAfterPutRequest()
        {
            string collection = "users";
            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var newUserId = HttpTool.CreateAndPostRandomUser(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => HttpTool.DeleteUser(client, newUserId));
            var newUserFromServer = HttpTool.GetUserById(client, newUserId);
            var newUserName = $"Updated {newUserFromServer.Name}";
            newUserFromServer.Name = newUserName;

            string uriRequestPut = $"api/{collection}/{newUserFromServer.Id}";

            HttpContent httpContent = HttpTool.ConvertObjectToHttpContent(newUserFromServer);
            var httpResponseMessagePut = HttpTool.MakeRequestToServer(client, HttpMethod.Put, uriRequestPut, httpContent);
            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessagePut);

            var updatedUserFromServer = HttpTool.GetUserById(client, newUserId);

            Assert.IsTrue(updatedUserFromServer.Equals(newUserFromServer), $"The User in the Response is not the expected one!");
        }

        #endregion Put Method

        #region Patch Method
        [TestCategory("Patch Method")]
        [TestMethod]
        public void UserWasUpdatedAfterPatchRequest()
        {
            string collection = "users";
            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var newUserId = HttpTool.CreateAndPostRandomUser(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => HttpTool.DeleteUser(client, newUserId));
            var newUserFromServer = HttpTool.GetUserById(client, newUserId);
            var newUserName = $"Updated {newUserFromServer.Name}";
            newUserFromServer.Name = newUserName;
            var jsonUpdateString = $"{{'Name': '{newUserName}'}}";
            string uriRequestPatch = $"api/{collection}/{newUserId}";

            HttpContent httpContent = HttpTool.ConvertObjectToHttpContent(jsonUpdateString);
            var httpResponseMessagePatch = HttpTool.MakeRequestToServer(client, HttpMethod.Patch, uriRequestPatch, httpContent);
            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessagePatch);

            var updatedUserFromServer = HttpTool.GetUserById(client, newUserId);

            Assert.IsTrue(updatedUserFromServer.Equals(newUserFromServer), $"The User in the Response is not the expected one!");
        }

        #endregion Patch Method

        #region Delete Method
        [TestCategory("Delete Method")]
        [TestMethod]
        public void UserWasDeletedAfterDeleteRequest()
        {
            string collection = "users";
            var client = HttpTool.CreateClient(BaseAddressUri, AcceptHeader);
            AddCleanupAction(() => client.Dispose());
            var newUserId = HttpTool.CreateAndPostRandomUser(BaseAddressUri, AcceptHeader);
            var newUserFromServer = HttpTool.GetUserById(client, newUserId);
            var newUserName = $"Updated {newUserFromServer.Name}";
            newUserFromServer.Name = newUserName;
            string uriRequestDelete = $"api/{collection}/{newUserId}";

            var httpResponseMessageDelete = HttpTool.MakeRequestToServer(client, HttpMethod.Delete, uriRequestDelete);
            Task<string> readTask = HttpTool.ReadContentFromMessage(httpResponseMessageDelete);

            var httpResponseMessage = HttpTool.EnsureObjectIsNotFound(client, uriRequestDelete);

            Assert.IsTrue(httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound, $"The User was not deleted as expected!");
        }

        #endregion Delete Method

    }
}
