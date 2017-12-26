using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Crawler.Core
{
    public interface IWebPageLinkManager
    {
        Dictionary<Uri, List<HtmlNode>> GetAllLinks(Uri baseUri, HtmlDocument doc);
        void ReplaceLink(HtmlNode node, string newLink);
    }
}