using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using NLog.Fluent;

namespace Crawler.Core
{
    public sealed class WebPageLinkManager : IWebPageLinkManager
    {
        //Note naive implementation, many cases not supported
        public Dictionary<Uri, List<HtmlNode>> GetAllLinks(Uri baseUri, HtmlDocument doc)
        {
            var links = GetLinks(doc);
            var refs = GetReferences(doc);

            var result = new Dictionary<Uri, List<HtmlNode>>();
            foreach (var lnkNode in links.Concat(refs))
            {
                var link = lnkNode.Item1;
                if (Uri.TryCreate(baseUri, link, out var uri))
                {
                    if (result.ContainsKey(uri))
                    {
                        result[uri].Add(lnkNode.Item2);
                    }
                    else
                    {
                        result[uri] = new List<HtmlNode> {lnkNode.Item2};
                    }
                }
            }
            
            return result;
        }

        public void ReplaceLink(HtmlNode node, string newLink)
        {
            var attribute = new[] {"background", "href", "src", "lowsrc", "href"}
            .Select(tag => node.Attributes[tag])
                .FirstOrDefault(attr => !string.IsNullOrWhiteSpace(attr?.Value));

            if (attribute == null)
            {
                Log.Error().Message("Cannot find link to replace on node{0}{1}",Environment.NewLine, node.InnerText);
                return;
            }

            attribute.Value = newLink;
        }

        private List<Tuple<string, HtmlNode>> GetLinks(HtmlDocument doc)
        {
            var links = new List<Tuple<string, HtmlNode>>();

            var atts = doc.DocumentNode.SelectNodes("//*[@background or @lowsrc or @src or @href]");
            if (atts == null)
            {
                return links;
            }

            foreach (HtmlNode n in atts)
            {
                ParseLink(links, n, "background");
                ParseLink(links, n, "href");
                ParseLink(links, n, "src");
                ParseLink(links, n, "lowsrc");
            }

            return links;
        }

        private List<Tuple<string, HtmlNode>> GetReferences(HtmlDocument doc)
        {
            var refs = new List<Tuple<string, HtmlNode>>();

            var hrefs = doc.DocumentNode.SelectNodes("//a[@href]");
            if (hrefs == null)
            {
                return refs;
            }

            foreach (HtmlNode href in hrefs)
            {
                refs.Add(Tuple.Create(href.Attributes["href"].Value, href));
            }

            return refs;
        }


        private void ParseLink(List<Tuple<string, HtmlNode>> links, HtmlNode node, string name)
        {
            var att = node.Attributes[name];
            if (att == null)
                return;

            if (name == "href" && node.Name != "link")
                return;

            links.Add(Tuple.Create(att.Value, node));
        }
    }
}