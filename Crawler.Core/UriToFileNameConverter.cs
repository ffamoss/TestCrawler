using System.IO;

namespace Crawler.Core
{
    public sealed class UriToFileNameConverter
    {
        public string ConvertToFileName(CrawlerPage page)
        {
            var fileName = page.IsRoot 
                ? page.Uri.Host 
                : Path.GetFileName(page.Uri.LocalPath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = page.Uri.AbsoluteUri.GetHashCode().ToString();
            }
            
            if (page.IsHtml)
            {
                fileName = fileName + ".html";
            }
            return fileName;
        }

    }
}