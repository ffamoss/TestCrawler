using System;
using System.Net;
using NLog.Fluent;

namespace Crawler.Core
{
    public sealed class WebDownloader : IDownloader
    {
        public WebDownloader(CrawlerSettings settings, IWebContentExtractor contentExtractor)
        {
            _settings = settings;
            _contentExtractor = contentExtractor;
            _proxy = new Lazy<IWebProxy>(() =>
            {
                var defaultProxy = WebRequest.GetSystemWebProxy();
                defaultProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                return defaultProxy;
            });
            _cookieContainer = new CookieContainer();
        }

        public DownloaderResult Download(Uri uri)
        {
            Log.Info().Message("Request to download [{0}]", uri.AbsoluteUri).Write();
            return MakeRequest(uri);
        }

        private DownloaderResult MakeRequest(Uri uri)
        {
            var result = new DownloaderResult(uri);
            HttpWebRequest request;
            HttpWebResponse response = null;
            try
            {
                request = BuildRequestObject(uri);
                response = (HttpWebResponse)request.GetResponse();
                ProcessResponseObject(response);
            }
            catch (WebException e)
            {
                result.SetException(e);

                if (e.Response != null)
                    response = (HttpWebResponse)e.Response;

                Log.Debug()
                    .Message("Error occurred requesting url [{0}]", uri.AbsoluteUri)
                    .Exception(e)
                    .Write();
            }
            catch (Exception e)
            {
                result.SetException(e);

                Log.Debug()
                    .Message("Error occurred requesting url [{0}]", uri.AbsoluteUri)
                    .Exception(e)
                    .Write();
            }
            finally
            {
                try
                {
                    result.SetResponseParams(response);
                    result.SetResponseData(_contentExtractor.GetContent(response));

                    response?.Close(); 
                }
                catch (Exception e)
                {
                    result.SetException(e);

                    Log.Info()
                        .Message("Error occurred finalizing requesting url [{0}]", uri.AbsoluteUri)
                        .Exception(e)
                        .Write();
                }

            }
            return result;
        }

        private HttpWebRequest BuildRequestObject(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.UserAgent = _settings.UserAgent;
            request.Accept = _settings.SupportedContentTypeString;
            request.Timeout = (int)_settings.RequestTimeout.TotalMilliseconds;
            request.Proxy = _proxy.Value;

            if (_settings.IsCookieEnabled)
            {
                request.CookieContainer = _cookieContainer;
            }

            return request;
        }

        private void ProcessResponseObject(HttpWebResponse response)
        {
            if (response != null && _settings.IsCookieEnabled)
            {
                CookieCollection cookies = response.Cookies;
                _cookieContainer.Add(cookies);
            }
        }

        private readonly CrawlerSettings _settings;
        private readonly IWebContentExtractor _contentExtractor;
        private readonly Lazy<IWebProxy> _proxy;
        private readonly CookieContainer _cookieContainer;
    }
}