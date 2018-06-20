using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace Scrapper
{
    public class ScrapperAgent
    {
        protected IConfiguration config;
        protected IBrowsingContext browsingContext;

        public ScrapperAgent()
        {
            config = Configuration.Default.WithDefaultLoader();
            browsingContext = BrowsingContext.New(config);
        }

        public IEnumerable<string> GetPageLinks(string url)
        {
            var result = GetPageLinksAsync(url);
            result.Wait();

            return result.Result;
        }

        protected async Task<IEnumerable<string>> GetPageLinksAsync(string url)
        {
            var document = await browsingContext.OpenAsync(url);
            return document.Links.Select(a => a.GetAttribute("href"));
        }
    }
}
