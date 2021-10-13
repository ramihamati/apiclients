using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ComX.Common.ApiClients.Builders
{
    public sealed class WebRequestHttpContentBuilder
    {
        private FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public WebRequestHttpContentBuilder()
        {
            _fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        }

        public HttpContent ContentMultiPart(
            string multiPartFormDataBoundary,
            params Func<WebRequestHttpContentBuilder, MultipartSectionContent>[] segments)
        {
            MultipartFormDataContent content = new MultipartFormDataContent(multiPartFormDataBoundary);
            foreach (Func<WebRequestHttpContentBuilder, MultipartSectionContent> segment in segments)
            {
                MultipartSectionContent section = segment(this);
                content.Add(section.Content, section.Name, section.FileName);
            }

            return content;
        }

        public HttpContent ContentMultiPart(
            params Func<WebRequestHttpContentBuilder, MultipartSectionContent>[] segments)
        {
            MultipartFormDataContent content = new MultipartFormDataContent(
                "Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

            foreach (Func<WebRequestHttpContentBuilder, MultipartSectionContent> segment in segments)
            {
                MultipartSectionContent section = segment(this);

                if (section.Name is not null && section.FileName is not null)
                {
                    content.Add(section.Content, section.Name, section.FileName);
                }
                else if (section.Name is not null)
                {
                    content.Add(section.Content, section.Name);
                }
                else
                {
                    content.Add(section.Content);
                }
            }

            return content;
        }

        public HttpContent ContentJson<TModel>(TModel model)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(model));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        public HttpContent ContentJson(string serialized)
        {
            StringContent content = new StringContent(serialized);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        public HttpContent ContentText(string text)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(text));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            return content;
        }

        public HttpContent ContentFormUrlEncoded(params KeyValuePair<string, string>[] entries)
        {
            return new FormUrlEncodedContent(entries);
        }

        public HttpContent ContentStream(Stream stream, string contentType = null)
        {
            var content = new StreamContent(stream);
            if (contentType != null)

            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }

            content.Headers.Add("Content-Length", $"{stream.Length}");
            return content;
        }

        public HttpContent ContentByteArray(byte[] array, string contentType = null)
        {
            var content = new ByteArrayContent(array);
            if (contentType != null)
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                content.Headers.Add("Content-Length", $"{array.Length}");
            }

            return content;
        }

        public HttpContent ContentFile(string filePath, string dispositionFileName = null, string mimeType = null)
        {
            if (dispositionFileName == null)
            {
                dispositionFileName = filePath.Split('/', '\\').Last();
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist");
            }

            ByteArrayContent fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            if (mimeType == null)
            {
                bool found = _fileExtensionContentTypeProvider.TryGetContentType(filePath, out string contentType);

                if (found)
                {
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }
            else
            {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            }

            if (dispositionFileName == null)
            {
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(filePath)
                };
            }
            else
            {
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = dispositionFileName
                };
            }

            return fileContent;
        }

        public HttpContent ContentFile(byte[] file, string fileName, string mimeType = null)
        {
            ByteArrayContent fileContent = new ByteArrayContent(file);

            if (mimeType == null)
            {
                if (fileName.Contains("."))
                {
                    string extension = fileName.Split(".").Last();

                    if (_fileExtensionContentTypeProvider.Mappings.ContainsKey(extension.ToLower()))
                    {
                        string _contentType = _fileExtensionContentTypeProvider.Mappings[extension.ToLower()];
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(_contentType);
                    }
                }
            }
            else
            {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            }

            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return fileContent;
        }
    }
}
