using ComX.Common.ApiClients.Builders;
using ComX.Common.ApiClients.Messages;
using ComX.Common.ApiClients.RequestBuilders;
using ComX.Common.ApiClients.Tests.HttpHandlers;
using ComX.Common.ApiClients.Tests.Models;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable RCS1090 // Call 'ConfigureAwait(false)'.

namespace ComX.Common.ApiClients.Tests
{
    public sealed class ApiWebRequestTests
    {
        private ApiWebRequestBuilderFactory factory;

        [SetUp]
        public void SetUp()
        {
            HttpClient httpClient = new HttpClient(new WebRequestClientHandler());
            httpClient.BaseAddress = new System.Uri(WebRequestClientHandler.BASEADDRESS);
            factory = new ApiWebRequestBuilderFactory(httpClient);
        }

        [Test]
        public async Task GetRequest_AsApiResult_ShouldWork()
        {
            GetWebRequestBuilder getRequest = factory.GetRequest();
            
            getRequest.SetUri(builder =>
            {
                builder.SetPath("/users/nocontent");
                builder.AddQueryParameter("name", "john");
            });

            WebRequestMessage requestMessage = getRequest.BuildRequest();

            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult result = await response.AsApiResultAsync().ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, result.HttpStatus);
        }

        [Test]
        public async Task GetRequest_AsString_NoContent_WillThrowException()
        {
            GetWebRequestBuilder getRequest = factory.GetRequest();
            getRequest.SetUri(builder =>
            {
                builder.SetPath("/users/nocontent");
                builder.AddQueryParameter("name", "john");
            });

            WebRequestMessage requestMessage = getRequest.BuildRequest();

            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            Assert.CatchAsync(async () => await response.AsStringAsync());
        }

        [Test]
        public async Task GetRequest_AsString_StringContent()
        {
            GetWebRequestBuilder getRequest = factory.GetRequest();
            getRequest.SetUri(builder =>
            {
                builder.SetPath("/users/strcontent");
                builder.AddQueryParameter("name", "john");
            });

            WebRequestMessage requestMessage = getRequest.BuildRequest();

            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            //as string will work
            string content = await response.AsStringAsync();
            Assert.AreEqual("Hello Home", content);
            //as stream will have to read the stream
            Stream streamcontent = await response.AsStreamAsync();
            StreamReader streamReader = new StreamReader(streamcontent);
            string strContent = streamReader.ReadToEnd();
            Assert.AreEqual("Hello Home", strContent);
            //as byte
            byte[] byteContent = await response.AsByteArrayAsync();
            string strByteContent = Encoding.UTF8.GetString(byteContent);
            Assert.AreEqual("Hello Home", strByteContent);
            //as form data will throw an error

            Assert.CatchAsync(async () => await response.AsFormDataAsync());

                               //as api result
            var apiResultContent = await response.AsApiResultAsync<string>();
            Assert.AreEqual(HttpStatusCode.OK, apiResultContent.HttpStatus);
            Assert.AreEqual("Hello Home", apiResultContent.Model);
        }

        [Test]
        public async Task GetRequest_JsonModelResponse()
        {
            GetWebRequestBuilder getRequest = factory.GetRequest();
            getRequest.SetUri(builder =>
            {
                builder.SetPath("/users/jsoncontent");
                builder.AddQueryParameter("name", "john");
            });

            WebRequestMessage requestMessage = getRequest.BuildRequest();
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            var personModel = await response.AsModelAsync<Person>();
            Assert.AreEqual(personModel.Name, "John");
            Assert.AreEqual(personModel.Surname, "Doe");

            var personModel2 = await response.AsJsonModelAsync<Person>();
            Assert.AreEqual(personModel2.Name, "John");
            Assert.AreEqual(personModel2.Surname, "Doe");
        }

        [Test]
        public async Task PostRequest_JsonContent()
        {
            PostWebRequestBuilder postRequest = factory.PostRequest();

            postRequest.SetUri(builder =>
            {
                builder.SetPath("/users/jsoncontent");
            });

            postRequest.AddHeader(builder =>
            {
                builder.AddHeader("X-Person", "John");
                builder.AddContentHeader("X-ContentPerson", "Doe");
            });
            Person person = new Person { Name = "John", Surname = "Doe" };
            postRequest.SetContent(builder => builder.ContentJson<Person>(person));

            WebRequestMessage requestMessage = postRequest.BuildRequest();
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult result = await response.AsApiResultAsync();
            Assert.AreEqual(HttpStatusCode.OK, result.HttpStatus);
        }

        [Test]
        public async Task Request_ErrorHandling()
        {
            PostWebRequestBuilder postRequest = factory.PostRequest();

            postRequest.SetUri(builder =>
            {
                builder.SetPath("/users/throwerr");
            });

            WebRequestMessage requestMessage = postRequest.BuildRequest();

            //the webresponsemessage caught the exception in the exception dispatcher because the ApiResult will actually return the Exception
            //as a message with the status code InternalServerError and the flag IsFaulted = true
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult apiResult = await response.AsApiResultAsync();
            Assert.AreEqual(true, apiResult.IsFaulted);
            Assert.AreEqual(HttpStatusCode.InternalServerError, apiResult.HttpStatus);
            Assert.CatchAsync(async () => await response.AsStringAsync());
        }

        [Test]
        public async Task BadRequest_HandleListOfErrors()
        {
            PostWebRequestBuilder postRequest = factory.PostRequest();

            postRequest.SetUri(builder =>
            {
                builder.SetPath("/badrequestlist");
            });

            WebRequestMessage requestMessage = postRequest.BuildRequest();

            //the webresponsemessage caught the exception in the exception dispatcher because the ApiResult will actually return the Exception
            //as a message with the status code InternalServerError and the flag IsFaulted = true
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult apiResult = await response.AsApiResultAsync();
            Assert.AreEqual(1, apiResult.ErrorMessage.Length);
            Assert.AreEqual("this is an error", apiResult.ErrorMessage[0]);
        }

        [Test]
        public async Task BadRequest_HandleArrOfErrors()
        {
            PostWebRequestBuilder postRequest = factory.PostRequest();

            postRequest.SetUri(builder =>
            {
                builder.SetPath("/badrequestarr");
            });

            WebRequestMessage requestMessage = postRequest.BuildRequest();

            //the webresponsemessage caught the exception in the exception dispatcher because the ApiResult will actually return the Exception
            //as a message with the status code InternalServerError and the flag IsFaulted = true
            WebResponseMessage response = await requestMessage.SendRequestAsync().ConfigureAwait(false);

            ApiResult apiResult = await response.AsApiResultAsync();
            Assert.AreEqual(1, apiResult.ErrorMessage.Length);
            Assert.AreEqual("another error", apiResult.ErrorMessage[0]);
        }

    }
}
#pragma warning restore RCS1090 // Call 'ConfigureAwait(false)'.