using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace KellCommons.RoleManage.DAL
{
    public  class RolePopedomService
    {
        public bool DeletePopedomByRoleId(int RoleId)
        {

            string sql = "delete from RolePoperdomInfo where RoleId = @RoleId ";
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


        public int InsertPopedom(int RoleId,int popedomId)
        {

            string sql = "insert into RolePoperdomInfo values(@RoleID,@PopedomId) ";
            SqlParameter[] param = { 
                                     new SqlParameter("@RoleID",RoleId),
                                     new SqlParameter("@PopedomId",popedomId) };

            int result = 0;
            try
            {
                result = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.Text, sql, param);

            }
            catch (Exception e)
            {

                throw e;
            }

            return result ;
        }

    }
}
