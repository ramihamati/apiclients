using ComX.Common.ApiClients.Builders;
using System.Net.Http;

namespace ComX.Common.ApiClients.RequestBuilders
{
    public class PutWebRequestBuilder : ApiWebRequestBuilder<PutWebRequestBuilder>
    {
        public PutWebRequestBuilder(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters) 
            : base(httpClient, defaultFormatters)
        {
            _message = new HttpRequestMessage(HttpMethod.Put, "");
            _message.Headers.Clear();
        }
    }
}
