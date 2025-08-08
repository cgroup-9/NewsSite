using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml.Linq;
using server.Models;

namespace server.DAL
{
    public class DBservicesUser
    {
        // ==============================
        // 1) CONNECTING TO THE DATABASE
        // ==============================

        // This method creates and opens a SQL Server connection using the connection string from appsettings.json.
        // Steps:
        //  1. Read appsettings.json (contains connection strings and config values).
        //  2. Retrieve the "myProjDB" connection string.
        //  3. Create a SqlConnection object with that string.
        //  4. Open the connection and return it.
        public SqlConnection connect(string conString)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build(); // Load appsettings.json file

            string cStr = configuration.GetConnectionString("myProjDB"); // Get connection string
            SqlConnection con = new SqlConnection(cStr); // Create connection object
            con.Open(); // Open DB connection
            return con; // Return ready-to-use connection
        }

        // ==================================================
        // 2) GENERIC STORED PROCEDURE COMMAND CREATION HELPER
        // ==================================================

        // This helper creates a SqlCommand that will execute a stored procedure.
        // Parameters:
        //  spName  = the stored procedure name.
        //  con     = the open SqlConnection.
        //  paramDic = dictionary of parameter name/value pairs to pass to the procedure.
        // Why we use this: It saves repetitive code for every DB method.
        private SqlCommand CreateCommandWithStoredProcedureGeneral(string spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = con,                // The DB connection this command will use
                CommandText = spName,            // Stored procedure name
                CommandTimeout = 10,             // Max execution time before timeout (seconds)
                CommandType = CommandType.StoredProcedure // Important: tells SQL Server this is a stored procedure
            };

            // If there are parameters, add them to the command
            if (paramDic != null)
            {
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value); // Adds parameter to SQL command
                }
            }

            return cmd; // Return the configured SqlCommand
        }

        // ===========================================
        // 3) INSERTING A NEW USER INTO THE DATABASE
        // ===========================================

        // Inserts a new user using the stored procedure SP_InsertUser_FP.
        // Returns:
        //  0 = success
        //  3 = duplicate email or username
        public int InsertUser(Users user)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = connect("myProjDB"); // Open connection
            }
            catch (Exception ex)
            {
                throw ex; // If connection fails, throw the exception
            }

            // Prepare parameters to pass to the stored procedure
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@name", user.Name },
                { "@password", user.Password },
                { "@email", user.Email }
            };

            // Create the command to run SP_InsertUser_FP with these parameters
            cmd = CreateCommandWithStoredProcedureGeneral("SP_InsertUser_FP", con, paramDic);

            try
            {
                cmd.ExecuteNonQuery(); // Execute without expecting rows returned
                return 0; // Success
            }
            catch (SqlException ex)
            {
                // SQL error codes 2627 and 2601 mean "unique constraint violation" (duplicate entry)
                if (ex.Number == 2627 || ex.Number == 2601)
                    return 3;
                throw; // If other SQL error, rethrow
            }
            finally
            {
                con.Close(); // Always close DB connection
            }
        }

        // ==========================
        // 4) USER LOGIN AUTHENTICATION
        // ==========================

        // Attempts to log in a user using their email and password.
        // If credentials are correct, returns a Users object; otherwise returns null.
        public Users? LoginUser(string email, string password)
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

            // Parameters for login stored procedure
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@email", email },
                { "@password", password }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_LoginUser_FP", con, paramDic);

            Users? u = null; // Default to null (meaning login failed)

            try
            {
                SqlDataReader reader = cmd.ExecuteReader(); // Execute and read results

                if (reader.Read()) // If a row exists => credentials are correct
                {
                    // Create a Users object from DB data
                    u = new Users
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Active = Convert.ToBoolean(reader["Active"])
                    };
                }

                return u;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }

        // =====================
        // 5) UPDATE USER DETAILS
        // =====================

        // Updates an existing user's name, email, and password.
        // Uses stored procedure SP_UpdateUser_FP.
        // Returns:
        //  1 = success
        //  0 = user not found
        //  3 = duplicate email/username
        public int UpdateUser(Users user)
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

            // Prepare parameters
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@id", user.Id },
                { "@name", user.Name },
                { "@password", user.Password },
                { "@email", user.Email }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_UpdateUser_FP", con, paramDic);

            // Output parameter to receive return value from stored procedure
            SqlParameter returnValue = new SqlParameter("@ReturnVal", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnValue);

            try
            {
                cmd.ExecuteNonQuery();
                return (int)returnValue.Value;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    return 3;
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // ====================================
        // 6) UPDATE USER ACTIVE/INACTIVE STATUS
        // ====================================

        // Changes a user's "Active" status.
        // Returns:
        //  1 = success
        //  0 = user not found
        public int UpdateUserStatus(int id, bool active)
        {
            SqlConnection con = connect("myProjDB");
            Dictionary<string, object> paramDic = new()
            {
                { "@id", id },
                { "@active", active }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_UpdateUserStatus_FP", con, paramDic);

            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected;
            }
            catch (SqlException ex)
            {
                throw new Exception($"SQL Error during UpdateUserStatus: {ex.Message}", ex);
            }
            finally
            {
                con.Close();
            }
        }

        // =====================
        // 7) SOFT DELETE A USER
        // =====================

        // Marks a user as deleted without physically removing them from the DB.
        // This is safer than hard delete (keeps history).
        // Returns:
        //  0 = success
        //  1 = user not found or already deleted
        public int SoftDeleteUserByEmail(string email)
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
                { "@userEmail", email }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_DeleteUser_FP", con, paramDic);

            try
            {
                cmd.ExecuteNonQuery();
                return 0;
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("User not found or already deleted"))
                    return 1;
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // ==============================
        // 8) GET ALL ACTIVE USERS
        // ==============================

        // Retrieves all users whose "Active" field = true.
        // Returns a list of Users objects.
        public List<Users> ReadAllUsers()
        {
            List<Users> users = new List<Users>();
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

            cmd = CreateCommandWithStoredProcedureGeneral("SP_GetAllUsers_FP", con, null);

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Users u = new Users
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Active = Convert.ToBoolean(reader["Active"])
                    };
                    users.Add(u);
                }

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }

        // =========================================
        // 9) GET ALL USERS (INCLUDING INACTIVE ONES)
        // =========================================

        // Retrieves all users, both active and inactive.
        // Useful for admin views.
        public List<Users> ReadAllUsersAdmin()
        {
            List<Users> users = new List<Users>();
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

            cmd = CreateCommandWithStoredProcedureGeneral("SP_GetAllUsersStatus_FP", con, null);

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Users u = new Users
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Active = Convert.ToBoolean(reader["Active"])
                    };
                    users.Add(u);
                }

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }

        // ==================================
        // 10) GET ADMIN DASHBOARD STATISTICS
        // ==================================

        // Retrieves daily aggregated statistics for admin:
        //  - loginCounter      = number of user logins
        //  - apiFetchCounter   = number of API calls to fetch news
        //  - savedNewsCounter  = number of saved articles
        public AdminStats GetAdminStats()
        {
            SqlConnection con;
            SqlCommand cmd;
            AdminStats stats = new AdminStats();

            try
            {
                con = connect("myProjDB");
            }
            catch (Exception ex)
            {
                throw new Exception("Database connection error: " + ex.Message, ex);
            }

            cmd = CreateCommandWithStoredProcedureGeneral("SP_GetAdminStats_FP", con, null);

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Read each field from the DB and map it to the AdminStats object
                    stats.Date = reader.GetDateTime(reader.GetOrdinal("Date")).ToString("yyyy-MM-dd");
                    stats.LoginCounter = reader.GetInt32(reader.GetOrdinal("loginCounter"));
                    stats.ApiFetchCounter = reader.GetInt32(reader.GetOrdinal("apiFetchCounter"));
                    stats.SavedNewsCounter = reader.GetInt32(reader.GetOrdinal("savedNewsCounter"));
                }

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve admin stats: " + ex.Message, ex);
            }
            finally
            {
                con.Close();
            }
        }
    }
}
