

using System;
using System.Text;
using Crawler.Core;
using NUnit.Framework;

namespace Crawler.Tests
{
    [TestFixture]
    public class SmokeTests
    {
        [Test]
        public void SmokeTest()
        {
            var rootPath = @"C:\1\";
            var uri = new Uri(@"http:\\ya.ru");
            var dwnMock = new Moq.Mock<IDownloader>();
            dwnMock.Setup(i => i.Download(uri))
                .Returns(() =>
            {
                var result = new DownloaderResult(uri);
                result.SetResponseData(new WebPageContent()
                {
                    Encoding = Encoding.UTF8,
                    IsHtmlContent = true,
                    Bytes = Encoding.UTF8.GetBytes(_yaSiteHtml)
                });
                return result;
            });

            var pageFileStorageMock = new Moq.Mock<IPageFileSystemStorage>();
            pageFileStorageMock.Setup(i => i.SavePage(null, rootPath, true));
                
            var downLoader = dwnMock.Object;
            var settings = new CrawlerSettings();
            var downloadManager = new DownloadManager(downLoader, new InMemoryLinkDataStorage(), settings);
            var webPageLinkManager = new WebPageLinkManager();
            var engine = new CrawlerEngine(downloadManager, webPageLinkManager, pageFileStorageMock.Object);

            var taskSettings = new CrawlerTaskSettings()
            {
                CrawlDepth = 1,
                IgnoreOtherDomains = false,
                ReplaceUrlToLocal = true
            };

            var task = new CrawlerTask(uri,rootPath, taskSettings);

            var page = engine.ProcessCrawlerTask(task).Result;
            
            Assert.AreEqual(page.Uri,uri);
            Assert.IsTrue(page.IsHtml);
            Assert.AreEqual(page.Html,_yaSiteHtml);
        }

        private string _yaSiteHtml =
                @"<!DOCTYPE HTML PUBLIC  ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd""><html><head><title> My Title</title></head><body>ya.ru body</body></html>";

    }
}
