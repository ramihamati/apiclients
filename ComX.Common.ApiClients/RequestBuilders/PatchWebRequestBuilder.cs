using ComX.Common.ApiClients.Builders;
using System.Net.Http;

namespace ComX.Common.ApiClients.RequestBuilders
{
    public class PatchWebRequestBuilder : ApiWebRequestBuilder<PatchWebRequestBuilder>
    {
        public PatchWebRequestBuilder(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters)
            : base(httpClient, defaultFormatters)
        {
            _message = new HttpRequestMessage(HttpMethod.Patch, "");
            _message.Headers.Clear();
        }
    }
}
