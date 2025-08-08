using System.Data.SqlClient;
using System.Data;
using server.Models;
using Microsoft.Extensions.Configuration;

namespace server.DAL
{
    public class DBservicesSharedArticles
    {
        // ==========================
        // Establishes a SQL connection
        // ==========================
        // Reads the connection string from appsettings.json
        // Creates a new SqlConnection object
        // Opens the connection and returns it
        private SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            string cStr = configuration.GetConnectionString(conString);
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

        // ==========================
        // Creates a SqlCommand for a Stored Procedure
        // ==========================
        // spName   → stored procedure name
        // con      → open SqlConnection
        // paramDic → dictionary of parameters (name → value)
        // Sets the CommandType to StoredProcedure
        // Loops through all parameters and adds them to the command
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object>? paramDic)
        {
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 10, // seconds before timeout
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

        // ==========================
        // Shares an article
        // ==========================
        // Calls stored procedure SP_ShareArticle_FP
        // Passes userId, articleUrl, and comment
        // Also defines a RETURN VALUE parameter to get result code from DB
        // Common result codes: 1 = success, other values = errors (e.g., already shared)
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
                Direction = ParameterDirection.ReturnValue // Tells SQL Server to store RETURN VALUE here
            };
            cmd.Parameters.Add(returnParameter);

            cmd.ExecuteNonQuery();
            return (int)returnParameter.Value;
        }

        // ==========================
        // Retrieves a paginated list of shared articles
        // ==========================
        // Supports filtering by:
        // - HiddenUserIds (articles from these users will be excluded)
        // - CurrentUserId (can be used to mark likes, etc.)
        // Uses stored procedure SP_GetAllSharedArticles_FP
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

            // Reads each row from the DB and maps it into a SharedArticleIndex object
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

        // ==========================
        // Reports a shared article
        // ==========================
        // Calls stored procedure SP_ReportSharedArticle_FP
        // Passes the ID of the reporter and the ID of the shared article
        // Uses RETURN VALUE parameter to get result code
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

        // ==========================
        // Retrieves all reported shared articles (for admin panel)
        // ==========================
        // Uses stored procedure SP_GetReportedSharedArticles_FP
        // Returns a list of ReportedCommentsAdminPanel objects
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

        // ==========================
        // Likes a shared article
        // ==========================
        // Inserts a record into SharedArticleLikes_FP
        // If the (SharedId, UserId) pair already exists, SQL throws a duplicate key error
        public async Task<bool> LikeSharedArticleAsync(int sharedId, int userId)
        {
            SqlConnection con = connect("myProjDB");
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
                if (ex.Number == 2627) // Primary key violation = already liked
                    throw new Exception("You already liked this article.");

                throw;
            }
            finally
            {
                con.Close(); // Always close connection manually here
            }
        }

        // ==========================
        // Removes a like from a shared article
        // ==========================
        // Deletes the (SharedId, UserId) record from SharedArticleLikes_FP
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
