using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using System.Data;
using System.Data.SqlClient;

namespace KellCommons.RoleManage.DAL
{
    public class PopedomInfoService
    {

        public List<PopedomInfo> GetPopedomInfos(int userId)
        {
            string sql = "select * from PopedomInfo where PopedomInfo.ParentID = 0 or PopedomInfo.PopedomId in (select PopedomId from  RolePoperdomInfo where RoleID in (select RoleId from UserInfoRole where UserId = @UserId))";
           
            List<PopedomInfo> list = new List<PopedomInfo>();
            Dictionary<int, List<PopedomInfo>> dicti = new Dictionary<int, List<PopedomInfo>>();
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            PopedomInfo p = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                while (reader.Read())
                {
                    p = new PopedomInfo();
                    p.PopedomId = Convert.ToInt32(reader["PopedomId"]);
                    p.PopedomName = reader["PopedomName"].ToString();
                    p.ParentID = Convert.ToInt32(reader["ParentID"]);
                    p.Url = reader["Url"].ToString();
                    list.Add(p);
                }
               
            }
            catch (Exception e)
            {

                throw e;
            }

            return list;
        }

        public List<PopedomInfo> GetPopedomInfoByUserId(int userId)
        {
            string sql = "select * from PopedomInfo where PopedomInfo.PopedomId in (select PopedomId from  RolePoperdomInfo where RoleID in (select RoleId from UserInfoRole where UserId = @UserId))";

            List<PopedomInfo> list = new List<PopedomInfo>();
            Dictionary<int, List<PopedomInfo>> dicti = new Dictionary<int, List<PopedomInfo>>();
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            PopedomInfo p = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                while (reader.Read())
                {
                    p = new PopedomInfo();
                    p.PopedomId = Convert.ToInt32(reader["PopedomId"]);
                    p.PopedomName = reader["PopedomName"].ToString();
                    p.ParentID = Convert.ToInt32(reader["ParentID"]);
                    p.Url = reader["Url"].ToString();
                    list.Add(p);
                }

            }
            catch (Exception e)
            {

                throw e;
            }

            return list;
        }

        public List<PopedomInfo> GetPopedomInfos()
        {
            string sql = "select * from PopedomInfo where ParentID <> 0 ";

            List<PopedomInfo> list = new List<PopedomInfo>();
            Dictionary<int, List<PopedomInfo>> dicti = new Dictionary<int, List<PopedomInfo>>();
          
            PopedomInfo p = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, null);
                while (reader.Read())
                {
                    p = new PopedomInfo();
                    p.PopedomId = Convert.ToInt32(reader["PopedomId"]);
                    p.PopedomName = reader["PopedomName"].ToString();
                    p.ParentID = Convert.ToInt32(reader["ParentID"]);
                    p.Url = reader["Url"].ToString();
                    list.Add(p);
                }

            }
            catch (Exception e)
            {

                throw e;
            }

            return list;
        }

        public List<PopedomInfo> GetPopedomInfosByRoleId(int roleId)
        {
            string sql = "select * from PopedomInfo where PopedomId in (select PopedomId from RolePoperdomInfo where RoleId = @RoleId)";

            List<PopedomInfo> list = new List<PopedomInfo>();
            Dictionary<int, List<PopedomInfo>> dicti = new Dictionary<int, List<PopedomInfo>>();
            SqlParameter[] param = { new SqlParameter("@RoleId", roleId) };
            PopedomInfo p = null;
            try
            {
                SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, param);
                while (reader.Read())
                {
                    p = new PopedomInfo();
                    p.PopedomId = Convert.ToInt32(reader["PopedomId"]);
                    p.PopedomName = reader["PopedomName"].ToString();
                    p.ParentID = Convert.ToInt32(reader["ParentID"]);
                    p.Url = reader["Url"].ToString();
                    list.Add(p);
                }

            }
            catch (Exception e)
            {

                throw e;
            }

            return list;
        }

       
        //public Dictionary<int, List<PopedomInfo>> GetPopedomInfos()
        //{
        //    string sql = "select * from PopedomInfo where ParentID = 0";
        //    string childSql = "select * from PopedomInfo where ParentID = @ParentID";
        //    List<PopedomInfo> list = new List<PopedomInfo>();
        //    Dictionary<int, List<PopedomInfo>> dicti = new Dictionary<int, List<PopedomInfo>>();
        //    PopedomInfo p = null;
        //    try
        //    {
        //        SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, sql, null);
        //        while (reader.Read())
        //        {
        //            p = new PopedomInfo();
        //            p.PopedomId = Convert.ToInt32(reader["PopedomId"]);
        //            p.PopedomName = reader["PopedomName"].ToString();
        //            p.ParentID = Convert.ToInt32(reader["ParentID"]);
        //            p.Url = reader["Url"].ToString();
        //            list.Add(p);
        //        }
        //        dicti.Add(0,list);

        //        for (int i = 0; i < dicti[0].Count; i++)
        //        {
        //            int parentId = dicti[0][i].PopedomId;

        //            SqlParameter[] param = { new SqlParameter("@ParentID",parentId) };
        //            list = new List<PopedomInfo>();
        //            SqlDataReader readers = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.Text, childSql, param);
        //            while (readers.Read())
        //            {
        //                p = new PopedomInfo();
        //                p.PopedomId = Convert.ToInt32(readers["PopedomId"]);
        //                p.PopedomName = readers["PopedomName"].ToString();
        //                p.ParentID = Convert.ToInt32(readers["ParentID"]);
        //                p.Url = readers["Url"].ToString();
        //                list.Add(p);
        //            }

        //            dicti.Add(parentId,list);
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //        throw e;
        //    }

        //    return dicti;
        //}
    }
}
