using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scrapper
{
    public class Manipulator
    {
        /// <summary>
        /// Maximum depth of link exploration
        /// </summary>
        protected int maxDepth;

        /// <summary>
        /// All explored links
        /// </summary>
        protected ISet<string> parsedLinks;

        /// <summary>
        /// Print messages during execution
        /// </summary>
        protected bool verbose;

        /// <summary>
        /// Depth of a link in exploration tree
        /// </summary>
        protected IDictionary<string, int> linksDepth;

        /// <summary>
        /// Links yet to be explored
        /// </summary>
        protected ISet<string> queuedLinks;

        /// <summary>
        /// Links which should be ignored during exploration
        /// </summary>
        protected ISet<string> ignoredLinks;

        /// <summary>
        /// Web Scrapper instance
        /// </summary>
        protected ScrapperAgent scrapperAgent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="linkToParse"></param>
        /// <param name="parseMaxDepth"></param>
        /// <param name="verbose"></param>
        public Manipulator(string linkToParse, int parseMaxDepth, bool verbose = false)
        {
            maxDepth = parseMaxDepth;
            this.verbose = verbose;
            parsedLinks = new HashSet<string>();
            linksDepth = new Dictionary<string, int>();
            queuedLinks = new HashSet<string>();
            ignoredLinks = new HashSet<string>();

            scrapperAgent = new ScrapperAgent();

            QueueLinks(new[] { linkToParse }, 0);
        }

        /// <inheritdoc />
        public Manipulator(string linkToParse, IEnumerable<string> ignoreLinks,int parseDepth, bool verbose = false) 
            : this(linkToParse, parseDepth, verbose)
        {
            ignoredLinks = new HashSet<string>(ignoreLinks);
        }

        /// <summary>
        /// Start Link Explorartion
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Parse()
        {
            while (queuedLinks.Count != 0)
            {
                var linkToParse = queuedLinks.First();

                Parse(linkToParse);

                if (verbose)
                {
                    Console.WriteLine($"Link: {linkToParse}");
                }
                Console.WriteLine($"Queue: {queuedLinks.Count}, Parsed: {parsedLinks.Count}, Current Depth: {linksDepth[linkToParse]}\n");

                queuedLinks.Remove(linkToParse);
                linksDepth.Remove(linkToParse);
            }

            return ExtractDomainNames(parsedLinks);
        }

        /// <summary>
        /// Explore a single link
        /// </summary>
        /// <param name="linkToParse"></param>
        protected void Parse(string linkToParse)
        {
            if (linksDepth[linkToParse] > maxDepth)
                return;
            
            var links = scrapperAgent.GetPageLinks(linkToParse);
            
            QueueLinks(links, linksDepth[linkToParse] + 1);

            // Marked this link as parsed
            parsedLinks.Add(linkToParse);
        }

        /// <summary>
        /// Put new links in Queue at given depth
        /// </summary>
        /// <param name="links"></param>
        /// <param name="depth"></param>
        protected void QueueLinks(IEnumerable<string> links, int depth)
        {
            // Filter-out unnecessary links
            var filteredLinks = FilterLinks(links);
            
            // Do not queue links whose depth exceeds the specified depth
            // Since we already found these then just mark them as processed.
            if (depth > maxDepth)
            {
                foreach (var link in filteredLinks)
                {
                    parsedLinks.Add(link);
                }
                return;
            }

            foreach (var link in filteredLinks)
            {
                QueueLink(link, depth);
            }
        }

        /// <summary>
        /// Put new link in Queue at given depth
        /// </summary>
        /// <param name="link"></param>
        /// <param name="depth"></param>
        protected void QueueLink(string link, int depth)
        {
            queuedLinks.Add(link);
            if (!linksDepth.ContainsKey(link))
                linksDepth.Add(link, depth);
        }

        /// <summary>
        /// Remove unnecessary links
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public IEnumerable<string> FilterLinks(IEnumerable<string> links)
        {
            // Remove all local anchors starting with #
            // Remove all local links starting with /
            // Remove all empty links
            // Remove all links like tel:2343 or mailto:email@tes.com
            // Remove all links which don't have a period
            // Remove JS, CSS and Images
            // Append http:// to links
            var filteredLinks = new List<string>();
            foreach (var link in links)
            {
                var filteredLink = link.Trim();
                if (filteredLink.Length == 0)
                    continue;
                
                if (Regex.IsMatch(filteredLink, @"^#|^.?.?\/", RegexOptions.Compiled))
                    continue;
                if (Regex.IsMatch(filteredLink, @"\/.*\.(css|js|jpeg|jpg|png|gif|bmp|svg)(\?.*)?$", 
                    RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    continue;
                if (!Regex.IsMatch(filteredLink, @"^https?:\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    filteredLink = "http://" + filteredLink;
                if (Regex.IsMatch(filteredLink, @"^https?:\/\/.*[:]", RegexOptions.Compiled))
                    continue;

                string domainName = Regex.Replace(filteredLink, @"^https?:\/\/", "").Split('/')[0];
                if (Regex.IsMatch(domainName, @"[.]", RegexOptions.Compiled) && !parsedLinks.Contains(link) && !ShouldItBeIgnored(link) && !IsDomainNameAlreadyProcessed(link))
                    filteredLinks.Add(filteredLink);
            }
            return filteredLinks;
            //return links.Select(a => a.Replace("http://", "").Replace("https://", ""))
            //    .Where(a => a.Trim().Length > 0 && !a.StartsWith("#") && !a.StartsWith("/") && !a.Contains(":") && a.Contains("."));
        }

        protected bool ShouldItBeIgnored(string link)
        {
            return ignoredLinks.Any(a => Regex.IsMatch(link, a, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        protected bool IsDomainNameAlreadyProcessed(string link)
        {
            var parts = Regex.Replace(link, @"^https?:\/\/", "").Split('/');
            if (queuedLinks.Any(a => Regex.IsMatch(a, parts[0])))
                return true;

            if (parsedLinks.Any(a => Regex.IsMatch(a, parts[0])))
                return true;

            return false;
        }

        /// <summary>
        /// Extract only domain names from remaning links
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public IEnumerable<string> ExtractDomainNames(IEnumerable<string> links)
        {
            var uniqeLinks = new HashSet<string>();
            foreach (var link in links)
            {
                var parts = Regex.Replace(link, @"^https?:\/\/", "").Split('/');
                uniqeLinks.Add(parts[0]);
            }

            return uniqeLinks.AsEnumerable();
        }
    }
}
