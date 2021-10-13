using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ComX.Common.ApiClients.Builders
{
    public sealed class WebRequestHeadersBuilder
    {
        /// <summary>
        /// headers that are added in the request.
        /// </summary>
        private readonly Dictionary<string, string> messageHeaders;
        
        /// <summary>
        /// headaers that are added in the content of the request
        /// </summary>
        private readonly Dictionary<string, string> contentHeaders;

        public WebRequestHeadersBuilder()
        {
            messageHeaders = new Dictionary<string, string>();
            contentHeaders = new Dictionary<string, string>();
        }

        public WebRequestHeadersBuilder AddHeader(string key, string value)
        {
            if (messageHeaders.ContainsKey(key))
            {
                messageHeaders.Remove(key);
            }

            messageHeaders.Add(key, value);
            return this;
        }

        public WebRequestHeadersBuilder AddHeaderAcceptJson()
        {
            if (messageHeaders.ContainsKey("Accept"))
            {
                messageHeaders.Remove("Accept");
            }

            messageHeaders.Add("Accept", "application/json");

            return this;
        }

        public WebRequestHeadersBuilder AddContentHeader(string key, string value)
        {
            if (contentHeaders.ContainsKey(key))
            {
                contentHeaders.Remove(key);
            }
            contentHeaders.Add(key, value);
            return this;
        }

        internal void AddHeadersToMessage(HttpRequestMessage httpRequestMessage)
        {
            foreach(var kvp in messageHeaders)
            {
                if (httpRequestMessage.Headers.Contains(kvp.Key))
                {
                    httpRequestMessage.Headers.Remove(kvp.Key);
                }
                httpRequestMessage.Headers.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in contentHeaders)
            {
                if (httpRequestMessage.Content == null)
                {
                    throw new Exception("THe request message does not have a content. Cannot add content headers");
                }

                if (httpRequestMessage.Content.Headers.Contains(kvp.Key))
                {
                    httpRequestMessage.Content.Headers.Remove(kvp.Key);
                }
                httpRequestMessage.Content.Headers.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
