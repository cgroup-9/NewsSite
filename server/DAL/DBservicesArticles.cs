using System;
using System.Data;
using System.Data.SqlClient;
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
                { "@articleUrl", save.ArticleUrl },
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
    }
}
