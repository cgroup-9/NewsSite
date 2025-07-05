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

        // Retrieves all shared articles (optionally filters by hidden user IDs)
        public List<SharedArticleIndex> GetSharedArticles(string? hiddenUserIds = null)
        {
            using SqlConnection con = connect("myProjDB");

            var paramDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(hiddenUserIds))
                paramDic.Add("@HiddenUserIds", hiddenUserIds);

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetAllSharedArticles_FP", con, paramDic.Count > 0 ? paramDic : null);

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
                    Comment = reader["Comment"].ToString()
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

    }
}
