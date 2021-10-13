using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace ComX.Common.ApiClients.MediaTypeFormatters
{
    public class TextPlainMediaTypeFormatter : MediaTypeFormatter
    {
        public TextPlainMediaTypeFormatter()
        {
            base.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            readStream.Position = 0;
            using StreamReader reader = new StreamReader(readStream);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            using var streamWriter = new StreamWriter(writeStream);
            await streamWriter.WriteAsync(value != null ? value.ToString() : string.Empty).ConfigureAwait(false);
        }
    }
}
