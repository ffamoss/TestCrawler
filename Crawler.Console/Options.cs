using CommandLine;
using CommandLine.Text;

namespace Crawler.Console
{
    class Options
    {
        [Option('s', HelpText = "Web page address", Required = true)]
        public string Uri { get; set; }
        [Option('p', HelpText = "Local path to save", Required = true)]
        public string Path { get; set; }
        [Option('r', HelpText = "Is need replace links to local", DefaultValue = true)]
        public bool ReplaceToLocal { get; set; }
        [Option('d',HelpText = "Crawling depth", DefaultValue = 2)]
        public int Depth { get; set; }
        [Option('i', HelpText = "Ignore other domains", DefaultValue = false)]
        public bool IgnoreOtherDomains { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}