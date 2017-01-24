using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    public class TwitterFriendMap : ClassMapper<TwitterFriend>
    {
        public TwitterFriendMap()
        {
            Map(x => x.Id).Key(KeyType.Identity);
            Map(x => x.ScreenName).Key(KeyType.NotAKey);
            AutoMap();
        }
    }
}
