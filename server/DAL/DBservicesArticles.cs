using System.Data.SqlClient;
using System.Data;
﻿using System;
using System.Xml.Linq;
using server.Models;

namespace server.DAL
{
    public class DBservicesArticles
    {
        public SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            string cStr = configuration.GetConnectionString("myProjDB");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

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

        public void IncrementApiFetchCounter()
        {
            SqlConnection con = connect("myProjDB");
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_IncrementApiFetchCounter", con, null);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to update apiFetchCounter: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        public int saveArticle(SavedArticle save)
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
                { "@articleUrl", save.ArticleUrl ?? ""},
                { "@title", save.Title ?? ""},
                { "@description", save.Description ?? ""},
                { "@urlToImage", save.UrlToImage ?? ""},
                { "@author", save.Author ?? ""},
                { "@publishedAt", save.PublishedAt ?? ""},
                { "@content", save.Content ?? ""},
                { "@category", save.Category ?? ""}
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

        private string SafeGetString(SqlDataReader r, string col) =>
            r[col] == DBNull.Value ? "" : r[col].ToString();

        public List<SavedArticle> GetSavedArticles(int userId)
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
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_GetSavedArticles_FP", con, paramDic);

            List<SavedArticle> articles = new List<SavedArticle>();

            SqlDataReader reader = cmd.ExecuteReader();

            while(reader.Read())
            {
                SavedArticle s = new SavedArticle
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
                };
                articles.Add(s);
            }

            con.Close();
            return articles;

        }
    }
}


