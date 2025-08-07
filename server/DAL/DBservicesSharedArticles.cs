using System.Data.SqlClient;
using System.Data;
using server.Models;
using Microsoft.Extensions.Configuration;

namespace server.DAL
{
    public class DBservicesSharedArticles
    {
        // Establishes a connection to the database using the connection string from appsettings.json
        private SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            string cStr = configuration.GetConnectionString(conString);
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

        // Builds a SqlCommand for a stored procedure with optional parameters
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object>? paramDic)
        {
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 10,
                CommandType = CommandType.StoredProcedure
            };

            if (paramDic != null)
            {
                foreach (var param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return cmd;
        }

        // Inserts a shared article using SP_ShareArticle_FP
        public int ShareArticle(SharedArticleRequest sharedArticle)
        {
            using SqlConnection con = connect("myProjDB");
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_ShareArticle_FP", con, new Dictionary<string, object>
            {
                { "@userId", sharedArticle.UserId },
                { "@articleUrl", sharedArticle.ArticleUrl },
                { "@comment", sharedArticle.Comment }
            });

            SqlParameter returnParameter = new SqlParameter
            {
                Direction = ParameterDirection.ReturnValue
            };
            cmd.Parameters.Add(returnParameter);

            cmd.ExecuteNonQuery();
            return (int)returnParameter.Value;
        }

        public List<SharedArticleIndex> GetSharedArticles(string? hiddenUserIds = null, int page = 1, int pageSize = 20, int? currentUserId = null)
        {
            using SqlConnection con = connect("myProjDB");

            var paramDic = new Dictionary<string, object>
            {
                { "@Page", page },
                { "@PageSize", pageSize }
            };

            if (!string.IsNullOrEmpty(hiddenUserIds))
                paramDic.Add("@HiddenUserIds", hiddenUserIds);

            if (currentUserId.HasValue)
                paramDic.Add("@CurrentUserId", currentUserId.Value);

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetAllSharedArticles_FP", con, paramDic);

            List<SharedArticleIndex> sharedList = new();

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SharedArticleIndex sa = new SharedArticleIndex
                {
                    SharedId = Convert.ToInt32(reader["SharedId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    UserName = reader["UserName"].ToString(),
                    ArticleUrl = reader["ArticleUrl"].ToString(),
                    Title = reader["Title"].ToString(),
                    UrlToImage = reader["UrlToImage"].ToString(),
                    Author = reader["Author"].ToString(),
                    Comment = reader["Comment"].ToString(),
                    DateShared = Convert.ToDateTime(reader["DateShared"]),
                    LikesCount = Convert.ToInt32(reader["LikesCount"]),
                    AlreadyLiked = Convert.ToBoolean(reader["AlreadyLiked"])
                };
                sharedList.Add(sa);
            }

            return sharedList;
        }


        // Reports a shared article using SP_ReportSharedArticle_FP
        public int ReportSharedArticle(ReportSharedArticleRequest reportRequest)
        {
            using SqlConnection con = connect("myProjDB");
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_ReportSharedArticle_FP", con, new Dictionary<string, object>
    {
        { "@ReporterUserId", reportRequest.ReporterUserId },
        { "@SharedArticleId", reportRequest.SharedArticleId }
    });

            SqlParameter returnParameter = new SqlParameter
            {
                Direction = ParameterDirection.ReturnValue
            };
            cmd.Parameters.Add(returnParameter);

            cmd.ExecuteNonQuery();
            return (int)returnParameter.Value;
        }
        public List<ReportedCommentsAdminPanel> GetReportedComments()
        {
            using SqlConnection con = connect("myProjDB");
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetReportedSharedArticles_FP", con, null);

            List<ReportedCommentsAdminPanel> list = new();

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ReportedCommentsAdminPanel item = new ReportedCommentsAdminPanel
                {
                    ReportId = Convert.ToInt32(reader["ReportId"]),
                    Comment = reader["Comment"].ToString(),
                    SharedByUserName = reader["SharedByUserName"].ToString(),
                    SharedByUserId = Convert.ToInt32(reader["SharedByUserId"]),
                    ReportDate = Convert.ToDateTime(reader["ReportDate"])
                };
                list.Add(item);
            }

            return list;
        }

        public async Task<bool> LikeSharedArticleAsync(int sharedId, int userId)
        {
            SqlConnection con = connect("myProjDB"); // כבר נפתח בפנים
            SqlCommand cmd = new SqlCommand("INSERT INTO SharedArticleLikes_FP (SharedId, UserId) VALUES (@SharedId, @UserId)", con);
            cmd.Parameters.AddWithValue("@SharedId", sharedId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Violation of PRIMARY KEY (duplicate like)
                    throw new Exception("You already liked this article.");

                throw; // any other error
            }
            finally
            {
                con.Close(); // נסגור בסוף ידנית
            }
        }

        public async Task<bool> UnlikeSharedArticleAsync(int sharedId, int userId)
        {
            using SqlConnection con = connect("myProjDB");
            SqlCommand cmd = new SqlCommand("DELETE FROM SharedArticleLikes_FP WHERE SharedId = @SharedId AND UserId = @UserId", con);
            cmd.Parameters.AddWithValue("@SharedId", sharedId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
