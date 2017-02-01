using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using RestSharp.Extensions.MonoHttp;
using bank.import.crawl.cleaners;

namespace bank.import.crawl
{
    //[JsonObject(MemberSerialization.OptIn)]
    public class Page
    {
        private static Regex _newlines = new Regex(@"[\n\r]+?", RegexOptions.Compiled);
        private static Regex _pipe = new Regex(@"\s{0,1}\|.{0,20}$", RegexOptions.Compiled);
        private List<BaseCleaner> _cleaners = new List<BaseCleaner>();


        public Page()
        {
            Data = new List<Datum>();
            Titles = new List<string>();
            Media = new List<Media>();
            Icons = new List<string>();

            _cleaners.Add(new TwitterCleaner() { Page = this });
            _cleaners.Add(new FacebookCleaner() { Page = this });

        }

        [JsonIgnore()]
        public string Url { get; set; }


        [JsonIgnore()]
        public string BaseHref { get; set; }

        [JsonIgnore()]
        public string Canonical { get; set; }

        [JsonIgnore()]
        public string Html { get; set; }

        [JsonProperty(PropertyName = "response-url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ResponseUrl { get; set; }

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public List<Datum> Data { get; set; }

        [JsonProperty(PropertyName = "http-status", NullValueHandling = NullValueHandling.Ignore)]
        public double? HttpStatusCode { get; set; }

        [JsonProperty(PropertyName = "twitter-site", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterSite { get; set; }


        [JsonProperty(PropertyName = "twitter-creator", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterCreator { get; set; }


        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }


        [JsonProperty(PropertyName = "content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }


        [JsonProperty(PropertyName = "words", NullValueHandling = NullValueHandling.Ignore)]
        public int? Words { get; set; }


        [JsonProperty(PropertyName = "author", NullValueHandling = NullValueHandling.Ignore)]
        public string Author { get; set; }


        [JsonProperty(PropertyName = "facebook", NullValueHandling = NullValueHandling.Ignore)]
        public string Facebook { get; set; }


        [JsonProperty(PropertyName = "gplus", NullValueHandling = NullValueHandling.Ignore)]
        public string GooglePlus { get; set; }


        [JsonProperty(PropertyName = "youtube", NullValueHandling = NullValueHandling.Ignore)]
        public string YouTube { get; set; }


        [JsonProperty(PropertyName = "lang", NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }


        [JsonProperty(PropertyName = "domain", NullValueHandling = NullValueHandling.Ignore)]
        public string Domain { get; set; }


        [JsonProperty(PropertyName = "date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }


        [JsonProperty(PropertyName = "quality", NullValueHandling = NullValueHandling.Ignore)]
        public double Quality { get; set; }


        [JsonProperty(PropertyName = "sitename", NullValueHandling = NullValueHandling.Ignore)]
        public string Sitename { get; set; }


        [JsonProperty(PropertyName = "titles", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Titles { get; set; }


        [JsonProperty(PropertyName = "excerpts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Excerpts { get; set; }


        [JsonProperty(PropertyName = "dates", NullValueHandling = NullValueHandling.Ignore)]
        public List<DateTime> Dates { get; set; }


        [JsonProperty(PropertyName = "images", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Images { get; set; }


        [JsonProperty(PropertyName = "media", NullValueHandling = NullValueHandling.Ignore)]
        public List<Media> Media { get; set; }

        [JsonProperty(PropertyName = "icons", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Icons { get; set; }

        [JsonProperty(PropertyName = "urls", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Urls { get; set; }


        [JsonProperty(PropertyName = "keywords", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Keywords { get; set; }

        [JsonProperty(PropertyName = "excerpt-words", NullValueHandling = NullValueHandling.Ignore)]
        public int? ExcerptWords { get; set; }

        public List<string> Links { get; set; }
        public List<Media> Logos { get; set; }

        [JsonIgnore()]
        public string RawContent { get; set; }

        public void Consolidate()
        {
            Clean();
            SetBaseHrefAbsolute();
            SetSingleValues();
            ConsolidateTitles();
            ConsolidateExcerpts();
            ConsolidateImages();
            ConsolidateIcons();
            ConsolidateMedia();
            ConsolidateUrls();
            ConsolidateKeywords();
            CalculateQuality();
            CalculateTrueUrl();

            if (Data.Count == 0)
            {
                Data = null;
            }

            RunCleaners();
        }

        private void ConsolidateIcons()
        {
            Icons = Icons.Distinct().ToList();

            var fixedIcons = new List<string>();

            foreach (var icon in Icons)
            {
                fixedIcons.Add(FixUrl(icon));
            }

            Icons = fixedIcons;
        }

        private void RunCleaners()
        {
            foreach (ICleaner cleaner in _cleaners)
            {
                cleaner.Clean();
            }
        }

        private void CalculateTrueUrl()
        {
            if (!string.IsNullOrWhiteSpace(this.Canonical))
            {
                if (Uri.IsWellFormedUriString(this.Canonical, UriKind.Absolute))
                {
                    this.ResponseUrl = new Uri(this.Canonical);
                }
            }
        }

        private void SetBaseHrefAbsolute()
        {

            if (!string.IsNullOrWhiteSpace(BaseHref))
            {
                if (Uri.IsWellFormedUriString(BaseHref, UriKind.Absolute))
                {
                    return;
                }
                else if (Uri.IsWellFormedUriString(BaseHref, UriKind.Relative))
                {
                    var builder = new UriBuilder(ResponseUrl);
                    builder.Query = null;
                    builder.Path = BaseHref;
                    BaseHref = builder.ToString();
                    return;
                }

            }
            else
            {
                var builder = new UriBuilder(ResponseUrl);
                builder.Query = null;

                if (!builder.Path.EndsWith("/"))
                {
                    builder.Path = builder.Path.Substring(0, builder.Path.LastIndexOf("/") + 1);
                }

                BaseHref = builder.ToString();
            }
        }

        private void CalculateQuality()
        {
            int extras = 0;

            if (Images != null)
            {
                Quality++;
                extras += Images.Count - 1;
            }

            if (Excerpts != null)
            {
                Quality++;
                extras += Excerpts.Count - 1;
            }

            if (Titles != null)
            {
                Quality++;
                extras += Titles.Count - 1;
            }

            if (extras > 0)
            {
                if (extras >= 10)
                {
                    Quality += 0.99;
                }
                else
                {
                    Quality += extras / 10.0;
                }

            }


        }

        private void Clean()
        {
            foreach (var datum in Data)
            {
                datum.Value = HttpUtility.HtmlDecode(datum.Value);
            }

            Data.RemoveAll(x => x.Key.Contains("og:image:type"));
            Data.RemoveAll(x => x.Key.Contains("og:locale"));
            Data.RemoveAll(x => x.Key.Contains("twitter:domain"));
            Data.RemoveAll(x => x.Key.Contains("og:updated_time"));
            Data.RemoveAll(x => x.Key.Contains("twitter:app"));

        }

        private void SetSingleValues()
        {

            Type = GetSingleValue("og:type");
            Sitename = GetSingleValue("og:site_name");
            TwitterSite = GetSingleValue("twitter:site");
            TwitterCreator = GetSingleValue("twitter:creator");
            Author = GetSingleValue("author");

            Content = GetSingleValue("articleBody");
            if (!string.IsNullOrWhiteSpace(Content))
            {
                Words = Content.Split(' ').Count();

                if (Content.Length > 2000)
                {
                    Content = Content.Substring(0, 1000);
                }
            }

            var datePublishedString = GetSingleValue("datePublished");
            DateTime d;
            if (DateTime.TryParse(datePublishedString, out d))
            {
                Date = d;
            }


            Data.RemoveAll(x => x.Key.Contains("twitter:card"));

            if (Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                var uri = new Uri(Url);
                Domain = uri.Host;
            }

        }

        private string GetSingleValue(string key)
        {
            string result = null;

            var datum = Data.FirstOrDefault(x => x.Key == key);
            if (datum != null)
            {
                result = datum.Value;
                Data.RemoveAll(x => x.Key.Contains(key));
            }

            return result;
        }

        private void ConsolidateTitles()
        {
            var titles = Data.Where(x => x.Key.Contains("title")).Select(x => _pipe.Replace(x.Value, "")).ToList();

            Titles = DeDup(titles).Where(x => x.Length > 20).OrderBy(x => Math.Abs(x.Length - 10)).ToList();

            if (Titles.Count == 0) Titles = null;

            Data.RemoveAll(x => x.Key.Contains("title"));
        }

        private void ConsolidateExcerpts()
        {
            var excerpts = Data.Where(x => x.Key.Contains("description")).Select(x => x.Value).ToList();
            Excerpts = DeDup(excerpts).OrderBy(x => Math.Abs(x.Length - 50)).ToList();

            if (Excerpts.Count == 0) Excerpts = null;

            if (Excerpts != null)
            {
                ExcerptWords = Excerpts.First().Split(' ').Count();
            }

            Data.RemoveAll(x => x.Key.Contains("description"));
        }

        //private void ConsolidateLinks()
        //{
        //    var links = Data.Where(x => x.Key.Contains("href")).ToList();

        //    foreach(var link in links)
        //    {
        //        var type = HrefTypes.Unknown;

        //        //if (link.Value.StartsWith())
        //    }

        //    //Links = links;

        //    Data.RemoveAll(x => x.Key.Contains("href"));
        //}

        private void ConsolidateImages()
        {
            var exclude = new string[] { "og:image:width", "og:image:height" };
            var images = Data.Where(x => x.Key.Contains("image") && !exclude.Contains(x.Key)).Select(x => HttpUtility.HtmlDecode(x.Value)).ToList();
            Images = DeDup(images).OrderByDescending(x => x.Length).ToList();

            if (Images.Count == 0) Images = null;

            Data.RemoveAll(x => x.Key.Contains("image"));
        }


        private void ConsolidateMedia()
        {
            foreach (var media in Media)
            {
                var url = FixUrl(media.Url);
                media.Url = url;
            }

            var comparer = new mediaComparer();
            Media = Media.Distinct(comparer).ToList();

            if (!Media.Any())
            {
                Media = null;
            }
        }

        private class mediaComparer : EqualityComparer<Media>
        {
            public override bool Equals(Media x, Media y)
            {
                return x.Type == y.Type && x.Url == y.Url;
            }

            public override int GetHashCode(Media obj)
            {
                return 0;
            }
        }

        private string StripQuery(string url)
        {
            try
            {
                var builder = new UriBuilder(url);
                builder.Query = null;
                return builder.ToString();
            }
            catch
            {
                return url;
            }

        }

        private string FixUrl(string url)
        {
            url = HttpUtility.HtmlDecode(url);

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url;
            }

            if (url.StartsWith("http://") ||
                url.StartsWith("https://") ||
                url.StartsWith("//"))
            {
                return url;
            }


            var builder = new UriBuilder(BaseHref);

            if (url.StartsWith("/"))
            {
                builder.Path = url;
            }
            else
            {
                builder.Path += url;
            }
            return builder.ToString();

        }

        private void ConsolidateUrls()
        {
            var urls = Data.Where(x => x.Key.Contains("url")).Select(x => x.Value).ToList();
            Urls = DeDup(urls).OrderByDescending(x => x.Length).ToList();

            Urls.Remove(this.Url);

            if (Urls.Count == 0) Urls = null;

            Data.RemoveAll(x => x.Key.Contains("url"));
        }

        private void ConsolidateKeywords()
        {
            var skip = new string[] { "general" };

            var keywordLists = Data.Where(x => x.Key.Contains("keywords")).Select(x => x.Value).ToList();

            var keywords = new ConcurrentDictionary<string, int>();

            foreach (var list in keywordLists)
            {
                var terms = list.Replace(";", ",").ToLower();
                var words = terms.Split(',');

                foreach (var word in words)
                {
                    if (skip.Contains(word)) continue;

                    keywords.AddOrUpdate(word.Trim(), 1, (key, existing) =>
                    {
                        return ++existing;
                    });
                }
            }

            Keywords = keywords.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

            if (Keywords.Count == 0) Keywords = null;

            Data.RemoveAll(x => x.Key.Contains("keywords"));
        }

        private List<string> DeDup(List<string> source)
        {
            var destCandidates = new List<string>();
            return source.Select(x => _newlines.Replace(x.Trim(), " ")).Distinct().ToList();


            //if (source.Count == 1)
            //{
            //    dest = source;
            //    return;
            //}

            //destCandidates.Add(source.First());
            //source.RemoveAt(0);

            //foreach (var sourceItem in source)
            //{
            //    var sourceWords = sourceItem.ToLower().Split(' ').Distinct().ToList();
            //    sourceWords.Sort();

            //    foreach (var destItem in destCandidates)
            //    {
            //        var destWords = destItem.ToLower().Split(' ').Distinct().ToList();
            //        destWords.Sort();

            //        int matches = (from s in sourceWords
            //                       join d in destWords
            //                       on s equals d
            //                       select new { s }).Count();

            //        var sourceRatio = (double)matches / (double)sourceWords.Count();
            //        var destRatio = (double)matches / (double)destWords.Count();



            //    }
            //}
        }
    }


    public class Media
    {
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }

    [DebuggerDisplay("{Key}={Value}")]
    public class Datum
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }

    public class Href
    {
        public HrefTypes Type { get; set; }
        public string Url { get; set; }

    }

    public enum HrefTypes
    {
        Unknown,
        Internal,
        External,
        Facebook,
        Twitter,
        GooglePlus,
        LinkedIn,
        Instagram,
        Pinterest,
        Youtube
    }
}
