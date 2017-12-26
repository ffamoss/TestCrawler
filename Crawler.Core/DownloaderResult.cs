using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Crawler.Core
{
    public sealed class DownloaderResult
    {
        public DownloaderResult(string uri)
            : this(new Uri(uri, UriKind.Absolute))
        { }

        public DownloaderResult(Uri uri)
        {
            Uri = uri;
            _waitCompleteTsc = new TaskCompletionSource<DownloaderResult>();
        }

        public Uri Uri { get; }

        public WebException WebException { get; private set; }
        public bool HasError { get; private set; }

        public bool HasContent => Content != null;
        public WebPageContent Content { get; private set; }

        public bool DownloadTimeout { get; set; }

        public Task<DownloaderResult> WaitCompliteTask => _waitCompleteTsc.Task;

        public void SetResponseParams(HttpWebResponse response)
        {
            if (response == null)
            {
                HasError = true;
            }

            //TODO copy response parameters 
        }

        public void SetResponseData(WebPageContent content)
        {
            Content = content;
            _waitCompleteTsc.TrySetResult(this);
        }

        public void SetException(Exception exception)
        {
            HasError = true;
            if (exception is WebException)
            {
                WebException = (WebException)exception;
            }

            _waitCompleteTsc.TrySetResult(this);
        }

        internal void CopyFrom(DownloaderResult result)
        {
            Debug.Assert(Uri.Equals(result.Uri));
            Debug.Assert(!_waitCompleteTsc.Task.IsCompleted);

            WebException = result.WebException;
            HasError = result.HasError;

            Content = result.Content;
            DownloadTimeout = result.DownloadTimeout;

            result.WaitCompliteTask.ContinueWith(t => _waitCompleteTsc.TrySetResult(t.Result));
        }

        private readonly TaskCompletionSource<DownloaderResult> _waitCompleteTsc;
    }
}