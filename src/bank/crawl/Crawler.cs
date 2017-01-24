using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
//using Serilog;


namespace bank.crawl
{
    public class Crawler
    {
        public delegate bool UrlHandler(string url);

        public static event UrlHandler CheckUrl;

        private static Regex _jsRedirect = new Regex(@"window.location.replace\('(?<url>.+?)'\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _snsanalyticsPost = new Regex("<input type=\"hidden\" name=\"url\" value=\"(?<url>.*?)\" id=\"id_url\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //private static Regex _metaRefresh = new Regex("<meta http-equiv=\"refresh\" content=\"0;URL='(?<url>.+?)'\">", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _metaRefresh = new Regex("(?<=url=).+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static int _cacheSize = 100;
        private static ConcurrentDictionary<string, Page> _cache = new ConcurrentDictionary<string, Page>();
        private static ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        private static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = false;

            return true;
        }

        static Crawler()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        private static bool OnCheckUrl(string url)
        {
            if (CheckUrl != null)
            {
                return CheckUrl(url);
            }

            return true;
        }

        private static Page CheckCache(string url)
        {
            Page page;
            if (_cache.TryGetValue(url, out page))
            {
                return page;
            }
            else
            {
                return null;
            }
        }

        private static void AddToCache(List<string> urls, Page page)
        {
            while (_cache.Count >= _cacheSize)
            {
                Page pageToRemove;
                string urlToRemove;

                if (_queue.TryDequeue(out urlToRemove))
                {
                    _cache.TryRemove(urlToRemove, out pageToRemove);
                }
            }

            foreach (var url in urls)
            {
                if (_cache.TryAdd(url, page))
                {
                    _queue.Enqueue(url);
                }
            }
        }

        public static Page Get(string url)
        {
            //Log.Information("Crawling {0}", url);



            var originalUrl = url;
            url = CleanUrl(url);
            //OpenGraph graph = OpenGraph.ParseUrl(url, userAgent: "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36");

            var crawler = new Crawler(url);

            //var html = new HtmlAgilityPack.HtmlWeb();
            //html.PreRequest = OnPreRequest;

            var client = new RestSharp.RestClient();
            client.Timeout = 10000;
            client.ReadWriteTimeout = 5000;

            client.FollowRedirects = false;
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36";

            //html.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36";

            try
            {
                int counter = 0;
                HtmlDocument doc = new HtmlDocument();
                var urls = new List<string>();

                while (counter++ < 5)
                {
                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        break;
                    }

                    urls.Add(url);
                    client.BaseUrl = new Uri(url);
                    var cachedPage = CheckCache(url);

                    if (cachedPage != null) return cachedPage;

                    var request = new RestSharp.RestRequest(RestSharp.Method.GET);
                    request.AddHeader("Accept", "*/*");

                    var tcs = new TaskCompletionSource<IRestResponse>();

                    var responseHandle = client.ExecuteAsync(request, (resp, sender) =>
                    {
                        //Console.WriteLine("hello");
                        tcs.SetResult(resp);

                    });

                    if (tcs.Task.Wait(15000))
                    {
                        var response = tcs.Task.Result;
                        crawler.Page.HttpStatusCode = (double)response.StatusCode;

                        if (crawler.Page.HttpStatusCode >= 300 && crawler.Page.HttpStatusCode < 400)
                        {
                            url = response.Headers.Single(x => x.Name == "Location").Value.ToString();
                            continue;
                        }


                        crawler.Page.ResponseUrl = response.ResponseUri;

                        doc.LoadHtml(response.Content);

                        var metaRefresh = doc.DocumentNode.SelectNodes("//meta[translate(@http-equiv, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'refresh']");


                        if (metaRefresh != null && metaRefresh.Count > 0)
                        {
                            var attribVal = metaRefresh.First().Attributes["content"].Value;
                            url = _metaRefresh.Match(attribVal).Value.Replace("'", "");

                            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                            {
                                var builder = new UriBuilder(response.ResponseUri);
                                builder.Path = url;
                                url = builder.ToString();
                            }
                        }
                        //else if (_metaRefresh.IsMatch(doc.DocumentNode.OuterHtml))
                        //{   //e.g. https://www.google.com/url?rct=j&sa=t&url=https://www.youtube.com/watch%3Fv%3DvnXmLpQt5nk&ct=ga&cd=CAIyGmNkNDkxYzcxMTViMWIzMTM6Y29tOmVuOlVT&usg=AFQjCNFzdL0r4yYgHt3H6wPnytkIdCEPiA&utm_source=twitterfeed&utm_medium=twitter
                        //    url = _metaRefresh.Match(doc.DocumentNode.OuterHtml).Groups["url"].Value;
                        //}
                        else if (_jsRedirect.IsMatch(doc.DocumentNode.OuterHtml))
                        {
                            url = _jsRedirect.Match(doc.DocumentNode.OuterHtml).Groups["url"].Value.Replace(@"\", "");
                        }
                        else if (_snsanalyticsPost.IsMatch(doc.DocumentNode.OuterHtml))
                        {
                            url = _snsanalyticsPost.Match(doc.DocumentNode.OuterHtml).Groups["url"].Value;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        crawler.Page.HttpStatusCode = (int)HttpStatusCode.NotAcceptable;
                        responseHandle.Abort();
                        break;
                    }



                }

                if (crawler.Page.HttpStatusCode != 200)
                {
                    //Log.Warning("Unable to crawl {0}", url);
                    return crawler.Page;
                }

                //crawler.Page.ResponseUrl = html.ResponseUri;

                //if (!OnCheckUrl(crawler.Page.ResponseUrl.ToString()))
                //{   //the caller decided this should not be crawled.
                //    return crawler.Page;
                //}

                crawler.Page.Html = doc.DocumentNode.OuterHtml;

                //canonical
                crawler.Canonical(doc.DocumentNode.SelectNodes("//link[@rel = 'canonical']"));

                //open graph
                crawler.OpenGraph(doc.DocumentNode.SelectNodes("//meta[starts-with(@property, 'og:')]"));

                //twitter
                crawler.Twitter(doc.DocumentNode.SelectNodes("//meta[starts-with(@name, 'twitter:')]"));

                //title tag
                crawler.Element(doc.DocumentNode.SelectNodes("//head//title"));

                //base href
                crawler.BaseHref(doc.DocumentNode.SelectNodes("//base[@href]"));

                //meta description
                crawler.MetaName(doc.DocumentNode.SelectNodes("//meta[@name = 'description']"));

                //meta keywords
                crawler.MetaName(doc.DocumentNode.SelectNodes("//meta[@name = 'keywords']"));

                //meta categories
                crawler.MetaName(doc.DocumentNode.SelectNodes("//meta[@name = 'category']"));

                //meta news_keywords
                crawler.MetaName(doc.DocumentNode.SelectNodes("//meta[@name = 'news_keywords']"));

                //links
                crawler.Links(doc.DocumentNode.SelectNodes("//*[@href]"));

                //schema.org ld+json
                crawler.LdJson(doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']"));

                //schema.org microdata
                crawler.Microdata(doc.DocumentNode.SelectNodes("//@itemscope"));

                //page language
                crawler.PageLanguage(doc.DocumentNode.SelectNodes("//@lang"));

                //other images
                crawler.MediaImages(doc.DocumentNode.SelectNodes("//img"));

                //icons
                crawler.Icons(doc.DocumentNode.SelectNodes("//link[contains(@rel, 'icon')]"));

                crawler.ArticleSections(doc.DocumentNode.SelectNodes("//article"));

                crawler.Page.RawContent = doc.DocumentNode.OuterHtml;

                crawler.Page.Consolidate();

                AddToCache(urls, crawler.Page);

                return crawler.Page;
            }
            catch (Exception ex)
            {
                //Log.Error(ex, "Error crawling url {0}", url);
                return null;
            }
        }

        private static string CleanUrl(string url)
        {
            url = url.Trim();

            var http = url.IndexOf("http");

            if (http > 0)
            {
                url = url.Substring(http);
            }

            var last = url.Last();

            if (last == '\'' || last == '"')
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public Page Page { get; set; }

        public Crawler(string url)
        {
            Page = new Page();
            Page.Url = url;
        }

        private void OpenGraph(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                try
                {
                    var key = node.Attributes["property"].Value;
                    var value = node.Attributes["content"].Value;

                    Page.Data.Add(new Datum { Key = key, Value = value });
                }
                catch { }

            }
        }

        private void Canonical(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                var key = node.Attributes["rel"].Value;
                var value = node.Attributes["href"].Value;

                Page.Canonical = value;

            }
        }

        private void Twitter(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;


            foreach (var node in nodes)
            {
                try
                {
                    var key = node.Attributes["name"].Value;
                    var value = node.Attributes["content"].Value;

                    Page.Data.Add(new Datum { Key = key, Value = value });
                }
                catch { }
            }

        }

        private void Links(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;


            foreach (var node in nodes)
            {
                try
                {
                    var key = "href";
                    var value = node.Attributes["href"].Value;

                    var attribs = node.Attributes.ToDictionary(x => x.Name, x => x.Value);

                    Page.Data.Add(new Datum { Key = key, Value = value, Attributes = attribs });
                }
                catch { }
            }

        }

        private void MetaName(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                try
                {
                    var key = node.Attributes["name"].Value;
                    var value = node.Attributes["content"].Value;

                    Page.Data.Add(new Datum { Key = key, Value = value });
                }
                catch { }
            }
        }

        private void Element(HtmlNodeCollection nodes, string attrName = null)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                var key = node.Name;
                string value = null;

                if (attrName != null)
                {
                    value = node.GetAttributeValue(attrName, null);
                }
                else
                {
                    value = node.InnerText;
                }

                Page.Data.Add(new Datum { Key = key, Value = value });
            }
        }

        private void BaseHref(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                string value = node.GetAttributeValue("href", null);
                Page.BaseHref = value;
            }
        }

        private void ArticleSections(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;
        }

        private void MediaImages(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                var key = node.Name;

                if (node.HasAttributes && node.Attributes.Contains("src"))
                {
                    var value = node.Attributes["src"].Value;

                    var media = new Media()
                    {
                        Type = "image",
                        Url = value
                    };

                    Page.Media.Add(media);
                }
            }
        }

        private void Icons(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;


            foreach (var node in nodes)
            {
                var key = node.Name;

                if (node.HasAttributes && node.Attributes.Contains("href"))
                {
                    var value = node.Attributes["href"].Value;

                    Page.Icons.Add(value);
                }
            }
        }

        private void PageLanguage(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            Page.Language = nodes.First().Attributes["lang"].Value;

        }


        private void LdJson(HtmlNodeCollection nodes)
        {
            if (nodes == null) return;

            try
            {

                foreach (var node in nodes)
                {
                    var key = node.Name;
                    var value = node.InnerText;

                    var json = JObject.Parse(value);

                    JToken token;

                    if (json.TryGetValue("@type", out token))
                    {
                        var type = token.ToString().ToLower();

                        switch (type)
                        {
                            case "organization":
                                LdJsonOrganization(json);
                                break;
                            case "article":
                            case "newsarticle":
                            case "report":
                            case "scholarlyarticle":
                            case "socialmediaposting":
                            case "techarticle":
                                LdJsonArticle(json);
                                break;
                        }
                        Page.Data.Add(new Datum { Key = key, Value = value });
                    }
                }
            }
            catch { }
        }

        private void Microdata(HtmlNodeCollection nodes)
        {
            var skip = new string[] { "http://schema.org/website" };

            if (nodes == null) return;

            foreach (var node in nodes)
            {
                var itemTypes = node.SelectNodes("./@itemtype");

                if (itemTypes == null) return;

                foreach (var itemType in itemTypes)
                {
                    var attr = itemType.Attributes["itemtype"].Value.ToLower();

                    if (skip.Contains(attr)) continue;

                    var value = itemType.InnerText.Trim();

                    Datum datum = null;

                    switch (attr)
                    {
                        case "http://data-vocabulary.org/breadcrumb":
                            datum = new Datum { Key = "keywords", Value = value };
                            break;
                        case "http://schema.org/webpage":
                            datum = new Datum { Key = "webpage", Value = null };
                            break;
                        case "http://schema.org/article":
                        case "http://schema.org/newsarticle":
                        case "http://schema.org/report":
                        case "http://schema.org/scholarlyarticle":
                        case "http://schema.org/socialmediaposting":
                        case "http://schema.org/techarticle":
                            SchemaArticle(itemType);
                            break;
                        case "http://schema.org/book":
                            SchemaBook(itemType);
                            break;
                        default:
                            datum = new Datum { Key = attr, Value = value };
                            break;
                    }

                    if (datum != null)
                    {
                        Page.Data.Add(datum);
                    }
                }

            }
        }

        private void LdJsonOrganization(JObject json)
        {
            //good example for this one
            //http://www.guardian.co.uk/uk/2007/dec/03/britishidentity.jamesranderson"
        }

        private void LdJsonArticle(JObject json)
        {
            //http://www.engadget.com/2015/07/31/huawei-microsoft-smartphone-sales/
            GetSafeToken(json, "headline");
        }

        private string GetSafeToken(JObject json, string propertyName)
        {
            JToken token;
            string result = null;

            if (json.TryGetValue(propertyName, out token))
            {
                result = token.ToString();
            }

            return result;
        }

        private string GetSafeAttribute(HtmlNode node, string xpath, string attrName = null, string key = null, bool addToPageData = true)
        {
            string result = null;
            var nodes = node.SelectNodes(xpath);

            if (nodes != null)
            {
                var target = nodes.First();
                if (attrName == null)
                {
                    result = target.InnerText.Trim();
                }
                else if (target.Attributes.Contains(attrName))
                {
                    result = target.Attributes[attrName].Value.Trim();
                }
            }

            if (addToPageData && result != null)
            {
                var datum = new Datum()
                {
                    Key = key,
                    Value = result
                };

                Page.Data.Add(datum);
            }

            return result;
        }

        private void SchemaArticle(HtmlNode node)
        {
            string format = "//*[contains(@{0}, '{1}')]";

            GetSafeAttribute(node, string.Format(format, "itemprop", "headline"), null, "title");
            GetSafeAttribute(node, string.Format(format, "itemprop", "name"), null, "title");
            GetSafeAttribute(node, string.Format(format, "itemprop", "datePublished"), "content", "datePublished");
            GetSafeAttribute(node, string.Format(format, "itemprop", "articleBody"), null, "articleBody");
            GetSafeAttribute(node, string.Format(format, "itemprop", "author"), null, "author");

        }

        private void SchemaBook(HtmlNode node)
        {
            string format = "//*[contains(@{0}, '{1}')]";

            GetSafeAttribute(node, string.Format(format, "itemprop", "name"), null, "title");
            GetSafeAttribute(node, string.Format(format, "itemprop", "description"), null, "description");
            GetSafeAttribute(node, string.Format(format, "itemprop", "image"), "src", "image");
        }
    }
}
