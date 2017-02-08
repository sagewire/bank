using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.poco
{
    public class UserFavorite
    {
        public int OrganizationId { get; set; }
        public string UserId { get; set; }
        public FavoriteTypes FavoriteType { get; set; }
        public int Visits { get; set; }
        public DateTime LastVisited { get; set; } = DateTime.Now;
    }

    public enum FavoriteTypes
    {
        None = 0,
        User = 100,
        Visit = 20
    }
}
