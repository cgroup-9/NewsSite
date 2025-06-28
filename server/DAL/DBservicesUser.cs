using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using server.Models;

namespace server.DAL
{
    public class DBservicesUser
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
                throw ex;
            }

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
                return 0;
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

            SqlParameter returnValue = new SqlParameter("@ReturnVal", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnValue);

            try
            {
                cmd.ExecuteNonQuery();
                return (int)returnValue.Value; // 1 = success, 0 = user not found
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
                return rowsAffected; // 1 = success, 0 = user not found
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

    }
}
