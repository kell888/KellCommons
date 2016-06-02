using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;
using KellCommons.DEncrypt;
using System.Configuration;

namespace KellCommons.DataBase
{
    public class OleDAL
    {
        public readonly static string MdbFilePath = AppDomain.CurrentDomain.BaseDirectory + KellCommon.ReadAppSetting("dbName");
        static string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=True;Data Source=" + MdbFilePath;
        static OleDbConnection conn = new OleDbConnection(connString);

        public static OleDbConnection Connection
        {
            get { return conn; }
        }

        public static string ConnString
        {
            get { return connString; }
        }

        public static bool ExecuteNonQuery(string sqlText)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool ExecuteNonQuery(string sqlText, OleDbCommand cmd)
        {
            try
            {
                cmd.CommandText = sqlText;
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static bool ExecuteNonQuery(string sqlText, OleDbParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                cmd.Parameters.AddRange(paras);
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool ExecuteNonQuery(string sqlText, CommandType cmdType, OleDbParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                cmd.CommandType = cmdType;
                cmd.Parameters.AddRange(paras);
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool ExecuteNonQuery(string sqlText, CommandType cmdType, OleDbCommand cmd)
        {
            try
            {
                cmd.CommandText = sqlText;
                cmd.CommandType = cmdType;
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static OleDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            if (conn.State == ConnectionState.Open)
                return conn.BeginTransaction(isolationLevel);
            else
                return null;
        }

        public static bool CommitTransaction(OleDbCommand cmd)
        {
            if (conn.State == ConnectionState.Open)
            {
                try
                {
                    cmd.Transaction.Commit();
                    return true;
                }
                catch (OleDbException e)
                {
                    throw new Exception("提交事务失败：" + e.Message);
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    conn.Close();
                }
            }
            return false;
        }

        public static bool EnforceRollback(OleDbCommand cmd)
        {
            if (conn.State == ConnectionState.Open)
            {
                try
                {
                    cmd.Transaction.Rollback();
                    return true;
                }
                catch (OleDbException e)
                {
                    throw new Exception("强行回滚事务失败：" + e.Message);
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    conn.Close();
                }
            }
            return false;
        }

        public static DataTable GetRecords(string sqlText)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbDataAdapter adp = new OleDbDataAdapter(sqlText, conn);
            try
            {
                DataTable data = new DataTable();
                adp.Fill(data);
                return data;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (adp != null)
                    adp.Dispose();
                conn.Close();
            }
        }

        public static DataTable GetRecords(string sqlText, OleDbParameter[] parameters)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = new OleDbCommand(sqlText, conn);
            cmd.Parameters.AddRange(parameters);
            OleDbDataAdapter adp = new OleDbDataAdapter(cmd);
            try
            {
                DataTable data = new DataTable();
                adp.Fill(data);
                return data;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                if (adp != null)
                    adp.Dispose();
                conn.Close();
            }
        }

        public static bool Exists(string tableName, string where)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                tableName = tableName.Trim();
                if (tableName.StartsWith("["))
                    tableName = tableName.Substring(1);
                if (tableName.EndsWith("]"))
                    tableName = tableName.Substring(0, tableName.Length - 1);
                where = where.Trim().ToLower();
                if (!where.StartsWith("and"))
                    where = "and " + where;
                cmd.CommandText = "select count(1) from [" + tableName + "] where (1=1) " + where;
                object obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    return int.Parse(obj.ToString()) > 0;
                return false;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool Exists(string tableName, string where, OleDbParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                tableName = tableName.Trim();
                if (tableName.StartsWith("["))
                    tableName = tableName.Substring(1);
                if (tableName.EndsWith("]"))
                    tableName = tableName.Substring(0, tableName.Length - 1);
                where = where.Trim().ToLower();
                if (!where.StartsWith("and"))
                    where = "and " + where;
                cmd.CommandText = "select count(1) from [" + tableName + "] where (1=1) " + where;
                cmd.Parameters.AddRange(paras);
                object obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    return int.Parse(obj.ToString()) > 0;
                return false;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static object GetOneValue(string sqlText)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                object obj = cmd.ExecuteScalar();
                return obj;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static object GetOneValue(string sqlText, OleDbCommand cmd)
        {
            try
            {
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                cmd.CommandText = sqlText;
                object obj = cmd.ExecuteScalar();
                return obj;
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static object GetOneValue(string sqlText, OleDbParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            OleDbCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                cmd.Parameters.AddRange(paras);
                object obj = cmd.ExecuteScalar();
                return obj;
            }
            catch (OleDbException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }
        [Obsolete("停用。在Access中不存在IDENT_CURRENT('TABLENAME')函数", true)]
        /// <summary>
        /// 停用。在Access中不存在IDENT_CURRENT('TABLENAME')函数
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static string GetCurrentID(string TableName)
        {
            object obj = GetOneValue("select IDENT_CURRENT('" + TableName + "')");
            if (obj != null && obj != DBNull.Value)
                return obj.ToString();
            else
                return "0";
        }
        [Obsolete("停用。在Access中不存在IDENT_CURRENT('TABLENAME')函数", true)]
        /// <summary>
        /// 停用。在Access中不存在IDENT_CURRENT('TABLENAME')函数
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string GetCurrentID(string TableName, OleDbCommand cmd)
        {
            try
            {
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                object obj = GetOneValue("select IDENT_CURRENT('" + TableName + "')", cmd);
                if (obj != null && obj != DBNull.Value)
                    return obj.ToString();
                else
                    return "0";
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static string GetCurrentID()
        {
            object obj = GetOneValue("select @@IDENTITY");
            if (obj != null && obj != DBNull.Value)
                return obj.ToString();
            else
                return "0";
        }

        public static string GetCurrentID(OleDbCommand cmd)
        {
            try
            {
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                object obj = GetOneValue("select @@IDENTITY", cmd);
                if (obj != null && obj != DBNull.Value)
                    return obj.ToString();
                else
                    return "0";
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static int GetMaxID(string idFieldName, string tableName)
        {
            object obj = OleDAL.GetOneValue("select max(" + idFieldName + ") from " + tableName);
            if (obj != null && obj != DBNull.Value)
                return (int)obj;
            else
                return 0;
        }
    }

    public class SqlDAL
    {
        static SqlConnection conn = new SqlConnection(ConnString);

        public static SqlConnection Connection
        {
            get { return conn; }
        }

        public static string ConnString
        {
            get
            {
                DesUtility des = new DesUtility();
                return des.Decrypt(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
            }
        }

        public static bool ExecuteNonQuery(string sqlText)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool ExecuteNonQuery(string sqlText, SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = sqlText;
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (SqlException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static bool ExecuteNonQuery(string sqlText, SqlParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                cmd.Parameters.AddRange(paras);
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool ExecuteNonQuery(string sqlText, CommandType cmdType, SqlParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                cmd.CommandType = cmdType;
                cmd.Parameters.AddRange(paras);
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool ExecuteNonQuery(string sqlText, CommandType cmdType, SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = sqlText;
                cmd.CommandType = cmdType;
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                int i = cmd.ExecuteNonQuery();
                return i > 0;
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static SqlTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            if (conn.State == ConnectionState.Open)
                return conn.BeginTransaction(isolationLevel);
            else
                return null;
        }

        public static bool CommitTransaction(SqlCommand cmd)
        {
            if (conn.State == ConnectionState.Open)
            {
                try
                {
                    cmd.Transaction.Commit();
                    return true;
                }
                catch (SqlException e)
                {
                    throw new Exception("提交事务失败：" + e.Message);
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    conn.Close();
                }
            }
            return false;
        }

        public static bool EnforceRollback(SqlCommand cmd)
        {
            if (conn.State == ConnectionState.Open)
            {
                try
                {
                    cmd.Transaction.Rollback();
                    return true;
                }
                catch (SqlException e)
                {
                    throw new Exception("强行回滚事务失败：" + e.Message);
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    conn.Close();
                }
            }
            return false;
        }

        public static DataTable GetRecords(string sqlText)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlDataAdapter adp = new SqlDataAdapter(sqlText, conn);
            try
            {
                DataTable data = new DataTable();
                adp.Fill(data);
                return data;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (adp != null)
                    adp.Dispose();
                conn.Close();
            }
        }

        public static DataTable GetRecords(string sqlText, SqlParameter[] parameters)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = new SqlCommand(sqlText, conn);
            cmd.Parameters.AddRange(parameters);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            try
            {
                DataTable data = new DataTable();
                adp.Fill(data);
                return data;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                if (adp != null)
                    adp.Dispose();
                conn.Close();
            }
        }

        public static bool Exists(string tableName, string where)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                tableName = tableName.Trim();
                if (tableName.StartsWith("["))
                    tableName = tableName.Substring(1);
                if (tableName.EndsWith("]"))
                    tableName = tableName.Substring(0, tableName.Length - 1);
                where = where.Trim().ToLower();
                if (!where.StartsWith("and"))
                    where = "and " + where;
                cmd.CommandText = "select count(1) from [" + tableName + "] where (1=1) " + where;
                object obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    return int.Parse(obj.ToString()) > 0;
                return false;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static bool Exists(string tableName, string where, SqlParameter[] paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                tableName = tableName.Trim();
                if (tableName.StartsWith("["))
                    tableName = tableName.Substring(1);
                if (tableName.EndsWith("]"))
                    tableName = tableName.Substring(0, tableName.Length - 1);
                where = where.Trim().ToLower();
                if (!where.StartsWith("and"))
                    where = "and " + where;
                cmd.CommandText = "select count(1) from [" + tableName + "] where (1=1) " + where;
                cmd.Parameters.AddRange(paras);
                object obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    return int.Parse(obj.ToString()) > 0;
                return false;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static object GetOneValue(string sqlText)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                object obj = cmd.ExecuteScalar();
                return obj;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }

        public static object GetOneValue(string sqlText, SqlCommand cmd)
        {
            try
            {
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                cmd.CommandText = sqlText;
                object obj = cmd.ExecuteScalar();
                return obj;
            }
            catch (OleDbException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static object GetOneValue(string sqlText, SqlParameterCollection paras)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = sqlText;
                foreach (SqlParameter para in paras)
                    cmd.Parameters.AddWithValue(para.ParameterName, para.Value);
                object obj = cmd.ExecuteScalar();
                return obj;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                conn.Close();
            }
        }
        /// <summary>
        /// 利用IDENT_CURRENT('TABLENAME')函数获取指定表名TABLENAME的当前ID
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static string GetCurrentID(string TableName)
        {
            object obj = GetOneValue("select IDENT_CURRENT('" + TableName + "')");
            if (obj != null && obj != DBNull.Value)
                return obj.ToString();
            else
                return "0";
        }
        /// <summary>
        /// 利用IDENT_CURRENT('TABLENAME')函数获取指定表名TABLENAME的当前ID
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string GetCurrentID(string TableName, SqlCommand cmd)
        {
            try
            {
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                object obj = GetOneValue("select IDENT_CURRENT('" + TableName + "')", cmd);
                if (obj != null && obj != DBNull.Value)
                    return obj.ToString();
                else
                    return "0";
            }
            catch (SqlException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static string GetCurrentID()
        {
            object obj = GetOneValue("select @@IDENTITY");
            if (obj != null && obj != DBNull.Value)
                return obj.ToString();
            else
                return "0";
        }

        public static string GetCurrentID(SqlCommand cmd)
        {
            try
            {
                if (cmd.Transaction == null)
                    cmd.Transaction = BeginTransaction(default(IsolationLevel));
                object obj = GetOneValue("select @@IDENTITY", cmd);
                if (obj != null && obj != DBNull.Value)
                    return obj.ToString();
                else
                    return "0";
            }
            catch (SqlException e)
            {
                cmd.Transaction.Rollback();
                throw e;
            }
        }

        public static int GetMaxID(string idFieldName, string tableName)
        {
            object obj = SqlDAL.GetOneValue("select max(" + idFieldName + ") from " + tableName);
            if (obj != null && obj != DBNull.Value)
                return (int)obj;
            else
                return 0;
        }
    }
}
