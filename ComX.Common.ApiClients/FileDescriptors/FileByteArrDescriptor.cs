namespace ComX.Common.ApiClients.FileDescriptors
{
    public class FileByteArrDescriptor
    {
        public string FileName { get; }

        public byte[] FileContent { get; }

        public string MimeType { get; }

        public FileByteArrDescriptor(string fileName, byte[] fileContent, string mimeType)
        {
            this.FileContent = fileContent;
            this.FileName = fileName;
            this.MimeType = mimeType;
        }
    }
}
