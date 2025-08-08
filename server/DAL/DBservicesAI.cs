using System;
using System.Data;
using System.Data.SqlClient;
using server.Models;

namespace server.DAL
{
    public class DBservicesAI
    {
        // ==========================
        // Creates and Opens SQL Connection
        // ==========================
        // Reads connection string "myProjDB" from appsettings.json
        // Returns an open SqlConnection
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
        // Adds all parameters if provided
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 10, // seconds
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
        // GetLoginsRaw
        // ==========================
        // Purpose:
        // - Fetches login statistics for the past X days.
        // - Uses stored procedure: SP_GetLoginsPastWeek_FP
        // Parameters:
        // - daysBack → how many days back to retrieve data.
        // Returns:
        // - List<LoginStats> containing date and login counter per day.
        public List<LoginStats> GetLoginsRaw(int daysBack)
        {
            List<LoginStats> statsList = new List<LoginStats>();

            using (SqlConnection con = connect("myProjDB"))
            {
                var paramDic = new Dictionary<string, object>
                {
                    { "@DaysBack", daysBack }
                };

                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetLoginsPastWeek_FP", con, paramDic);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    LoginStats stat = new LoginStats
                    {
                        Date = Convert.ToDateTime(dr["date"]),
                        LoginCounter = Convert.ToInt32(dr["loginCounter"])
                    };
                    statsList.Add(stat);
                }
            }

            return statsList;
        }

        // ==========================
        // GetSavedArticlesRaw
        // ==========================
        // Purpose:
        // - Fetches saved article statistics for the past X days.
        // - Uses stored procedure: SP_GetDayMaxSaved_FP
        // Parameters:
        // - daysBack → how many days back to retrieve data.
        // Returns:
        // - List<SavedStats> containing the top saved articles (userId, articleId, date saved, category).
        public List<SavedStats> GetSavedArticlesRaw(int daysBack)
        {
            List<SavedStats> savedList = new List<SavedStats>();

            using (SqlConnection con = connect("myProjDB"))
            {
                var paramDic = new Dictionary<string, object>
                {
                    { "@DaysBack", daysBack }
                };

                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetDayMaxSaved_FP", con, paramDic);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    SavedStats saved = new SavedStats
                    {
                        UserId = Convert.ToInt32(dr["userId"]),
                        ArticleId = Convert.ToInt32(dr["articleId"]),
                        DateSaved = Convert.ToDateTime(dr["dateSaved"]),
                        Category = dr["category"].ToString()
                    };
                    savedList.Add(saved);
                }
            }

            return savedList;
        }

        // ==========================
        // GetActivityRaw
        // ==========================
        // Purpose:
        // - Fetches overall activity stats (logins + saved news) for the past X days.
        // - Uses stored procedure: SP_GetActivityData_FP
        // Parameters:
        // - daysBack → how many days back to retrieve data.
        // Returns:
        // - List<ActivityData> containing daily login count and saved news count.
        public List<ActivityData> GetActivityRaw(int daysBack)
        {
            List<ActivityData> activityList = new List<ActivityData>();

            using (SqlConnection con = connect("myProjDB"))
            {
                var paramDic = new Dictionary<string, object>
                {
                    { "@DaysBack", daysBack }
                };

                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_GetActivityData_FP", con, paramDic);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ActivityData act = new ActivityData
                    {
                        Date = Convert.ToDateTime(dr["date"]),
                        LoginCounter = Convert.ToInt32(dr["loginCounter"]),
                        SavedNewsCounter = Convert.ToInt32(dr["savedNewsCounter"])
                    };
                    activityList.Add(act);
                }
            }

            return activityList;
        }
    }
}
