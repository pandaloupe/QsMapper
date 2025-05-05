using System;
using System.IO;

namespace Samples.Model.Messaging
{
    [Serializable]
    public class Attachment
    {
        public int MessageId { get; set; }
        public string Filename { get; set; }
        public string MimeType { get; set; }
        
        /**
         * Base64 encoded file contents
         */
        public string Data { get; set; }

        public System.Net.Mail.Attachment ToMailAttachment()
        {
            return new System.Net.Mail.Attachment(new MemoryStream(Convert.FromBase64String(Data)), Filename, MimeType);
        }
    }
}