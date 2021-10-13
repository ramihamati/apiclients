using ComX.Common.ApiClients.RequestBuilders;
using System.Net.Http;

namespace ComX.Common.ApiClients.Builders
{
    public class ApiWebRequestBuilderFactory
    {
        private readonly HttpClient _client;
        private readonly ReaderDefaultFormatters _defaultFormatters;

        public ApiWebRequestBuilderFactory(HttpClient client, ReaderDefaultFormatters defaultFormatters = null)
        {
            _client = client;
            _defaultFormatters = defaultFormatters ?? new ReaderDefaultFormatters();
        }

        public GetWebRequestBuilder GetRequest()
        {
            return new GetWebRequestBuilder(_client, _defaultFormatters);
        }

        public PostWebRequestBuilder PostRequest()
        {
            return new PostWebRequestBuilder(_client, _defaultFormatters);
        }

        public PutWebRequestBuilder PutRequest()
        {
            return new PutWebRequestBuilder(_client, _defaultFormatters);
        }

        public PatchWebRequestBuilder PatchRequest()
        {
            return new PatchWebRequestBuilder(_client, _defaultFormatters);
        }

        public DeleteWebRequestBuilder DeleteRequest()
        {
            return new DeleteWebRequestBuilder(_client, _defaultFormatters);
        }
    }
}
