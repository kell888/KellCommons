using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using System.Data.SqlClient;
using System.Data;

namespace KellCommons.RoleManage.DAL
{
    public class UserInfoService
    {
        public List<UserInfo> GetUserInfos()
        {
            List<UserInfo> list = new List<UserInfo>();
            string sql = "select * from UserInfo ";

            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, null);
                while (reader.Read())
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserID = Convert.ToInt32(reader["UserID"]);
                    userInfo.UserName = reader["UserName"].ToString();
                    userInfo.UserPass = reader["UserPass"].ToString();
                    userInfo.Sex = reader["Sex"].ToString();
                    userInfo.Phone = reader["Phone"].ToString();
                    userInfo.Address = reader["Address"].ToString();
                    userInfo.TrueName = reader["TrueName"].ToString();
                    userInfo.Time = Convert.ToDateTime(reader["Time"]);
                    list.Add(userInfo);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return list;
        }

        public UserInfo GetUserInfoByName(string name, string pass)
        {
            string sql = "select * from UserInfo where UserName = @UserName and UserPass = @UserPass";
            SqlParameter[] param = {
                                   new SqlParameter("@UserName",name),
                                   new SqlParameter("@UserPass",pass)};
            UserInfo userInfo = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                if (reader.Read())
                {
                    userInfo = new UserInfo();
                    userInfo.UserID = Convert.ToInt32(reader["UserID"]);
                    userInfo.UserName = reader["UserName"].ToString();
                    userInfo.UserPass = reader["UserPass"].ToString();
                    userInfo.Sex = reader["Sex"].ToString();
                    userInfo.Phone = reader["Phone"].ToString();
                    userInfo.Address = reader["Address"].ToString();
                    userInfo.TrueName = reader["TrueName"].ToString();
                    userInfo.Time = Convert.ToDateTime(reader["Time"]);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return userInfo;
        }


        public UserInfo GetUserInfoById(int id)
        {
            string sql = "select * from UserInfo where UserID = @UserID";
            SqlParameter[] param = {
                                   new SqlParameter("@UserID",id) };
            UserInfo userInfo = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                if (reader.Read())
                {
                    userInfo = new UserInfo();
                    userInfo.UserID = Convert.ToInt32(reader["UserID"]);
                    userInfo.UserName = reader["UserName"].ToString();
                    userInfo.UserPass = reader["UserPass"].ToString();
                    userInfo.Sex = reader["Sex"].ToString();
                    userInfo.Phone = reader["Phone"].ToString();
                    userInfo.Address = reader["Address"].ToString();
                    userInfo.TrueName = reader["TrueName"].ToString();
                    userInfo.Time = Convert.ToDateTime(reader["Time"]);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return userInfo;
        }

        public int RegisterUser(UserInfo userInfo)
        {

            string sql = "insert into UserInfo values(@userName,@userPass,@sex,@phone,@addresss,@trueName,@time) ";
            sql += "select @@identity as 'UserId'";
            SqlParameter[] param = {
                                   new SqlParameter("@userName",userInfo.UserName), 
                                   new SqlParameter("@userPass",userInfo.UserPass), 
                                   new SqlParameter("@sex",userInfo.Sex), 
                                   new SqlParameter("@phone",userInfo.Phone), 
                                   
                                   new SqlParameter("@addresss",userInfo.Address), 
                                   new SqlParameter("@trueName",userInfo.TrueName), 
                                   
                                   new SqlParameter("@time",userInfo.Time), };
            int result = 0;
            try
            {
               
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                if (reader.Read())
                {
                    result = Convert.ToInt32(reader["UserId"]);

                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }

        public int DeleteUser(int userId)
        {

            string sql = "delete from UserInfoRole where UserId = @UserId;";
            sql += "delete from UserInfo where UserId = @UserId";
            SqlParameter[] param = {
                                   new SqlParameter("@UserId",userId), 
                                  };
            int result = 0;
            try
            {

                result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, param);
              
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }
    }
}
