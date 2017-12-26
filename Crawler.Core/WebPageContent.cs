using System.Text;

namespace Crawler.Core
{
    public class WebPageContent
    {
        public byte[] Bytes { get; set; }

        public string ContentType { get; set; }

        public Encoding Encoding { get; set; }

        public bool IsHtmlContent { get; set; }

        public string HtmlText => !IsHtmlContent && Bytes == null || Bytes.Length == 0 ? string.Empty : Encoding.GetString(Bytes);
    }
}