using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml.Linq;
using server.Models;

namespace server.DAL
{
    public class DBservicesAI
    {
        // Create and open a SQL connection using connection string from appsettings.json
        public SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            string cStr = configuration.GetConnectionString("myProjDB");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

        // Helper method to create a SqlCommand configured for a stored procedure with given parameters
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

        // This function returns login data for the past X days from Admin_FP table- for Q1+Q2
        public List<LoginStats> GetLoginsRaw(int daysBack)
        {
            List<LoginStats> statsList = new List<LoginStats>();

            using (SqlConnection con = connect("myProjDB"))
            {
                Dictionary<string, object> paramDic = new Dictionary<string, object>();
                paramDic.Add("@DaysBack", daysBack);

                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetLoginsPastWeek_FP", con, paramDic);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    LoginStats stat = new LoginStats();
                    stat.Date = Convert.ToDateTime(dr["date"]);
                    stat.LoginCounter = Convert.ToInt32(dr["loginCounter"]);
                    statsList.Add(stat);
                }
            }

            return statsList;
        }

        // This function returns Saving Article Data for the past X days from Admin_FP table- for Q3+Q4

        public List<SavedStats> GetSavedArticlesRaw(int daysBack)
        {
            List<SavedStats> savedList = new List<SavedStats>();

            using (SqlConnection con = connect("myProjDB"))
            {
                Dictionary<string, object> paramDic = new Dictionary<string, object>();
                paramDic.Add("@DaysBack", daysBack);

                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetDayMaxSaved_FP", con, paramDic);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    SavedStats saved = new SavedStats();
                    saved.UserId = Convert.ToInt32(dr["userId"]);
                    saved.ArticleId = Convert.ToInt32(dr["articleId"]);
                    saved.DateSaved = Convert.ToDateTime(dr["dateSaved"]);
                    saved.Category = dr["category"].ToString();
                    savedList.Add(saved);
                }
            }

            return savedList;
        }

        public List<ActivityData> GetActivityRaw(int daysBack)
        {
            List<ActivityData> activityList = new List<ActivityData>();

            using (SqlConnection con = connect("myProjDB"))
            {
                Dictionary<string, object> paramDic = new Dictionary<string, object>();
                paramDic.Add("@DaysBack", daysBack);

                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetActivityData_FP", con, paramDic);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ActivityData act = new ActivityData();
                    act.Date = Convert.ToDateTime(dr["date"]);
                    act.LoginCounter = Convert.ToInt32(dr["loginCounter"]);
                    act.SavedNewsCounter = Convert.ToInt32(dr["savedNewsCounter"]);
                    activityList.Add(act);
                }
            }

            return activityList;
        }
    }
}
