using System.Net;

namespace Crawler.Core
{
    public interface IWebContentExtractor
    {
        WebPageContent GetContent(WebResponse response);
    }
}