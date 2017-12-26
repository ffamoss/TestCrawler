namespace Crawler.Core
{
    public interface ILinkDataStorage
    {
        bool TryGetLinkContent(string link, out WebPageContent content);
        void SetOrUpdateLinkContent(string link, WebPageContent content);
    }
}