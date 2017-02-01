using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace bank.import.crawl.cleaners
{
    class TwitterCleaner : BaseCleaner
    {
        private static Regex _twitter = new Regex(@"http[s]{0,1}://(www\.){0,1}twitter.com/(?<handle>[\d\w]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public override void Clean()
        {
            if (!string.IsNullOrWhiteSpace(Page.TwitterSite))
            {
                return;
            }

            var handles = Page.Data.Where(x => x.Key == "href" && _twitter.IsMatch(x.Value));
            var links = handles.Select(x => x.Value).Distinct();

            if(links.Count() == 1)
            {
                Page.TwitterSite = _twitter.Match(links.First()).Groups["handle"].Value;
            }
        }
    }
}
