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

        // Insert a new user using the stored procedure SP_InsertUser_FP
        public int InsertUser(Users user)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = connect("myProjDB");
            }
            catch (Exception ex)
            {
                throw ex; // Connection failure
            }

            // Set parameters for the stored procedure
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@name", user.Name },
                { "@password", user.Password },
                { "@email", user.Email }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_InsertUser_FP", con, paramDic);

            try
            {
                cmd.ExecuteNonQuery();
                return 0; // Success
            }
            catch (SqlException ex)
            {
                // 2627/2601 = unique constraint violation (duplicate key)
                if (ex.Number == 2627 || ex.Number == 2601)
                    return 3;
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // Attempt to log in a user by email and password
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

            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@email", email },
                { "@password", password }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_LoginUser_FP", con, paramDic);

            Users? u = null;

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Create Users object from DB result
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

        // Update an existing user's details (name, email, password)
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

            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@id", user.Id },
                { "@name", user.Name },
                { "@password", user.Password },
                { "@email", user.Email }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("SP_UpdateUser_FP", con, paramDic);

            // Set up output parameter to get return value from the stored procedure
            SqlParameter returnValue = new SqlParameter("@ReturnVal", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnValue);

            try
            {
                cmd.ExecuteNonQuery();
                return (int)returnValue.Value; // 1 = success, 0 = not found
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    return 3; // Duplicate email or username
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        // Toggle user active status (true/false)
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
                return rowsAffected; // 1 = success, 0 = not found
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

        // Soft-delete a user by marking them as deleted (does not remove from DB)
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
                return 0; // Success
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

        // Retrieve list of all active users (simplified view)
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

        // Retrieve list of all users, including inactive ones (for admin view)
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

        // Get aggregated admin statistics from DB (logins, API fetches, saved articles)
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
