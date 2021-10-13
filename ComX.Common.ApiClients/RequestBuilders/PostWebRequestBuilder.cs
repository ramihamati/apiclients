using ComX.Common.ApiClients.Builders;
using System.Net.Http;

namespace ComX.Common.ApiClients.RequestBuilders
{
    public class PostWebRequestBuilder : ApiWebRequestBuilder<PostWebRequestBuilder>
    {
        public PostWebRequestBuilder(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters) 
            : base(httpClient, defaultFormatters)
        {
            _message = new HttpRequestMessage(HttpMethod.Post, "");
            _message.Headers.Clear();
        }
    }
}
