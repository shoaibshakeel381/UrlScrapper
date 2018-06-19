using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace Scrapper
{
    public class ScrapperAgent
    {
        protected IConfiguration config;

        public ScrapperAgent()
        {
            config = Configuration.Default.WithDefaultLoader();
        }

        public IEnumerable<string> GetPageLinks(string url)
        {
            var result = GetPageLinksAsync(url);
            result.Wait();

            return result.Result.Select(a => a.GetAttribute("href"));
        }

        protected async Task<IHtmlCollection<IElement>> GetPageLinksAsync(string url)
        {
            var document = await BrowsingContext.New(config).OpenAsync(url);

            var querySelectorAll = document.Links;

            return querySelectorAll;
        }
    }
}
