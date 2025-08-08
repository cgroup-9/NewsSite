using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace server.DAL
{
    public class DBservicesArticles
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
        // Sets CommandType to StoredProcedure
        // Adds each parameter to the SqlCommand if provided
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 10, // seconds before timeout
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
        // Increments the API Fetch Counter in the Admin_FP table
        // ==========================
        // Calls stored procedure: SP_IncrementApiFetchCounter
        // Purpose:
        // - In the admin panel, this counter tracks how many times the server fetched articles from the external API.
        // - This method simply increments that counter by 1 each time it runs.
        public void IncrementApiFetchCounter()
        {
            SqlConnection con = connect("myProjDB");

            // No parameters are required for this stored procedure
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_IncrementApiFetchCounter", con, null);

            try
            {
                cmd.ExecuteNonQuery(); // Executes the SP (no return value expected)
            }
            catch (Exception ex)
            {
                // Wrap and rethrow for more descriptive error message
                throw new Exception("❌ Failed to update apiFetchCounter: " + ex.Message);
            }
            finally
            {
                con.Close(); // Always close connection
            }
        }
    }
}
