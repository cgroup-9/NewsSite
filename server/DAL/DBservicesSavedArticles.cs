using System.Data.SqlClient;
using System.Data;
using server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace server.DAL
{
    public class DBservicesSavedArticles
    {
        // Establishes a connection to the database using the connection string from appsettings.json
        public SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            string cStr = configuration.GetConnectionString("myProjDB");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

        // Builds a SqlCommand for a stored procedure with optional parameters
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object> paramDic)
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
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return cmd;
        }

        // Saves an article using SP_SaveArticle_FP
        public int SaveArticle(SaveArticleRequest save)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = connect("myProjDB");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@userId", save.UserId },
                { "@articleUrl", save.ArticleUrl ?? "" },
                { "@title", save.Title ?? "" },
                { "@description", save.Description ?? "" },
                { "@urlToImage", save.UrlToImage ?? "" },
                { "@author", save.Author ?? "" },
                { "@publishedAt", save.PublishedAt ?? "" },
                { "@content", save.Content ?? "" },
                { "@category", save.Category ?? "" }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_SaveArticle_FP", con, paramDic);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery();
                int result = (int)returnParameter.Value;
                return result;
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // Deletes a saved article using SP_DeleteSavedArticle_FP
        public int DeleteSavedArticle(int userId, string articleUrl)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = connect("myProjDB");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@userId", userId },
                { "@articleUrl", articleUrl ?? "" } // fallback to empty string if null
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_DeleteSavedArticle_FP", con, paramDic);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery();
                int result = (int)returnParameter.Value;
                return result;
            }
            catch (SqlException ex)
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // Gets all saved articles for a specific user
        public List<SaveArticleRequest> GetSavedArticles(int userId, int page, int pageSize)
        {
            using var con = connect("myProjDB");

            var paramDic = new Dictionary<string, object>
            {
                { "@userId", userId },
                { "@page", page },
                { "@pageSize", pageSize }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetSavedArticles_FP", con, paramDic);

            var articles = new List<SaveArticleRequest>();
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                articles.Add(new SaveArticleRequest
                {
                    UserId = Convert.ToInt32(reader["userId"]),
                    ArticleUrl = SafeGetString(reader, "articleUrl"),
                    Title = SafeGetString(reader, "title"),
                    Description = SafeGetString(reader, "description"),
                    UrlToImage = SafeGetString(reader, "urlToImage"),
                    Author = SafeGetString(reader, "author"),
                    PublishedAt = SafeGetString(reader, "publishedAt"),
                    Content = SafeGetString(reader, "content"),
                    Category = SafeGetString(reader, "category")
                });
            }

            return articles;
        }

        // Helper function to safely read string values from a SQL result row.
        // In SQL Server, if a column contains NULL, accessing it with .ToString()
        // will throw an exception (because DBNull cannot be cast to string).
        // This method checks if the value is DBNull:
        // - If it's NULL (DBNull.Value), it returns an empty string "".
        // - Otherwise, it returns the string value of the column.
        //
        // This avoids runtime errors and makes code cleaner and safer.
        private string SafeGetString(SqlDataReader r, string col) =>
            r[col] == DBNull.Value ? "" : r[col].ToString();
    }
}
