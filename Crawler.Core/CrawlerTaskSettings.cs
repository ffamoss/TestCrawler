namespace Crawler.Core
{
    public class CrawlerTaskSettings
    {
        public int CrawlDepth { get; set; }
        public bool IgnoreOtherDomains { get; set; }
        public bool ReplaceUrlToLocal { get; set; }
    }
}