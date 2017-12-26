using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NLog.Fluent;

namespace Crawler.Core
{
    public sealed class CrawlerEngine
    {
        public CrawlerEngine(DownloadManager downloadManager, IWebPageLinkManager webPageLinkManager, IPageFileSystemStorage pageFileSystemStorage)
        {
            _downloadManager = downloadManager;
            _webPageLinkManager = webPageLinkManager;
            _pageFileSystemStorage = pageFileSystemStorage;
        }

        public void SavePageToDisk(CrawlerPage page, CrawlerTask crawlerTask)
        {
            Log.Info().Message("Start saving page [{0}] path {1} ReplaceUrlToLocal {2}",
                page.Uri.AbsoluteUri, crawlerTask.LocalPath, crawlerTask.TaskSettings.ReplaceUrlToLocal).Write();

            _pageFileSystemStorage.SavePage(page,crawlerTask.LocalPath,crawlerTask.TaskSettings.ReplaceUrlToLocal);

            Log.Info().Message("Saving page complite. [{0}] path {1}", page.Uri.AbsoluteUri, crawlerTask.LocalPath).Write();
        }
        
        public Task<CrawlerPage> ProcessCrawlerTask(CrawlerTask task)
        {
            Log.Info().Message("Start crawling page [{0}]", task.BaseUri);

            var rootPage = _downloadManager.AddToDownloadQueue(task.BaseUri);
            var rootCrawlerPage = new CrawlerPage { IsRoot = true, Level = 0, Uri = task.BaseUri };
            
            var processTask = rootPage.WaitCompliteTask.ContinueWith(t =>
            {
                var data = t.Result;
                if (!data.HasContent)
                {
                    throw new PageCrawlerException();
                }
                
                if (ParseDownloadedPage(data, rootCrawlerPage, task))
                {
                    var level = new List<CrawlerPage> { rootCrawlerPage };
                    while (true)
                    {
                       var nextLevel = new List<CrawlerPage>();
                        foreach (var page in level)
                        {
                            var levelTask = ProcessNextPageLevel(page, task);
                            levelTask.Wait();

                            nextLevel.AddRange(levelTask.Result);
                            page.ChildPages = levelTask.Result;
                        }

                        if (nextLevel.Count == 0)
                        {
                            return;
                        }

                        level = nextLevel;
                    }
                }
            });

            return Task.Factory.StartNew(() =>
            {
                processTask.Wait();
                Log.Info().Message("Crawling page complete [{0}]", task.BaseUri);
                return rootCrawlerPage;
            });
        }


        private Task<List<CrawlerPage>> ProcessNextPageLevel(CrawlerPage parentPage, CrawlerTask crawlerTask)
        {
            var linksProcessed = 0;
            var tcs = new TaskCompletionSource<List<CrawlerPage>>();
            var result = new ConcurrentQueue<CrawlerPage>();
            if (parentPage.Level >= crawlerTask.TaskSettings.CrawlDepth || !parentPage.Links.Any())
            {
                tcs.SetResult(result.ToList());
            }
            else
            {
                foreach (var lnkNodes in parentPage.Links)
                {
                    var link = lnkNodes.Key;
                    var isNeedToProcess = !crawlerTask.TaskSettings.IgnoreOtherDomains
                                          || IsLinkBelongToDomain(parentPage.Uri, link);
                    if (isNeedToProcess)
                    {
                        var dItm = _downloadManager.AddToDownloadQueue(link);
                        dItm.WaitCompliteTask.ContinueWith(t =>
                        {
                            var data = t.Result;
                            if (data.HasContent)
                            {
                                var page = new CrawlerPage { Level = parentPage.Level + 1, Uri = link };
                                result.Enqueue(page);
                                ParseDownloadedPage(data, page, crawlerTask);
                            }
                            Interlocked.Add(ref linksProcessed, 1);
                            if (linksProcessed == parentPage.Links.Count)
                            {
                                tcs.SetResult(result.ToList());
                            }
                        });
                    }
                    else
                    {
                        Interlocked.Add(ref linksProcessed, 1);
                        if (linksProcessed == parentPage.Links.Count)
                        {
                            tcs.SetResult(result.ToList());
                        }
                    }
                }
            }

            return tcs.Task;
        }

        private bool IsLinkBelongToDomain(Uri baseLink, Uri link)
        {
            return string.Equals(baseLink.Host, link.Host, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool ParseDownloadedPage(DownloaderResult downloaderResult, CrawlerPage page, CrawlerTask crawlerTask)
        {
            page.Data = downloaderResult.Content.Bytes;
            page.IsHtml = downloaderResult.Content.IsHtmlContent;
            page.Html = downloaderResult.Content.HtmlText;
            if (!page.IsHtml)
            {
                return false;
            }
            try
            {
                page.HtmlDoc = new HtmlDocument();
                page.HtmlDoc.LoadHtml(downloaderResult.Content.HtmlText);

                page.Links = _webPageLinkManager.GetAllLinks(crawlerTask.BaseUri, page.HtmlDoc);
            }
            catch (Exception e)
            {
                Log.Warn()
                    .Message("Error while process [{0}]", downloaderResult.Uri.AbsoluteUri)
                    .Exception(e)
                    .Write();

                return false;
            }

            return true;
        }

        private readonly DownloadManager _downloadManager;
        private readonly IWebPageLinkManager _webPageLinkManager;
        private readonly IPageFileSystemStorage _pageFileSystemStorage;
    }
}