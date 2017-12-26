using System;

namespace Crawler.Core
{
    public class CrawlerTask
    {
        public CrawlerTask(Uri baseUri, string localPath, CrawlerTaskSettings taskSettings)
        {
            BaseUri = baseUri;
            LocalPath = localPath;
            TaskSettings = taskSettings;
        }

        public Uri BaseUri { get; }
        public string LocalPath { get; }
        public CrawlerTaskSettings TaskSettings { get; }
    }
}