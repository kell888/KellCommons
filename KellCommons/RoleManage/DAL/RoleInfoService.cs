using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using System.Data.SqlClient;
using System.Data;

namespace KellCommons.RoleManage.DAL
{
    public  class RoleInfoService
    {

        public List<RoleInfo> GetRoleInfos()
        {
            string sql = "select * from RoleInfo";
            List<RoleInfo> lists = new List<RoleInfo>();
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, null);
                while (reader.Read())
                {
                    RoleInfo userRole = new RoleInfo();
                    userRole.RoleID = Convert.ToInt32(reader["RoleID"]);
                    userRole.RoleName = reader["RoleName"].ToString();
                    lists.Add(userRole);
                }

            }
            catch (Exception e)
            {

                throw e;
            }

            return lists;
        }

        public RoleInfo GetRoleInfoById(int roleId)
        {
            string sql = "select * from RoleInfo where RoleID = @RoleID";
            SqlParameter[] param = {
                                   new SqlParameter("@RoleID",roleId) };
            RoleInfo userRole = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString,CommandType.Text,sql,param);
                while(reader.Read())
                {
                    userRole = new RoleInfo();
                    userRole.RoleID = Convert.ToInt32( reader["RoleID"]);
                    userRole.RoleName = reader["RoleName"].ToString();
                }
              
            }
            catch (Exception e)
            {
                
                throw e;
            }

            return userRole;
        }


        public int AddRoleInfo(string roleName)
        {
            string sql = "insert into RoleInfo values(@RoleName) select @@identity as 'id'";
            SqlParameter[] param = {
                                   new SqlParameter("@RoleName",roleName) };
            int result = 0;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                if (reader.Read())
                {
                    result = Convert.ToInt32(reader["id"]);
                }

               

            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }


        public int UpdateRoleInfoByRole(RoleInfo roleInfo)
        {
            string sql = "update RoleInfo set RoleName = @RoleName where RoleID = @RoleID ";
            SqlParameter[] param = {
                                        new SqlParameter("@RoleID",roleInfo.RoleID) ,
                                   new SqlParameter("@RoleName",roleInfo.RoleName) };
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

        public int DeleteRoleInfoByRole(int roleId)
        {
            string sql = "delete from  RoleInfo where RoleID = @RoleID";
            SqlParameter[] param = {
                                        new SqlParameter("@RoleID",roleId)
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
