using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using RestSharp.Extensions.MonoHttp;

namespace bank.import.crawl
{
    public class Web
    {
        private TaskPool<WebJob> _taskPool = new TaskPool<WebJob>();
        public delegate void TaskCompletedHandler(WebJob job);
        public event TaskCompletedHandler TaskCompleted;

        public void Start()
        {
            _taskPool.MaxWorkers = 12;
            _taskPool.NextTask += _taskPool_NextTask;
        }

        public void Enqueue(WebJob job)
        {
            _taskPool.Enqueue(job);
        }

        private void _taskPool_NextTask(WebJob job)
        {
            try {
                Console.WriteLine("Web task {0}", job.Url);

                job.Url = FixUrl(job.Url);

                if (job.Url != null)
                {
                    Crawler crawler = new Crawler(job.Url);
                    var page = Crawler.Get(job.Url);

                    if (page != null && page.HttpStatusCode == 200)
                    {
                        job.Organization.VerifiedUrl = page.ResponseUrl.AbsoluteUri.Replace(page.ResponseUrl.PathAndQuery, "/");
                        job.Organization.Twitter = page.TwitterSite;
                        job.Organization.Facebook = page.Facebook;
                    }

                }

                if (TaskCompleted != null)
                {
                    TaskCompleted(job);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("{0} {1}", job.Organization.OrganizationId, ex);
            }
        }

        private string FixUrl(string url)
        {

            url = HttpUtility.HtmlDecode(url);

            try
            {
                var builder = new UriBuilder(url);
                if (Uri.IsWellFormedUriString(builder.ToString(), UriKind.Absolute))
                {
                    return builder.ToString();
                }

            }
            catch
            {
            }

            return null;
        }
    }

    public class WebJob
    {
        public Organization Organization { get; set;}
        public string Url { get; set; }
    }
}
