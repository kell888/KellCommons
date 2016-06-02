using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;

namespace KellCommons
{
    public class SysUtil
    {
        /// <summary>
        /// 取得配置信息
        /// </summary>
        public static string getAppSetting(string appKey)
        {
            string getAppSetString = ConfigurationManager.AppSettings[appKey].ToString();
            return getAppSetString;
        }
        /// <summary>
        /// 调用分页过程
        /// </summary>
        public static bool ExecPagination(string connectionString, int pindex, string pTable, string pSearch, string sfield, int orderType, string tbID, string orderby, int psize, out int pcount, out DataSet getRecordData)
        {
            bool vret = false;
            getRecordData = null;
            pcount = 0;
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connectionString;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            //取得总记录数
            pcount = GetRecordCount(conn, pTable, pSearch);
            string gSearchStr = " 1=1 " + pSearch;

            //取得记录集
            if (pcount > 0)
            {
                vret = true;
                cmd.Parameters.Clear();
                try
                {
                    cmd.CommandText = "SP_Pager";
                    cmd.Parameters.Add("@page", SqlDbType.Int).Value = pindex;
                    cmd.Parameters.Add("@pagesize", SqlDbType.Int).Value = psize;
                    cmd.Parameters.Add("@table", SqlDbType.VarChar, 2000).Value = pTable;
                    cmd.Parameters.Add("@condition", SqlDbType.VarChar, 800).Value = gSearchStr;
                    cmd.Parameters.Add("@collist", SqlDbType.VarChar, 800).Value = sfield;
                    cmd.Parameters.Add("@orderby", SqlDbType.Bit).Value = orderType;
                    cmd.Parameters.Add("@col", SqlDbType.VarChar, 50).Value = tbID;
                    cmd.Parameters.Add("@orderbyCol", SqlDbType.VarChar, 800).Value = orderby;
                    cmd.Parameters.Add("@total_Page", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@sql", SqlDbType.NVarChar, 4000).Direction = ParameterDirection.Output;
                    SqlDataReader Adr = cmd.ExecuteReader();
                    //int totalPage = Convert.ToInt32(cmd.Parameters[7].Value.ToString());
                    //string sql = cmd.Parameters[8].Value.ToString();
                    getRecordData = ConvertDataReaderToDataSet(Adr);
                    Adr.Close();
                }
                catch// (Exception ex)
                {
                    return false;
                }
            }
            conn.Close();
            conn.Dispose();
            return vret;
        }
        /// <summary>
        /// 调用分页过程
        /// </summary>
        public static DataSet ExecPagination(string connectionString, string sql, string selectAll, out int pcount)
        {
            DataSet getRecordData = new DataSet();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connectionString;
            SqlDataAdapter adp = new SqlDataAdapter(sql, conn);
            conn.Open();
            adp.Fill(getRecordData);
            adp.Dispose();
            DataTable dt = new DataTable();
            SqlDataAdapter ad = new SqlDataAdapter(selectAll, conn);
            ad.Fill(dt);
            pcount = dt.Rows.Count;
            ad.Dispose();
            conn.Close();
            conn.Dispose();
            return getRecordData;
        }

        /// <summary>
        /// 选出记录数
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">查询条件</param>
        public static int GetRecordCount(SqlConnection conn, string TBName, string serachStr)
        {
            int getCount = 0;
            string sql = "SELECT Count(*) as CountNum FROM " + TBName + " where 1=1 " + serachStr + " ";
            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    getCount = int.Parse(dr["CountNum"].ToString());
                }
                dr.Close();
            }
            catch// (Exception ex)
            {
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return getCount;
        }
        /// <summary>
        /// 通查询表名,查询条件,返回DATASET结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">查询条件</param>
        public static DataSet VIEW_TableData(SqlConnection conn, string TBName, string serachStr)
        {
            DataSet ds = null;

            string sql = "SELECT * FROM " + TBName + " where 1=1 " + serachStr + " ";
            //WriterErr("VIEW_TableData", sql);
            SqlDataAdapter adp = null;
            try
            {
                adp = new SqlDataAdapter(sql, conn);
                ds = new DataSet();
                adp.Fill(ds);
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (adp != null)
                    adp.Dispose();
            }

            return ds;
        }//VIEW_TableData

        /// <summary>
        /// 通查询表名,查询条件,返回DATASET结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">查询条件</param>
        public static DataSet VIEW_TableDataTop(SqlConnection conn, string TBName, string serachStr, int vTop)
        {
            DataSet ds = null;

            string sql = "SELECT top " + vTop.ToString() + " * FROM " + TBName + " where 1=1 " + serachStr + " ";
            SqlDataAdapter adp = null;
            try
            {
                adp = new SqlDataAdapter(sql, conn);
                ds = new DataSet();
                adp.Fill(ds);
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (adp != null)
                    adp.Dispose();
            }

            return ds;
        }//VIEW_TableData

        /// <summary>
        /// 通查询表名,查询条件,返回DATASET结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql">查询条件</param>
        public static DataSet VIEW_TableData(SqlConnection conn, string sql)
        {
            DataSet ds = null;
            SqlDataAdapter adp = null;
            try
            {
                adp = new SqlDataAdapter(sql, conn);
                ds = new DataSet();
                adp.Fill(ds);
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (adp != null)
                    adp.Dispose();
            }

            return ds;
        }

        /// <summary>
        /// UPDATE表名,条件,返回结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">UPDATE值</param>
        public static int Update_MyTable(SqlConnection conn, string TBName, string UpdateValueStr, string equalStr)
        {
            int nRet = -1;

            string sql = "UPDATE " + TBName + " Set " + UpdateValueStr + " where " + equalStr;
            //WriterErr("Update_MyTable", sql);
            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                nRet = cmd.ExecuteNonQuery() >= 1 ? 0 : -1;
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return nRet;
        }

        /// <summary>
        /// UPDATE表名,条件,返回结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">UPDATE值</param>
        public static int Insert_MyTable(SqlConnection conn, string TBName, string InsertCol, string InsertValues)
        {
            int nRet = -1;

            string sql = "Insert INTO " + TBName + "" + InsertCol + " values " + InsertValues + "";
            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                nRet = cmd.ExecuteNonQuery() == 1 ? 0 : -1;
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return nRet;
        }
        /// <summary>
        /// UPDATE表名,条件,返回结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">UPDATE值</param>
        public static int Delete_MyTable(SqlConnection conn, string TBName, string equalStr)
        {
            int nRet = -1;

            string sql = "Delete " + TBName + " where " + equalStr;
            //WriterErr("Delete_MyTable", sql);
            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                nRet = cmd.ExecuteNonQuery() == 1 ? 0 : -1;
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return nRet;
        }


        //执行sql语句将结果存在Table表里
        public static DataTable ReturnDataTableExecuteSql(SqlConnection conn, string sql)
        {
            SqlDataAdapter adp = null;
            try
            {
                DataSet ds = new DataSet();
                adp = new SqlDataAdapter(sql, conn);
                adp.Fill(ds);
                if (ds.Tables[0].Rows.Count != 0)
                    return ds.Tables[0];

                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (adp != null)
                    adp.Dispose();
            }
        }

        /// <summary>
        /// 通过某条件得到某值
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="fieldNames">查询的列名</param>
        /// <param name="serachStr">查询条件</param>
        public static string getTableAValues(SqlConnection conn, string TBName, string fieldName, string serachStr)
        {
            string getValues = "";
            string sql = "SELECT top 1 " + fieldName + " FROM " + TBName + " where 1=1 " + serachStr + " ";

            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (dr[0] != null && dr[0] != DBNull.Value)
                        getValues = dr[0].ToString();
                }

            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }

            return getValues;
        }//VIEW_TableData

        /// <summary>
        /// 通查询表名,查询条件,返回DataReader结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">查询条件</param>
        public static SqlDataReader getTableData_DataReader(SqlConnection conn, string TBName, string serachStr)
        {
            SqlDataReader DataRead = null;
            string sql = "SELECT * FROM " + TBName + " where 1=1 " + serachStr + " ";
            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                DataRead = cmd.ExecuteReader();
            }
            catch// (Exception ex)
            {
                
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return DataRead;
        }//getTableData_DataReader
        /// <summary>
        /// 通查询表名,查询条件,返回DataReader结果
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TBName">表名</param>
        /// <param name="serachStr">查询条件</param>
        public static SqlDataReader getTableData_DataReader(SqlConnection conn, string sql)
        {
            SqlDataReader DataRead = null;
            SqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                DataRead = cmd.ExecuteReader();
            }
            catch// (Exception ex)
            {

            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return DataRead;
        }//getTableData_DataReader

        /// <summary>
        /// 取得单条记录
        /// </summary>
        public static DataRow GetSingleDataRow(SqlConnection conn, string TBName, string serachStr)
        {
            string sql = "SELECT TOP 1 * FROM " + TBName + " where 1=1 " + serachStr + " ";
            SqlDataAdapter adp = null;
            try
            {
                adp = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
            }
            catch { }
            finally
            {
                if (adp != null)
                    adp.Dispose();
            }
            return null;
        }


        /// <summary>
        /// 取得下拉列表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="DDLName"></param>
        /// <param name="TBName"></param>
        /// <param name="serachStr"></param>
        /// <param name="valueID"></param>
        /// <param name="valueName"></param>
        public static int getDrawDownList(SqlConnection conn, DropDownList DDLName, string TBName, string serachStr, string valueID, string valueName)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }
        /// <summary>
        /// 取得下拉列表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="DDLName"></param>
        /// <param name="TBName"></param>
        /// <param name="serachStr"></param>
        /// <param name="valueID"></param>
        /// <param name="valueName"></param>
        /// <param name="ShowAllStr"></param>
        public static int getDrawDownListAll(SqlConnection conn, DropDownList DDLName, string TBName, string serachStr, string valueID, string valueName, string ShowAllStr)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (ShowAllStr != "")
                DDLName.Items.Add(new ListItem("所有" + ShowAllStr, ""));
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }

        public static int getDrawDownListShow(SqlConnection conn, DropDownList DDLName, string TBName, string serachStr, string valueID, string valueName, string ShowName)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (ShowName != "")
                DDLName.Items.Add(new ListItem(ShowName, ""));
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }
        /// <summary>
        /// 取得下拉列表
        /// </summary>
        public static int getDrawDownList2(SqlConnection conn, DropDownList DDLName, string TBName, string serachStr, string valueID, string valueName1, string valueName2)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName1 + ""].ToString() + myRow["" + valueName2 + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }
        /// <summary>
        /// 取得下拉列表
        /// </summary>
        public static int getDrawDownList3(SqlConnection conn, DropDownList DDLName, string TBName, string serachStr, string valueID, string valueName)
        {
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 取得下拉列表
        /// </summary>
        public static int getListBox(SqlConnection conn, ListBox DDLName, string TBName, string serachStr, string valueID, string valueName)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }

        /// <summary>
        /// 取得下拉列表
        /// </summary>
        public static int getListBox2(SqlConnection conn, ListBox DDLName, string TBName, string serachStr, string valueID, string valueName1, string valueName2)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName1 + ""].ToString() + "<" + myRow["" + valueName2 + ""].ToString() + ">", myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }


        /// <summary>
        /// 取得下拉列表
        /// </summary>
        public static int getListBoxParent(SqlConnection conn, ListBox DDLName, string TBName, string serachStr, string valueID, string valueName1, string valueName2)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName1 + ""].ToString() + "上级 <" + myRow["" + valueName2 + ""].ToString() + ">", myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }


        public static int getCheckListBox(SqlConnection conn, CheckBoxList DDLName, string TBName, string serachStr, string valueID, string valueName)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }

        /// <summary>
        /// 取得下拉列表2
        /// </summary>
        public static int getDrawDownList(SqlConnection conn, DropDownList DDLName, string TBName, string serachStr, string valueID, string valueName, string SelectID)
        {
            //清除控件已有的内容
            if (DDLName.Items.Count != 0) DDLName.Items.Clear();
            //临时ID
            string tmpID1 = "", tmpID2 = "";
            DataSet DDL_GS = null;
            //取得数据表信息
            DDL_GS = VIEW_TableData(conn, TBName, serachStr);
            if (DDL_GS != null)
            {
                DataRow myRow;
                for (int i = 0; i < DDL_GS.Tables[0].Rows.Count; i++)
                {
                    myRow = DDL_GS.Tables[0].Rows[i];
                    tmpID1 = myRow["" + valueID + ""].ToString();
                    //过滤重复记录
                    if (tmpID1 != tmpID2)
                        DDLName.Items.Add(new ListItem(myRow["" + valueName + ""].ToString(), myRow["" + valueID + ""].ToString()));
                    tmpID2 = myRow["" + valueID + ""].ToString();
                }//end for
                ListItem item = DDLName.Items.FindByValue(SelectID);
                DDLName.SelectedIndex = DDLName.Items.IndexOf(item);
                return 1;
            }
            else
            {
                DDLName.Items.Add(new ListItem("没有记录", ""));
                return 0;
            }
        }

        /// <summary>
        /// 取得DataList中的选择的ID组合
        /// </summary>
        public static string GetSelectIDList(GridView myGridView, string sCheckBox)
        {
            string idStr = "";
            foreach (GridViewRow row in myGridView.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl(sCheckBox);
                if (cb != null)
                {
                    if (cb.Checked)
                    {
                        idStr += myGridView.DataKeys[row.RowIndex].Value.ToString() + ",";
                    }
                }
            }
            if (idStr != "")
                idStr = idStr.Substring(0, idStr.Length - 1);

            return idStr;
        }
        //把取得的ID转换为字符串格式
        public static string GetSelectIDListToStr(string getIDstr)
        {
            string retStr = "";
            retStr = "'" + getIDstr.Replace(",", "','") + "'";
            return retStr;
        }


        /// <summary>
        /// 获取指定数据源中某列对象
        /// </summary>
        /// <param name="dtSource"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DataColumn GetDataColumn(DataTable dtSource, string name)
        {

            DataColumn col = null;
            foreach (DataColumn dc in dtSource.Columns)
            {
                if (dc.ColumnName.Trim() == name.Trim() || dc.Caption.Trim() == name.Trim())
                {
                    col = dc;
                    break;
                }
            }

            return col;
        }
        /// <summary>
        /// 导出到Excel文件
        /// </summary>
        /// <param name="gv"></param>
        /// <param name="fileName"></param>
        public static void ExportToExcel(System.Web.UI.WebControls.GridView gv, string fileName)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.Charset = "GB2312";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8) + ".xls");
            // 如果设置为 GetEncoding("GB2312");导出的文件将会出现乱码！！！
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Default;
            HttpContext.Current.Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。 

            System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);

            gv.RenderControl(oHtmlTextWriter);
            HttpContext.Current.Response.Output.Write(oStringWriter.ToString());
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 下载指定的文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        public static void DownFile(HttpContext context, string path, string fileName)
        {
            FileInfo fi = new FileInfo(path);

            HttpContext.Current.Response.Clear();

            // //当要下载的文件名是中文时,需加上HttpUtility.UrlEncode
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            HttpContext.Current.Response.AddHeader("Content-Length", fi.Length.ToString());
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.WriteFile(fi.FullName);
            HttpContext.Current.Response.End();

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// 将DataReader 转为 DataSet
        /// </summary>
        /// <param name="DataReader">DataReader</param>
        public static DataSet ConvertDataReaderToDataSet(SqlDataReader reader)
        {
            DataSet dataSet = new DataSet();
            do
            {
                // Create new data table

                DataTable schemaTable = reader.GetSchemaTable();
                DataTable dataTable = new DataTable();

                if (schemaTable != null)
                {
                    // A query returning records was executed

                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        DataRow dataRow = schemaTable.Rows[i];
                        // Create a column name that is unique in the data table
                        string columnName = (string)dataRow["ColumnName"]; //+ "<C" + i + "/>";
                        // Add the column definition to the data table
                        DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                        dataTable.Columns.Add(column);
                    }

                    dataSet.Tables.Add(dataTable);

                    // Fill the data table we just created

                    while (reader.Read())
                    {
                        DataRow dataRow = dataTable.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dataRow[i] = reader.GetValue(i);

                        dataTable.Rows.Add(dataRow);
                    }
                }
                else
                {
                    // No records were returned

                    DataColumn column = new DataColumn("RowsAffected");
                    dataTable.Columns.Add(column);
                    dataSet.Tables.Add(dataTable);
                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = reader.RecordsAffected;
                    dataTable.Rows.Add(dataRow);
                }
            }
            while (reader.NextResult());
            return dataSet;
        }
    }
}
