using System.Collections.Concurrent;

namespace Crawler.Core
{
    public sealed class InMemoryLinkDataStorage : ILinkDataStorage
    {
        public bool TryGetLinkContent(string link, out WebPageContent content)
        {
            return _storage.TryGetValue(link, out content);
        }

        public void SetOrUpdateLinkContent(string link, WebPageContent content)
        {
            _storage.AddOrUpdate(link, content, (_, __) => content);
        }

        private readonly ConcurrentDictionary<string,WebPageContent> _storage = new ConcurrentDictionary<string, WebPageContent>();
    }
}