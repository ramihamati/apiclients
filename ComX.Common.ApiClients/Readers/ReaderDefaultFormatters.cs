using ComX.Common.ApiClients.MediaTypeFormatters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace ComX.Common.ApiClients
{
    public class ReaderDefaultFormatters
    {
        public static string ContentTypeFormUrlEncoded = "application/x-www-form-urlencoded";
        public static string ContentTypeApplicationJson = "application/json";
        public static string ContentTypeApplicationXml = "application/xml";
        public static string ContentTypeApplicationBson = "application/bson";
        public static string ContentTypeTextPlain = "text/plain";

        public MediaTypeFormatter FormUrlEncoded { get; set; } = new FormUrlEncodedMediaTypeFormatter();
        public MediaTypeFormatter ApplicationJson { get; set; } = new JsonMediaTypeFormatter();
        public MediaTypeFormatter ApplicationXml { get; set; } = new XmlMediaTypeFormatter();
        public MediaTypeFormatter ApplicationBson { get; set; } = new BsonMediaTypeFormatter();
        public MediaTypeFormatter TextPlain { get; set; } = new TextPlainMediaTypeFormatter();
    }
}
