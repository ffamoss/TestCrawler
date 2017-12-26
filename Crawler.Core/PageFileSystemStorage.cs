using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog.Fluent;

namespace Crawler.Core
{
    public sealed class PageFileSystemStorage : IPageFileSystemStorage
    {
        public PageFileSystemStorage(UriToFileNameConverter uriToFileNameConverter, IWebPageLinkManager linkManager)
        {
            _uriToFileNameConverter = uriToFileNameConverter;
            _linkManager = linkManager;
        }

        //Note ignore access checks, disk size checks, directory not empty checks...
        public void SavePage(CrawlerPage page, string rootPath, bool replaceLinksToLocal)
        {
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            var dataPath = GetDataPath(rootPath);
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }


            var allPages = ToEnumerableAtBreadthFirst(page).ToList();

            if (replaceLinksToLocal)
            {
                ReplaceLinks(allPages);
            }

            foreach (var currentPage in allPages)
            {
                try
                {
                    SavePage(currentPage, rootPath, dataPath);
                }
                catch (Exception e)
                {
                    Log.Error()
                        .Message("Error while saving page on disk [{0}]. Location: [{1}], [{2}]",
                            currentPage.Uri.AbsoluteUri, rootPath, dataPath)
                        .Exception(e)
                        .Write();
                }
            }
        }

        private void ReplaceLinks(List<CrawlerPage> allPages)
        {
            Dictionary<Uri, CrawlerPage> pageDic  = new Dictionary<Uri, CrawlerPage>();
            foreach (var p in allPages)
            {
                pageDic[p.Uri] = p;
            }

            foreach (var page in allPages)
            {
                if (!page.IsHtml || !page.ChildPages.Any())
                {
                    continue;
                }

                foreach (var pageLink in page.Links)
                {
                    if (!pageDic.TryGetValue(pageLink.Key, out var linkContent))
                    {
                        continue ;
                    }
                    var name = _uriToFileNameConverter.ConvertToFileName(linkContent);
                    var newLink = GetRelativePath(linkContent, name);

                    foreach (var htmlNode in pageLink.Value)
                    {
                        _linkManager.ReplaceLink(htmlNode, newLink);
                    }
                }
            }
        }

        private void SavePage(CrawlerPage page, string rootPath, string dataPath)
        {
            string path;
            var fileName = _uriToFileNameConverter.ConvertToFileName(page);
            if (page.IsHtml)
            {
                path = Path.Combine(rootPath, fileName);
                using (var fs = File.OpenWrite(path))
                {
                    page.HtmlDoc.Save(fs);
                }
            }
            else
            {
                path = Path.Combine(dataPath, fileName);
                File.WriteAllBytes(path, page.Data);
            }
            Log.Debug().Message("Write [{0}] to [{1}]", page.Uri.AbsoluteUri, path).Write();
        }

        private string GetDataPath(string rootPath)
        {
            return Path.Combine(rootPath, "data");
        }

        private string GetRelativePath(CrawlerPage page, string fileName)
        {
            return !page.IsHtml
                ? "../data/" + fileName
                : "../" + fileName;
        }

        private static IEnumerable<CrawlerPage> ToEnumerableAtBreadthFirst(CrawlerPage rootPage)
        {
            var queue = new Queue<CrawlerPage>();
            queue.Enqueue(rootPage);
            while (queue.Any())
            {
                var t = queue.Dequeue();
                yield return t;

                foreach (var child in t.ChildPages)
                {
                    queue.Enqueue(child);
                }
            }
        }

        private readonly UriToFileNameConverter _uriToFileNameConverter;
        private readonly IWebPageLinkManager _linkManager;
    }
}