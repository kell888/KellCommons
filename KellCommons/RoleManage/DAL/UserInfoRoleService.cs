using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using System.Data.SqlClient;
using System.Data;

namespace KellCommons.RoleManage.DAL
{
    public class UserInfoRoleService
    {
        public List<UserInfoRole> GetUserInfoRoles(int userId)
        {
            List<UserInfoRole> list = new List<UserInfoRole>();
            string sql = "select * from UserInfoRole where UserId = @UserId";
            SqlParameter[] param = {
                                   new SqlParameter("@UserId",userId) };
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString,CommandType.Text,sql,param);
                while(reader.Read())
                {
                    UserInfoRole userRole = new UserInfoRole();
                    userRole.UserRoleId = Convert.ToInt32( reader["UserRoleId"]);
                    userRole.UserInfo = new UserInfoService().GetUserInfoById(userId);
                    userRole.RoleInfo = new RoleInfoService().GetRoleInfoById(Convert.ToInt32(reader["RoleId"]));
                    list.Add(userRole);
                }
              
            }
            catch (Exception e)
            {
                
                throw e;
            }

            return list;
        }


        public List<UserInfoRole> GetUserInfoRoles()
        {
            List<UserInfoRole> list = new List<UserInfoRole>();
            string sql = "select * from UserInfoRole";
           
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, null);
                while (reader.Read())
                {
                    UserInfoRole userRole = new UserInfoRole();
                    userRole.UserRoleId = Convert.ToInt32(reader["UserRoleId"]);
                    userRole.UserInfo = new UserInfoService().GetUserInfoById(Convert.ToInt32(reader["UserID"]));
                    userRole.RoleInfo = new RoleInfoService().GetRoleInfoById(Convert.ToInt32(reader["RoleId"]));
                    list.Add(userRole);
                }

            }
            catch (Exception e)
            {

                throw e;
            }

            return list;
        }

        public bool InsertUserRole(int userId)
        {
           
            string sql = "insert into UserInfoRole values (@userId,6)";
            SqlParameter[] param = { new SqlParameter("@userId", userId) };

            int result = 0;
            try
            {
                result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, param);
           
            }
            catch (Exception e)
            {

                throw e;
            }

            return result > 0 ;
        }

        public bool UpdateUserRoleByUserId(int RoleId,int userId)
        {

            string sql = "update UserInfoRole set RoleId = @RoleId where UserId = @UserId ";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) ,
                                     new SqlParameter("@RoleId",RoleId)};

            int result = 0;
            try
            {
                result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, param);

            }
            catch (Exception e)
            {

                throw e;
            }

            return result > 0;
        }

        public bool DeleteUserRoleByRoleId(int RoleId)
        {

            string sql = "delete from UserInfoRole where RoleId = @RoleId ";
            SqlParameter[] param = { 
                                     new SqlParameter("@RoleId",RoleId)};

            int result = 0;
            try
            {
                result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, param);

            }
            catch (Exception e)
            {

                throw e;
            }

            return result > 0;
        }


        public bool UpdateRoleByRoleId(int RoleId)
        {

            string sql = "update UserInfoRole set RoleId = 6 where RoleId = @RoleId";
            SqlParameter[] param = { 
                                     new SqlParameter("@RoleId",RoleId)};

            int result = 0;
            try
            {
                result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, param);

            }
            catch (Exception e)
            {

                throw e;
            }

            return result > 0;
        }
    }
}
