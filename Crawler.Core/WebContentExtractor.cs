using System;
using System.IO;
using System.Net;
using System.Text;
using NLog.Fluent;

namespace Crawler.Core
{
    public sealed class WebContentExtractor : IWebContentExtractor
    {
        public WebPageContent GetContent(WebResponse response)
        {
            var webPageContent = new WebPageContent();
            using (var memoryStream = GetRawData(response))
            {
                webPageContent.Bytes = memoryStream.ToArray();
                webPageContent.ContentType = response.ContentType;
                webPageContent.IsHtmlContent = response.ContentType?.Contains("text/html") == true;
                //Note ignore case when content body has different charset
                webPageContent.Encoding = GetEncodingFromHeadersOrDefault(response);

                return webPageContent;
            }
        }

        private Encoding GetEncodingFromHeadersOrDefault(WebResponse webResponse)
        {
            string charset = null;
            var ctype = webResponse.Headers["content-type"];
            if (ctype != null)
            {
                var ind = ctype.IndexOf("charset=");
                if (ind >= 0)
                {
                    charset = CleanCharset(ctype.Substring(ind + 8));
                }
            }
            return GetEncoding(charset);
        }

        private Encoding GetEncoding(string charset)
        {
            var encoding = Encoding.UTF8;
            if (charset == null)
            {
                return encoding;
            }

            try
            {
                encoding = Encoding.GetEncoding(charset);
            }
            catch (Exception e)
            {
                Log.Debug()
                    .Message("Parsing charset {0}. Use default.({1})", charset, encoding)
                    .Exception(e)
                    .Write();
            }

            return encoding;
        }

        private string CleanCharset(string charset)
        {
            if (charset == "cp1251")
                charset = "windows-1251";

            return charset;
        }

        // Note. Ignore memory usage for huge content size
        private MemoryStream GetRawData(WebResponse webResponse)
        {
            MemoryStream rawData = new MemoryStream();
            try
            {
                using (var r = webResponse.GetResponseStream())
                {
                    int read = 0;
                    var buffer = new byte[1024];
                    do
                    {
                        read = r.Read(buffer, 0, buffer.Length);
                        rawData.Write(buffer, 0, read);
                    } while (read > 0);
                }
            }
            catch (Exception e)
            {
                Log.Warn()
                    .Message("Error occurred while downloading content of url {0}", webResponse.ResponseUri.AbsoluteUri)
                    .Exception(e)
                    .Write();
            }

            rawData.Seek(0, SeekOrigin.Begin);
            return rawData;
        }
    }
}

