using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using bank.poco;

namespace bank.data.repositories
{
    public class TwitterFriendRepository : BaseRepository<TwitterFriend>
    {
        public override TwitterFriend Get(TwitterFriend model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var fact = conn.QuerySingleOrDefault<TwitterFriend>("  select  * " +
                                                "from   TwitterFriend " +
                                                "where OrganizationID = @OrganizationID " +
                                                "   and TwitterId = @TwitterId ", new
                                                {
                                                    OrganizationID = model.OrganizationId,
                                                    TwitterId = model.TwitterId
                                                },
                                                commandType: CommandType.Text);

                return fact;

            }
        }

        public List<TwitterFriend> GetMissingScreenNames()
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var friends = conn.Query<TwitterFriend>("  select top 100  * " +
                                                "from   TwitterFriend " +
                                                "where  ScreenName is null",
                                                commandType: CommandType.Text)
                                                .ToList();

                return friends;

            }
        }

        public override void Save(TwitterFriend model)
        {
            var existing = Get(model);

            if (existing != null)
            {
                model.Id = existing.Id;
                Update(model);
            }
            else
            {
                Insert(model);
            }
        }
    }
}
