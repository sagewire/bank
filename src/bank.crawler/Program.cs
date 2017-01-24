using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using bank.crawler.services;
using bank.crawler.services.Twitter;
using bank.data.repositories;
using bank.poco;

namespace bank.crawler
{
    class Program
    {
        private static Timer _twitterFriendsTimer = new Timer();
        private static Timer _twitterUserLookupTimer = new Timer();

        static void Main(string[] args)
        {
            StartTimers();
            Console.ReadKey();
        }

        static void StartTimers()
        {
            _twitterFriendsTimer.Elapsed += _twitterFriendsTimer_Elapsed;
            StartTimer(_twitterFriendsTimer);


            _twitterUserLookupTimer.Elapsed += _twitterUserLookupTimer_Elapsed;
            StartTimer(_twitterUserLookupTimer);
        }

        static void StartTimer(Timer timer)
        {
            timer.Interval = 1;
            timer.AutoReset = false;
            timer.Start();
        }

        static void ResetTimer(Timer timer, int interval)
        {
            timer.Interval = interval;
            //timer.AutoReset = true;
            timer.Start();
        }

        private static void _twitterUserLookupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetTimer(_twitterUserLookupTimer, 3000);

            var repo = new TwitterFriendRepository();

            var friends = repo.GetMissingScreenNames();

            if (friends.Any())
            {
                var service = new TwitterService();
                service.Execute(friends);
            }
        }

        private static void _twitterFriendsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetTimer(_twitterFriendsTimer, 60000);

            var orgRepo = new OrganizationRepository();
            var org = orgRepo.NextTwitterFriendUpdate();

            if (org != null)
            {
                org.TwitterFriendUpdate = DateTime.Now;
                orgRepo.Update(org);

                Console.WriteLine("Updating Twitter Friends for {0}", org.Name);
                var service = new TwitterService();

                service.Execute(org.Twitter, org.OrganizationId); ;

            }

        }
    }
}
