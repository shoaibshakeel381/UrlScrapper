using System.Collections.Generic;

namespace Scrapper
{
    public class DataModel
    {
        public string Url { get; set; }

        public IEnumerable<string> IgnoreUrls { get; set; }

        public bool Verbose { get; set; }

        public int MaxDepth { get; set; }
    }
}
