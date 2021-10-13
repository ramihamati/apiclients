using ComX.Common.ApiClients.Builders;
using System.Net.Http;

namespace ComX.Common.ApiClients.RequestBuilders
{
    public class GetWebRequestBuilder : ApiWebRequestBuilder<GetWebRequestBuilder>
    {
        public GetWebRequestBuilder(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters) 
            : base(httpClient, defaultFormatters)
        {
            _message = new HttpRequestMessage(HttpMethod.Get, "");
            _message.Headers.Clear();
        }
    }
}
