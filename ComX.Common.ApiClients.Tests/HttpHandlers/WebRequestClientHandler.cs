using ComX.Common.ApiClients.Tests.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Common.ApiClients.Tests.HttpHandlers
{
    public sealed class WebRequestClientHandler : HttpMessageHandler
    {
        internal const string BASEADDRESS = "http://test.dig";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.OriginalString == "http://test.dig/users/nocontent?name=john" && request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }

            if (request.RequestUri.OriginalString == "http://test.dig/users/strcontent?name=john" && request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("Hello Home")
                };
            }

            if (request.RequestUri.OriginalString == "http://test.dig/users/jsoncontent?name=john" && request.Method == HttpMethod.Get)
            {
                Person person = new Person { Name = "John", Surname = "Doe" };

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ObjectContent<Person>(person, new JsonMediaTypeFormatter())
                };
            }

            if (request.RequestUri.OriginalString == "http://test.dig/users/jsoncontent" && request.Method == HttpMethod.Post)
            {
                string headerValue = request.Headers.GetValues("X-Person").FirstOrDefault();

                if (headerValue == null) { throw new Exception("Header X-Person not set"); }
                if (headerValue != "John") { throw new Exception("Header X-Person is not set as John"); }

                string contentHeaderValue = request.Content.Headers.GetValues("X-ContentPerson").FirstOrDefault();
                if (contentHeaderValue == null) { throw new Exception("Content Header X-ContentPerson not set"); }
                if (contentHeaderValue != "Doe") { throw new Exception("Content Header X-ContentPerson is not set as Doe"); }

                var person = await request.Content.ReadAsAsync<Person>();

                if (person.Name != "John") { throw new Exception("Request content person.Name is not John"); }
                if (person.Surname != "Doe") { throw new Exception("Request content person.Name is not John"); }

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }

            if (request.RequestUri.OriginalString == "http://test.dig/users/throwerr" && request.Method == HttpMethod.Post)
            {
                throw new Exception("This is an error");
            }

            if (request.RequestUri.OriginalString == "http://test.dig/badrequestlist")
            {
                List<string> errors = new List<string>()
                {
                    "this is an error"
                };

                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    //Content = new ObjectContent(typeof(List<string>), errors, new JsonMediaTypeFormatter())
                    Content = new ObjectContent(errors.GetType(), errors, new JsonMediaTypeFormatter())
                };
            }

            if (request.RequestUri.OriginalString == "http://test.dig/badrequestarr")
            {
                string[] errorsarr = new string[] { "another error" };

                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    //Content = new ObjectContent(typeof(List<string>), errors, new JsonMediaTypeFormatter())
                    Content = new ObjectContent(errorsarr.GetType(), errorsarr, new JsonMediaTypeFormatter())
                };
            }

            if (request.RequestUri.OriginalString == "http://test.dig/modelstatedictionary")
            {
                ModelStateDictionary keyValuePairs = new ModelStateDictionary();
                keyValuePairs.AddModelError("FirstName", "name is too long");
                keyValuePairs.AddModelError("LastName", "name is required");

                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    //Content = new ObjectContent(typeof(List<string>), errors, new JsonMediaTypeFormatter())
                    Content = new ObjectContent(keyValuePairs.GetType(), keyValuePairs, new JsonMediaTypeFormatter())
                };
            }

            throw new NotImplementedException();
        }
    }
}
