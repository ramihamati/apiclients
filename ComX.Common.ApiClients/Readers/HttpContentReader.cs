using ComX.Common.ApiClients.MediaTypeFormatters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace ComX.Common.ApiClients.Readers
{

    public sealed class HttpContentReader
    {
        internal Dictionary<string, MediaTypeFormatter> _formatters = new Dictionary<string, MediaTypeFormatter>();

        private readonly HttpContent _content;

        public HttpContentReader(HttpContent content, ReaderDefaultFormatters defaultFormatters)
        {
            _content = content;

            _formatters.Add(ReaderDefaultFormatters.ContentTypeFormUrlEncoded, defaultFormatters.FormUrlEncoded);
            _formatters.Add(ReaderDefaultFormatters.ContentTypeApplicationJson, defaultFormatters.ApplicationJson);
            _formatters.Add(ReaderDefaultFormatters.ContentTypeApplicationXml, defaultFormatters.ApplicationXml);
            _formatters.Add(ReaderDefaultFormatters.ContentTypeApplicationBson, defaultFormatters.ApplicationBson);
            _formatters.Add(ReaderDefaultFormatters.ContentTypeTextPlain, defaultFormatters.TextPlain);
        }

        /// <summary>
        /// Deserializes the response using one of the registered formatters.
        /// A formatter is selected based on the content type. If the content type is missing the formatter will throw an error.
        /// </summary>
        public async Task<TModel> AsModelAsync<TModel>() where TModel : class
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }

            if (_content.Headers.ContentType == null
                || string.IsNullOrEmpty(_content.Headers.ContentType.MediaType))
            {
                throw new Exception("The response message does not have a content-type header");
            }

            if (_formatters.ContainsKey(_content.Headers.ContentType.MediaType))
            {
                MediaTypeFormatter formatter = _formatters[_content.Headers.ContentType.MediaType];

                return await _content.ReadAsAsync<TModel>(new List<MediaTypeFormatter> { formatter });
            }

            throw new Exception($"Could not find a valid MediaTypeFormatter for the content type {_content.Headers.ContentType.MediaType}");
        }

        /// <summary>
        /// Deserializes the response using the JsonMediaTypeFormatter. Not all responses contain a content type appropiate for deserialization.
        /// </summary>
        public async Task<TModel> AsJsonModelAsync<TModel>() where TModel : class
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }
            MediaTypeFormatter formatter = _formatters["application/json"];
            return await _content.ReadAsAsync<TModel>(new List<MediaTypeFormatter> { formatter });
        }

        /// <summary>
        /// Reads the content as a string. If the status code is not succesfull it throws 
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<string> AsStringAsync()
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }
            return await _content.ReadAsStringAsync();
        }

        /// <summary>
        /// Reads the content as a stream. If the status code is not succesfull it throws 
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<Stream> AsStreamAsync()
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }
            return await _content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Reads the content as a bytearray. If the status code is not succesfull it throws 
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<byte[]> AsByteArrayAsync()
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }
            return await _content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Reads the content as form data. If the status code is not succesfull it throws 
        /// an exception with the content as a string or the reason phrase.
        /// </summary>
        public async Task<NameValueCollection> AsFormDataAsync()
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }
            return await _content.ReadAsFormDataAsync();
        }

        public async Task<MultipartMemoryStreamProvider> AsMultiPartAsync()
        {
            //do not move in constructor. We may want to catch this exception when the reader tries to read the content
            if (_content == null)
            {
                throw new Exception($"The response message does not have a content");
            }
            return await _content.ReadAsMultipartAsync();
        }

        internal void AddOrReplaceFormatters(Dictionary<string, MediaTypeFormatter> formatters)
        {
            foreach(var kvp in formatters)
            {
                
                if (_formatters.ContainsKey(kvp.Key))
                {
                    _formatters.Remove(kvp.Key);
                }

                _formatters.Add(kvp.Key, kvp.Value);
            }
        }

        public void AddOrReplaceFormatter(string contentType, MediaTypeFormatter formatter)
        {
            if (_formatters.ContainsKey(contentType))
            {
                _formatters.Remove(contentType);
            }

            _formatters.Add(contentType, formatter);
        }
    }
}
