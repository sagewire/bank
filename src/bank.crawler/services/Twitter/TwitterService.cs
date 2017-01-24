using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.data.repositories;
using bank.poco;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace bank.crawler.services.Twitter
{
    public class TwitterService
    {
        private static string _key = Settings.TwitterKey;// "mbKExALtVsoyTk9XT5FHlIXnO";
        private static string _secret = Settings.TwitterSecret;// "vWYpp68OCCobCHydQWpJ3H7sbWTpCNQJ4AHwZwtMqUyIVe72JH";
        private string _access_token = null;

        public void Execute(string screenName, int organizationId)
        {
            Authorize();
            GetFriends(screenName, organizationId);
        }

        public void Execute(List<TwitterFriend> friends)
        {
            Authorize();
            GetScreenNames(friends);
        }

        private void Authorize()
        {
            if (!string.IsNullOrWhiteSpace(_access_token))
            {
                return;
            }

            string url = "https://api.twitter.com";
            var uri = new Uri(url);
            var client = new RestClient(uri);

            var request = new RestRequest("/oauth2/token");
            request.Method = Method.POST;
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
            request.AddParameter("grant_type", "client_credentials");

            var plainText = string.Format("{0}:{1}", _key, _secret);
            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            var base64 = System.Convert.ToBase64String(bytes);

            request.AddHeader("Authorization", string.Format("Basic {0}", base64));

            var response = client.Post(request);

            //var results = new ServiceResult(this.ServiceType);

            dynamic json = JObject.Parse(response.Content);

            _access_token = json.access_token.Value;
        }

        private void GetScreenNames(List<TwitterFriend> friends)
        {
            
            var ids = friends.Select(x => x.TwitterId).Distinct().ToList();

            string url = "https://api.twitter.com/1.1";

            var client = new RestClient(url);

            var request = new RestRequest("users/lookup.json");

            request.Method = Method.POST;
            request.AddParameter("user_id", string.Join(",", ids));
            request.AddParameter("count", "5000");

            request.AddHeader("Authorization", string.Format("Bearer {0}", _access_token));

            var response = client.Execute<List<TwitterFriend>>(request);

            foreach(var friend in friends)
            {
                var result = response.Data.SingleOrDefault(x => x.Id.ToString() == friend.TwitterId);

                if (result != null)
                {
                    friend.ScreenName = result.ScreenName;
                    Repository<TwitterFriend>.New().Save(friend);
                }
            }

        }

        private void GetFriends(string screenName, int orgId)
        {
            string url = "https://api.twitter.com/1.1";

            var client = new RestClient(url);

            var request = new RestRequest("friends/ids.json");

            request.Method = Method.GET;
            
            request.AddQueryParameter("screen_name", screenName);
            request.AddQueryParameter("count", "5000");

            request.AddHeader("Authorization", string.Format("Bearer {0}", _access_token));


            var response = client.Execute<TwitterFriendIds>(request);

            foreach(var id in response.Data.ids)
            {
                var twitterFriend = new TwitterFriend
                {
                    OrganizationId = orgId,
                    TwitterId = id.ToString()
                };

                Repository<TwitterFriend>.New().Save(twitterFriend);

            }

            //foreach(var user in response.Data.Users)
            //{
            //    var twitterFriend = new TwitterFriend
            //    {
            //        OrganizationId = orgId,
            //        ScreenName = user.ScreenName,
            //        TwitterId = user.Id
            //    };

            //    Repository<TwitterFriend>.New().Save(twitterFriend);

            //}

            //var response = base.RestCache(client, request);

            //var results = new ServiceResult(this.ServiceType);

            //dynamic json = JObject.Parse(response.Content);
        }

        private void SearchById(string id)
        {
            string url = "https://api.twitter.com/1.1";

            var client = new RestClient(url);

            var request = new RestRequest("statuses/show.json");

            request.Method = Method.GET;
            request.AddQueryParameter("id", id);
            request.AddQueryParameter("include_entities", "1");

            request.AddHeader("Authorization", string.Format("Bearer {0}", _access_token));


            var response = client.Execute(request);

            //var response = base.RestCache(client, request);

            //var results = new ServiceResult(this.ServiceType);

            dynamic json = JObject.Parse(response.Content);

            //var card = ConvertToCard(json);

            //if (card != null)
            //{
            //    results.AddCard(card);
            //}

            //return results;
        }
    }

    internal class TwitterFriendIds
    {
        public List<long> ids { get; set; }
        
    }
}
