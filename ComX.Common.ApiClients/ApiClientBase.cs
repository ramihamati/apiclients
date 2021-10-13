using ComX.Common.ApiClients.Builders;
using System.Net.Http;

namespace ComX.Common.ApiClients
{
    public class ApiClientBase
    {
        public ApiWebRequestBuilderFactory RequestFactory { get; }

        public ApiClientBase(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters = null)
        {
            RequestFactory = new ApiWebRequestBuilderFactory(httpClient, defaultFormatters ?? new ReaderDefaultFormatters());
        }
    }
}
