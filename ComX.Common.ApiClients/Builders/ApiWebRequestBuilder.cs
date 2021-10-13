using ComX.Common.ApiClients.Messages;
using System;
using System.Net.Http;

namespace ComX.Common.ApiClients.Builders
{
    public abstract class ApiWebRequestBuilder<TWebRequest> where TWebRequest : ApiWebRequestBuilder<TWebRequest>
    {
        protected readonly HttpClient _httpClient;
        protected HttpRequestMessage _message;

        private readonly WebRequestHeadersBuilder headersBuilder;
        private HttpContent _content;
        private readonly WebRequestUriBuilder _uriBuilder;
        private readonly ReaderDefaultFormatters _defaultFormatters;

        protected ApiWebRequestBuilder(HttpClient httpClient, ReaderDefaultFormatters defaultFormatters)
        {
            _httpClient = httpClient;
            headersBuilder = new WebRequestHeadersBuilder();
            _uriBuilder = new WebRequestUriBuilder();
            _defaultFormatters = defaultFormatters;
        }

        public TWebRequest SetUri(Action<WebRequestUriBuilder> builder)
        {
            builder(_uriBuilder);
            return (TWebRequest)this;
        }

        public TWebRequest SetContent(Func<WebRequestHttpContentBuilder, HttpContent> builder)
        {
            WebRequestHttpContentBuilder httpContentBuilder = new WebRequestHttpContentBuilder();
            _content = builder(httpContentBuilder);
            return (TWebRequest)this;
        }

        public TWebRequest SetContentMultiPart(params Func<WebRequestHttpContentBuilder, HttpContent>[] builders)
        {
            MultipartContent multiPartContent = new MultipartContent();
            WebRequestHttpContentBuilder httpContentBuilder = new WebRequestHttpContentBuilder();

            foreach (var builder in builders)
            {
                HttpContent content = builder(httpContentBuilder);

                if (!(content is StreamContent) && !(content is ByteArrayContent))
                {
                    throw new NotSupportedException("Cannot create multi part content from the provided content types." +
                        "Supported types are StreamContent and ByteArrayContent");
                }

                multiPartContent.Add(content);
            }

            _content = multiPartContent;

            return (TWebRequest)this;
        }

        public TWebRequest AddHeader(Action<WebRequestHeadersBuilder> builder)
        {
            builder(headersBuilder);
            return (TWebRequest)this;
        }

        /// <summary>
        /// Adds an Authorization header with the value "Bearer [token]".
        /// If the passed token does not contain the Bearer keyword, the mehtod will ensure it'sa dded
        /// </summary>
        public TWebRequest AddAuthorizationBearerToken(string token)
        {
            if (!token.StartsWith("Bearer "))
            {
                token = $"Bearer {token}";
            }

            this.AddHeader(builder => builder.AddHeader("Authorization", token));
            return (TWebRequest)this;
        }

        public WebRequestMessage BuildRequest()
        {
            _message.RequestUri = _uriBuilder.BuildUri();
            //first add the content
            if (_content != null)
            {
                _message.Content = _content;
            }
            //then add headers which might include content headers
            headersBuilder.AddHeadersToMessage(_message);

            return new WebRequestMessage(_httpClient, _message, _defaultFormatters);
        }
    }
}
