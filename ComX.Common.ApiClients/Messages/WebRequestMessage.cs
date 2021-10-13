using ComX.Common.ApiClients.ExceptionHandler;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComX.Common.ApiClients.Messages
{
    public class WebRequestMessage
    {
        private readonly HttpClient _httpClient;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly ExceptionDispatcher _exceptionDispatcher;
        private readonly ReaderDefaultFormatters _defaultFormatters;

        internal WebRequestMessage(HttpClient httpClient, HttpRequestMessage httpRequestMessage, ReaderDefaultFormatters defaultFormatters)
        {
            _httpClient = httpClient;
            _httpRequestMessage = httpRequestMessage;
            _exceptionDispatcher = new ExceptionDispatcher();
            _defaultFormatters = defaultFormatters;
        }

        public async Task<WebResponseMessage> SendRequestAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(_httpRequestMessage).ConfigureAwait(false);
                return new WebResponseMessage(response, _defaultFormatters, _exceptionDispatcher);
            }
            catch (System.Exception ex)
            {
                _exceptionDispatcher.Capture(ex);
                return new WebResponseMessage(null, _defaultFormatters, _exceptionDispatcher);
            }
        }
    }
}
