using System.Data.SqlClient;
using System.Data;
using server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace server.DAL
{
    public class DBservicesSavedArticles
    {
        // ==========================
        // Connects to SQL Server
        // ==========================
        // Reads the connection string "myProjDB" from appsettings.json
        // Creates and opens a SqlConnection, then returns it
        public SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            string cStr = configuration.GetConnectionString("myProjDB");
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
        // Adds each parameter from paramDic to the SqlCommand
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 10, // seconds before timing out
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

        // ==========================
        // Saves an article to the DB
        // ==========================
        // Uses stored procedure: SP_SaveArticle_FP
        // Parameters include userId, URL, title, description, image URL, author, date, content, category
        // Also defines a RETURN VALUE parameter to get result code from DB
        // Common results: 1 = success, 0 = already saved
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
                throw ex; // If connection fails, throw error to caller
            }

            // Prepare parameters for stored procedure
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

            // Add return value parameter
            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // Run the stored procedure
                int result = (int)returnParameter.Value;
                return result;
            }
            catch (SqlException)
            {
                throw;
            }
            finally
            {
                con.Close(); // Always close connection
            }
        }

        // ==========================
        // Deletes a saved article
        // ==========================
        // Uses stored procedure: SP_DeleteSavedArticle_FP
        // Takes userId and articleUrl as parameters
        // Also uses RETURN VALUE parameter to indicate result (1 = deleted, 0 = not found)
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
                { "@articleUrl", articleUrl ?? "" } // Fallback to empty string if null
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
            catch (SqlException)
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // ==========================
        // Retrieves saved articles
        // ==========================
        // Uses stored procedure: SP_GetSavedArticles_FP
        // Supports:
        // - Pagination (page, pageSize)
        // - Filtering by categories
        // - Searching by title/content
        public List<SaveArticleRequest> GetSavedArticles(
            int userId, int page, int pageSize, string? categories = null, string? searchTerm = null)
        {
            using var con = connect("myProjDB");

            var paramDic = new Dictionary<string, object>
            {
                { "@userId", userId },
                { "@page", page },
                { "@pageSize", pageSize },
                { "@categories", string.IsNullOrWhiteSpace(categories) ? (object)DBNull.Value : categories },
                { "@searchTerm", string.IsNullOrWhiteSpace(searchTerm) ? (object)DBNull.Value : searchTerm }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetSavedArticles_FP", con, paramDic);

            var articles = new List<SaveArticleRequest>();

            // Read each record from SQL and map it to a SaveArticleRequest object
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

        // ==========================
        // Helper: Safely gets string value from a SqlDataReader column
        // ==========================
        // Why needed:
        // - In SQL Server, NULL values are represented as DBNull.Value
        // - Trying to cast DBNull.Value to string will cause an exception
        // Behavior:
        // - If column is NULL → returns empty string ""
        // - Else → returns the string value
        // This avoids runtime errors and makes code cleaner and safer.

        // DBNull.Value represents a NULL from the database (SQL). It's not the same as C# null — 
        // it's a special object used to indicate that a column has no value in the database.
        private string SafeGetString(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
            {
                return "";
            }
            else
            {
                return reader[columnName].ToString();
            }
        }
    }
}
