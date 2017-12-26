using System;
using CommandLine;
using Crawler.Core;
using TinyIoC;

namespace Crawler.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                System.Console.WriteLine("Simple use:");
                System.Console.WriteLine(@"Crawler.Console.exe -s http:\\ya.ru -p c:\1");
                System.Console.WriteLine("All property use:");
                System.Console.WriteLine(@"Crawler.Console.exe -s http:\\ya.ru -p c:\1 -r -i -d 3");
                return;
            }
            //TODO input checks
          
            var container = Configure();
            var crawler = container.Resolve<CrawlerEngine>();

            var crawlerTask = new CrawlerTask(new Uri(options.Uri), options.Path,
                new CrawlerTaskSettings
                {
                    CrawlDepth = options.Depth,
                    IgnoreOtherDomains = options.IgnoreOtherDomains,
                    ReplaceUrlToLocal = options.ReplaceToLocal
                });

            crawler.ProcessCrawlerTask(crawlerTask)
                .ContinueWith(t=>crawler.SavePageToDisk(t.Result,crawlerTask))
                .Wait();
        }

        static TinyIoCContainer Configure()
        {
            LoggerConfig.ConfigureLogging(LoggerConfig.GetDefaultLogDir());

            var container = TinyIoCContainer.Current;

            container.Register<CrawlerSettings>().AsSingleton();
            container.Register<IDownloader, WebDownloader>().AsSingleton();
            container.Register<IWebContentExtractor, WebContentExtractor>();
            container.Register<ILinkDataStorage,InMemoryLinkDataStorage>().AsSingleton();
            container.Register<DownloadManager>().AsSingleton();
            container.Register<IWebPageLinkManager, WebPageLinkManager>();
            container.Register<IPageFileSystemStorage, PageFileSystemStorage>();
            container.Register<CrawlerEngine>();
            container.Register<UriToFileNameConverter>();

            return container;
        }
    }
}
