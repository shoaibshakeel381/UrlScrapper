using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scrapper
{
    public class Manipulator
    {
        protected DataModel InputData { get; }

        protected ProcessingLists Lists { get; }
        
        protected ScrapperAgent scrapperAgent;

        public Manipulator(DataModel inputData)
        {
            InputData = inputData;
            Lists = new ProcessingLists();
            scrapperAgent = new ScrapperAgent();

            QueueLinks("root", new[] { InputData.Url }, 0);
        }

        /// <summary>
        /// Start Link Explorartion
        /// </summary>
        public IEnumerable<string> Parse()
        {
            while (Lists.QueuedLinks.Count != 0)
            {
                var linkToParse = Lists.QueuedLinks.First();

                try
                {
                    Parse(linkToParse);
                    if (InputData.Verbose)
                    {
                        Console.WriteLine($"Link: {linkToParse}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred when parsing url:\n{linkToParse}");
                    if (InputData.Verbose)
                    {
                        Console.WriteLine(e);
                    }
                }
                
                Console.WriteLine($"Queue: {Lists.QueuedLinks.Count}, Parsed: {Lists.ParsedLinks.Count}, Current Depth: {Lists.LinksDepth[linkToParse]}\n");

                Lists.QueuedLinks.Remove(linkToParse);
                Lists.LinksDepth.Remove(linkToParse);
            }

            return ExtractDomainNames(Lists.ParsedLinks);
        }

        private void Parse(string linkToParse)
        {
            if (Lists.LinksDepth[linkToParse] > InputData.MaxDepth)
                return;
            
            var links = scrapperAgent.GetPageLinks(linkToParse);
            
            QueueLinks(linkToParse, links, Lists.LinksDepth[linkToParse] + 1);

            // Marked this link as parsed
            Lists.ParsedLinks.Add(linkToParse);
        }

        /// <summary>
        /// Put new links in Queue at given depth
        /// </summary>
        protected void QueueLinks(string parentLink, IEnumerable<string> links, int depth)
        {
            // Filter-out unnecessary links
            var filteredLinks = FilterLinks(links);
            
            // Do not queue links whose depth exceeds the specified depth
            // But Since we already found these then just mark them as processed.
            if (depth > InputData.MaxDepth)
            {
                foreach (var link in filteredLinks)
                {
                    Lists.ParsedLinks.Add(link);
                }
                return;
            }

            foreach (var link in filteredLinks)
            {
                QueueLink(parentLink, link, depth);
            }
        }

        private void QueueLink(string parentLink, string link, int depth)
        {
            // If Same domain exploration is prohibited than skip link
            if (!InputData.ParseSameDomainLinks && IsDomainNameAlreadyProcessed(link))
            {
                return;
            }

            Lists.QueuedLinks.Add(link);

            if (!Lists.LinkParent.ContainsKey(link))
                Lists.LinkParent.Add(link, parentLink);

            if (!Lists.LinksDepth.ContainsKey(link))
                Lists.LinksDepth.Add(link, depth);
        }

        /// <summary>
        /// Remove unnecessary links
        /// </summary>
        public IEnumerable<string> FilterLinks(IEnumerable<string> links)
        {
            var filteredLinks = new List<string>();
            foreach (var link in links)
            {
                var filteredLink = link.Trim();

                // Remove all empty links
                if (filteredLink.Length == 0)
                    continue;

                // Remove all local anchors starting with # or /
                if (Regex.IsMatch(filteredLink, @"^#|^.?.?\/", RegexOptions.Compiled))
                    continue;

                // Remove JS, CSS and Images
                if (Regex.IsMatch(filteredLink, @"\/.*\.(css|js|jpeg|jpg|png|gif|bmp|svg)(\?.*)?$", 
                    RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    continue;

                // Append http:// to links
                if (!Regex.IsMatch(filteredLink, @"^https?:\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    filteredLink = "http://" + filteredLink;

                // Remove all links like tel:2343 or mailto:email@tes.com
                if (Regex.IsMatch(filteredLink, @"^https?:\/\/.*[:]", RegexOptions.Compiled))
                    continue;

                var domainName = ExtractDomainName(filteredLink);

                // Remove all links which don't have a period
                if (!Regex.IsMatch(domainName, @"[.]", RegexOptions.Compiled))
                    continue;

                // Remove all local links which might look like external links. e.g. index.html
                if (Regex.IsMatch(domainName.Split('.')[1], @"html?|php|asp*|jsp*|cgi", 
                    RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    continue;

                if (!Lists.ParsedLinks.Contains(link) && !ShouldItBeIgnored(link) && !IsDomainNameAlreadyProcessed(link))
                    filteredLinks.Add(filteredLink);
            }
            return filteredLinks;
        }

        protected bool ShouldItBeIgnored(string link)
        {
            return InputData.IgnoreUrls.Any(a => Regex.IsMatch(link, a, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        protected bool IsDomainNameAlreadyProcessed(string link)
        {
            var domainName = ExtractDomainName(link);
            if (Lists.QueuedLinks.Any(a => Regex.IsMatch(a, domainName)))
                return true;

            if (Lists.ParsedLinks.Any(a => Regex.IsMatch(a, domainName)))
                return true;

            return false;
        }

        /// <summary>
        /// Extract only domain names from remaning links
        /// </summary>
        public IEnumerable<string> ExtractDomainNames(IEnumerable<string> links)
        {
            var uniqeLinks = new HashSet<string>();
            foreach (var link in links)
            {
                uniqeLinks.Add(ExtractDomainName(link));
            }

            return uniqeLinks.AsEnumerable();
        }

        public string ExtractDomainName(string link)
        {
            return Regex.Replace(link, @"^https?:\/\/", "").Split('/')[0];
        }

        protected class ProcessingLists
        {
            /// <summary>
            /// All explored links
            /// </summary>
            public ISet<string> ParsedLinks { get; }

            /// <summary>
            /// Depth of a link in exploration tree
            /// </summary>
            public IDictionary<string, int> LinksDepth { get; }

            /// <summary>
            /// Links yet to be explored
            /// </summary>
            public ISet<string> QueuedLinks { get; }

            /// <summary>
            /// This will store parent of a link for debugging purposes. Key is link and value is parent link
            /// </summary>
            public IDictionary<string, string> LinkParent { get; }

            public ProcessingLists()
            {
                ParsedLinks = new HashSet<string>();
                LinksDepth = new Dictionary<string, int>();
                QueuedLinks = new HashSet<string>();
                LinkParent = new Dictionary<string, string>();
            }
        }
    }
}
