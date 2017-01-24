using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using Newtonsoft.Json;
using RestSharp;

namespace bank.import.crawl
{
    class Facebook
    {
        private TaskPool<Organization> _taskPool = new TaskPool<Organization>();
        private static string _auth = string.Format("{0}|{1}", Settings.FacebookImportAppId, Settings.FacebookImportAppSecret);
        public delegate void TaskCompletedHandler(Organization org);
        public event TaskCompletedHandler TaskCompleted;

        public void Start()
        {
            _taskPool.MaxWorkers = 4;
            _taskPool.NextTask += _taskPool_NextTask;

        }

        public void Enqueue(Organization org)
        {
            _taskPool.Enqueue(org);
        }

        private void _taskPool_NextTask(Organization org)
        {
            try {
                Console.WriteLine("Facebook task {0}", org.Facebook);
                var banner = GetProfileBanner(org);
                var pic = GetProfilePicture(org);

                org.Avatar = pic;
                org.ProfileBanner = banner;

                if (TaskCompleted != null)
                {
                    TaskCompleted(org);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("{0} {1}", org.OrganizationId, ex);
            }
        }

        private string GetProfilePicture(Organization org)
        {
            var client = new RestClient("https://graph.facebook.com/");
            var request = new RestRequest(string.Format("/v2.8/{0}/picture", org.Facebook), Method.GET);
            request.AddParameter("type", "large");
            request.AddParameter("redirect", "false");
            request.AddParameter("access_token", _auth);
            
            var response = client.Execute(request);

            dynamic json = JsonConvert.DeserializeObject(response.Content);
            
            if (json.data != null && json.data.is_silhouette != null)
            {
                var is_silhouette = (bool)json.data.is_silhouette.Value;

                if (!is_silhouette)
                {
                    return json.data.url;
                }
            }
            
            return null;

        }

        private string GetProfileBanner(Organization org)
        {
            var client = new RestClient("https://graph.facebook.com/");
            var request = new RestRequest(string.Format("/v2.8/{0}", org.Facebook), Method.GET);
            request.AddParameter("fields", "cover");
            request.AddParameter("access_token", _auth);

            var response = client.Execute(request);

            dynamic json = JsonConvert.DeserializeObject(response.Content);

            if (json.cover != null && json.cover.source != null)
            {
                return json.cover.source.Value;
            }
            return null;
        }

        public static void Auth(string token)
        {

            var client = new RestClient("https://www.facebook.com/");
            var request = new RestRequest("/v2.8/dialog/oauth", Method.GET);
            request.AddParameter("client_id", Settings.FacebookImportAppId);
            request.AddParameter("redirect_uri", "https://www.facebook.com/connect/login_success.html");


            var response = client.Execute(request);

            //var client = new RestClient("https://graph.facebook.com/");
            //var request = new RestRequest("/oauth/access_token", Method.GET);
            //request.AddParameter("grant_type", "fb_exchange_token");
            //request.AddParameter("client_id", Settings.FacebookImportAppId);
            //request.AddParameter("client_secret", Settings.FacebookImportAppSecret);
            //request.AddParameter("fb_exchange_token", token);

            //var response = client.Execute(request);
        }
    }

}
