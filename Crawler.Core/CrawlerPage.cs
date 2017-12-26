using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Crawler.Core
{
    public class CrawlerPage
    {
        public CrawlerPage()
        {
            Links = new Dictionary<Uri, List<HtmlNode>>();
            ChildPages = new List<CrawlerPage>();
        }

        public Uri Uri { get; set; }
        public int Level { get; set; }
        public bool IsRoot { get; set; }
        public byte[] Data { get; set; }

        public bool IsHtml { get; set; }
        public string Html { get; set; }
        public HtmlDocument HtmlDoc { get; set; }
        public Dictionary<Uri, List<HtmlNode>> Links { get; set; }

        public List<CrawlerPage> ChildPages { get; set; } 
    }
}