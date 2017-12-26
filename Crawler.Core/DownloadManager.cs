using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crawler.Core
{
    public sealed class DownloadManager
    {
        public DownloadManager(IDownloader downloader, ILinkDataStorage linkDataStorage, CrawlerSettings settings)
        {
            _downloader = downloader;
            _linkDataStorage = linkDataStorage;
            _settings = settings;

            InitDownloaderProcess();
        }

        public DownloaderResult AddToDownloadQueue(Uri uri)
        {
            return AddToDownloadQueue(uri.AbsoluteUri);
        }

        public DownloaderResult AddToDownloadQueue(string uri)
        {
            var result = new DownloaderResult(uri);
            if (_linkDataStorage.TryGetLinkContent(uri, out var content))
            {
                result.SetResponseData(content);
                return result;
            }

            _queue.Add(result);
            return result;
        }


        private void InitDownloaderProcess()
        {
            if (_settings.MaxParallelDownloadsCount > 0)
            {
                for (int i = 0; i < _settings.MaxParallelDownloadsCount; i++)
                {
                    CreateDownloadedProcesses();
                }
            }
            else
            {
                CreateDownloadedProcesses();
            }
        }

        private void CreateDownloadedProcesses()
        {
            _downloadedProcesses.Add(Task.Factory.StartNew(() =>
            {
                foreach (var itm in _queue.GetConsumingEnumerable())
                {
                    var result = _downloader.Download(itm.Uri);
                    itm.CopyFrom(result);

                    _linkDataStorage.SetOrUpdateLinkContent(itm.Uri.AbsoluteUri, itm.Content);
                }
            }));
        }


        private List<Task> _downloadedProcesses = new List<Task>();
        private readonly IDownloader _downloader;
        private readonly ILinkDataStorage _linkDataStorage;
        private readonly CrawlerSettings _settings;
        private BlockingCollection<DownloaderResult> _queue = new BlockingCollection<DownloaderResult>();
    }
}