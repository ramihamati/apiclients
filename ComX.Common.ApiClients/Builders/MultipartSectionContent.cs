using System.Net.Http;

namespace ComX.Common.ApiClients.Builders
{
    // Builder definition of a section to be added in 
    // the multi part form data content
    public sealed class MultipartSectionContent
    {
        internal HttpContent Content { get; set; }
        internal string Name { get; set; } = null; // leave null. do not initialize with empty string
        internal string FileName { get; set; } = null; // leave null. do not initialize with empty string

        private MultipartSectionContent()
        {
        }

        /// <summary>
        /// Section with HTTP content to be added in a collection of System.Net.Http.HttpContent objects that
        /// get serialized to multipart/form-data MIME type.
        /// </summary>
        /// <param name="content"> The HTTP content to add to the collection.</param>
        public static MultipartSectionContent Create(HttpContent content)
            => new MultipartSectionContent
            {
                Content = content
            };

        /// <summary>
        /// Section with HTTP content to be added in a collection of System.Net.Http.HttpContent objects that
        /// get serialized to multipart/form-data MIME type.
        /// </summary>
        /// <param name="content"> The HTTP content to add to the collection.</param>
        /// <param name="name">The name for the HTTP content to add.</param>
        public static MultipartSectionContent Create(HttpContent content, string name)
            => new MultipartSectionContent
            {
                Content = content,
                Name = name
            };

        /// <summary>
        /// Section with HTTP content to be added in a collection of System.Net.Http.HttpContent objects that
        /// get serialized to multipart/form-data MIME type.
        /// </summary>
        /// <param name="content"> The HTTP content to add to the collection.</param>
        /// <param name="name">The name for the HTTP content to add.</param>
        /// <param name="fileName">The file name for the HTTP content to add to the collection.</param>
        public static MultipartSectionContent Create(HttpContent content, string name, string fileName)
            => new MultipartSectionContent
            {
                Content = content,
                Name = name,
                FileName = fileName
            };
    }
}
