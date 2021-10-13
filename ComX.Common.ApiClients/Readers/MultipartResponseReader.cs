using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace ComX.Common.ApiClients.Readers
{
    public class MultipartResponseReader
    {
        private readonly MultipartMemoryStreamProvider _streamProvider;
        private readonly Dictionary<string, MediaTypeFormatter> _extraFormatters;
        private readonly ReaderDefaultFormatters _defaultFormatters;

        public MultipartResponseReader(
            MultipartMemoryStreamProvider streamProvider,
            Dictionary<string, MediaTypeFormatter> extraFormatters,
            ReaderDefaultFormatters defaultFormatters)
        {
            _streamProvider = streamProvider;
            _extraFormatters = extraFormatters;
            _defaultFormatters = defaultFormatters;
        }

        public HttpContentReader Content(int index)
        {
            HttpContent content = _streamProvider.Contents[index];

            HttpContentReader reader = new HttpContentReader(content, _defaultFormatters);

            reader.AddOrReplaceFormatters(_extraFormatters);

            return reader;
        }
    }
}
