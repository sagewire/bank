using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using Dapper;

namespace bank.data.repositories
{
    public class UserFavoriteRepository : BaseRepository<UserFavorite>
    {
        public override UserFavorite Get(UserFavorite model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var obj = conn.QuerySingleOrDefault<UserFavorite>("  select  * " +
                                                "from   UserFavorite " +
                                                "where OrganizationID = @OrganizationID " +
                                                "   and UserId = @UserId ", new
                                                {
                                                    OrganizationID = model.OrganizationId,
                                                    UserId = model.UserId
                                                },
                                                commandType: CommandType.Text);

                return obj;

            }
        }

        public override void Delete(UserFavorite model)
        {
            var sql = new StringBuilder();
            sql.AppendLine("delete from UserFavorite");
            sql.AppendLine("where OrganizationID = @OrganizationID and UserId = @UserId");

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var obj = conn.Execute(sql.ToString(),
                            new
                            {
                                OrganizationID = model.OrganizationId,
                                UserId = model.UserId
                            },
                            commandType: CommandType.Text);
            }
        }

        public override void Save(UserFavorite model)
        {
            var existing = Get(model);

            if (existing != null)
            {
                model.Visits = Math.Max(existing.Visits, model.Visits) + 1;
                model.FavoriteType = (FavoriteTypes)Math.Max((int)model.FavoriteType, (int)existing.FavoriteType);
                model.LastVisited = DateTime.Now;

                Update(model);
            }
            else
            {
                Insert(model);
            }
        }


        public override void Update(UserFavorite model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var reportItems = conn.Execute(" update UserFavorite " +
                                                "set FavoriteType = @FavoriteType, Visits = @Visits, LastVisited = @LastVisited " +
                                                "where OrganizationID = @OrganizationID and UserId = @UserId",
                                                new
                                                {
                                                    OrganizationID = model.OrganizationId,
                                                    FavoriteType = model.FavoriteType,
                                                    LastVisited = model.LastVisited,
                                                    UserId = model.UserId,
                                                    Visits = model.Visits

                                                },
                                                commandType: CommandType.Text);


            }
        }

        public IList<Organization> GetFavorites(string userId, FavoriteTypes type, int take = 10)
        {
            var sb = new StringBuilder();
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                sb.AppendLine("select * from Organization where OrganizationID in ( ");
                sb.AppendLine("select  top (@top) OrganizationID ");
                sb.AppendLine("from   UserFavorite ");
                sb.AppendLine("where UserId = @UserID and FavoriteType = @FavoriteType order by LastVisited desc ) ");

                var obj = conn.Query<Organization>(sb.ToString(),
                    new
                    {
                        UserId = userId,
                        FavoriteType = type,
                        Top = take
                    },
                    commandType: CommandType.Text);

                return obj.ToList();

            }
        }
    }
}
