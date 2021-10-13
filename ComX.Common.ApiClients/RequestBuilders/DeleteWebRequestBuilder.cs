using ComX.Common.ApiClients.Builders;
using System.Net.Http;

namespace ComX.Common.ApiClients.RequestBuilders
{
    public class DeleteWebRequestBuilder : ApiWebRequestBuilder<DeleteWebRequestBuilder>
    {
        public DeleteWebRequestBuilder(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters) 
            : base(httpClient, defaultFormatters)
        {
            _message = new HttpRequestMessage(HttpMethod.Delete, "");
            _message.Headers.Clear();
        }
    }
}
