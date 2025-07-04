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
            SqlCommand cmd = new SqlCommand()
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
        public int ShareArticle(SharedArticle sharedArticle)
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

        // Gets all shared articles using SP_GetSharedArticles_FP
        public List<SharedArticle> GetSharedArticles()
        {
            using SqlConnection con = connect("myProjDB");
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetSharedArticles_FP", con, null);

            List<SharedArticle> sharedList = new();

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SharedArticle sa = new SharedArticle
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    ArticleUrl = reader["ArticleUrl"].ToString(),
                    Comment = reader["Comment"].ToString()
                };
                sharedList.Add(sa);
            }

            return sharedList;
        }
    }
}
