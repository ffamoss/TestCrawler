namespace Crawler.Core
{
    public interface IPageFileSystemStorage
    {
        void SavePage(CrawlerPage page, string rootPath, bool replaceLinksToLocal);
    }
}