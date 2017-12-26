using System;
using System.Threading.Tasks;

namespace Crawler.Core
{
    public interface IDownloader
    {
        DownloaderResult Download(Uri uri);
    }
}