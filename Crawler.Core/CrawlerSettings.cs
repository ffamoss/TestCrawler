using System;
using System.Collections.Generic;

namespace Crawler.Core
{
    public class CrawlerSettings
    {
        public CrawlerSettings()
        {
            MaxParallelDownloadsCount = 10;
            UserAgent = @"Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 57.0.2987.133 Safari / 537.36";
            ConnectionLimit = 5;
            RequestTimeout = TimeSpan.FromMinutes(1);
            RetryTimeout = TimeSpan.FromMinutes(3);
            RetryCount = 2;
            IsCookieEnabled = true;
            SupportedContentType = new List<string> { "*/*" };
        }
        
        public int MaxParallelDownloadsCount { get; set; }
        public int ConnectionLimit { get; set; }
        public List<string> SupportedContentType { get; set; }
        public TimeSpan RequestTimeout { get; set; }
        public TimeSpan RetryTimeout { get; set; }
        public int RetryCount { get; set; }
        public string UserAgent { get; set; }
        public bool IsCookieEnabled { get; set; }
        public string SupportedContentTypeString => string.Join(";", SupportedContentType);
    }
}
