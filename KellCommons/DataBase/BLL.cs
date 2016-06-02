using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;

namespace KellCommons.DataBase
{
    public class OleBLL
    {
        //static object LockObject = new object();
        public static bool AddInfo(int typeID, InfoClass info, ProductClass product, Dictionary<int, string> keyvalues)
        {
            string sql = "insert into T_Info (Title, TypeID) values ('" + info.Title + "', " + info.TypeId.ToString() + ")";
            OleDbCommand cmd = OleDAL.Connection.CreateCommand();
            cmd.Transaction = OleDAL.Connection.BeginTransaction(IsolationLevel.RepeatableRead);
            bool insert = OleDAL.ExecuteNonQuery(sql, cmd);
            if (insert)
            {
                int infoID = OleDAL.GetMaxID("ID", "T_Info");
                List<KeyClass> keys = GetKeys(infoID, false);
                foreach (KeyClass key in keys)
                {
                    string sql1 = "insert into T_KeyValue (KeyId, Value) values (@KeyId, @Value)";
                    cmd.Parameters.AddWithValue("@KeyId", key.Id);
                    cmd.Parameters.AddWithValue("@Value", keyvalues[key.Id]);
                    OleDAL.ExecuteNonQuery(sql1, cmd);
                }
                string sql2 = "insert into T_Product (Picture) values (@Picture)";
                cmd.Parameters.AddWithValue("@Picture", GetBytesFromImageList(product.Picture));
                bool insert1 = OleDAL.ExecuteNonQuery(sql2, cmd);
                if (insert1)
                {
                    OleDAL.CommitTransaction(cmd);
                    return true;
                }
            }
            return false;
        }

        public static List<KeyClass> GetKeys(int typeId, bool privated = false)
        {
            List<KeyClass> keys = new List<KeyClass>();
            string parentIds = "";
            if (privated)
            {
                parentIds = typeId.ToString();
            }
            else
            {
                parentIds = GetAllParentIds(typeId);
                if (parentIds != "")
                    parentIds = typeId.ToString() + "," + parentIds;
                else
                    parentIds = typeId.ToString();
            }
            DataTable dt = OleDAL.GetRecords("select * from T_KeyInfo where TypeId in (" + parentIds + ")");
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    keys.Add(new KeyClass((int)dt.Rows[i]["ID"], (int)dt.Rows[i]["TypeId"], dt.Rows[i]["KeyName"].ToString(), dt.Rows[i]["FriendName"].ToString(), dt.Rows[i]["Unit"].ToString()));
                }
            }
            return keys;
        }

        public static KeyClass GetKey(int id)
        {
            DataTable dt = OleDAL.GetRecords("select * from T_KeyInfo where ID=" + id);
            if (dt != null && dt.Rows.Count > 0)
            {
                return new KeyClass((int)dt.Rows[0]["ID"], (int)dt.Rows[0]["TypeId"], dt.Rows[0]["KeyName"].ToString(), dt.Rows[0]["FriendName"].ToString(), dt.Rows[0]["Unit"].ToString());
            }
            return null;
        }

        public static List<ValueClass> GetValues(int infoID)
        {
            List<ValueClass> values = new List<ValueClass>();
            List<KeyClass> keys = GetKeys(GetTypeIdOfInfo(infoID), false);
            if (keys != null && keys.Count > 0)
            {
                foreach (KeyClass key in keys)
                {
                    DataTable dt = OleDAL.GetRecords("select * from T_KeyValue where InfoId=" + infoID + " and KeyId=" + key.Id);
                    ValueClass vc = new ValueClass();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        vc = new ValueClass((int)dt.Rows[0]["ID"], (int)dt.Rows[0]["InfoId"], key.Id, dt.Rows[0]["Value"].ToString());
                    }
                    values.Add(vc);
                }
            }
            return values;
        }

        public static ValueClass GetValue(int infoID, string keyName)
        {
            DataTable dt = OleDAL.GetRecords("select * from T_KeyValue where InfoId=" + infoID + " and KeyId=(select ID from T_KeyInfo where KeyName='" + keyName + "'");
            if (dt != null && dt.Rows.Count > 0)
            {
                return new ValueClass((int)dt.Rows[0]["ID"], (int)dt.Rows[0]["InfoId"], (int)dt.Rows[0]["KeyId"], dt.Rows[0]["Value"].ToString());
            }
            return null;
        }

        public static Dictionary<KeyClass, ValueClass> GetKeyValues(int infoId)
        {
            int typeId = GetTypeIdOfInfo(infoId);
            Dictionary<KeyClass, ValueClass> keyvalues = new Dictionary<KeyClass, ValueClass>();
            string parentIds = GetAllParentIds(typeId);
            if (parentIds != "")
                parentIds = typeId.ToString() + "," + parentIds;
            else
                parentIds = typeId.ToString();
            DataTable key = OleDAL.GetRecords("select * from T_KeyInfo where TypeId in (" + parentIds + ")");
            if (key != null && key.Rows.Count > 0)
            {
                for (int i = 0; i < key.Rows.Count; i++)
                {
                    DataTable value = OleDAL.GetRecords("select * from T_KeyValue where KeyId=" + key.Rows[i]["ID"].ToString());
                    ValueClass vc = new ValueClass();
                    if (value != null && value.Rows.Count > 0)
                    {
                        vc = new ValueClass((int)value.Rows[0]["ID"], (int)value.Rows[0]["InfoId"], (int)value.Rows[0]["KeyId"], value.Rows[0]["Value"].ToString());
                    }
                    keyvalues.Add(new KeyClass((int)key.Rows[i]["ID"], (int)key.Rows[i]["TypeId"], key.Rows[i]["KeyName"].ToString(), key.Rows[i]["FriendName"].ToString(), key.Rows[i]["Unit"].ToString()), vc);
                }
            }
            return keyvalues;
        }

        public static bool AddKeyInfo(int typeId, string keyName, string friendName, string unit)
        {
            //lock (LockObject)
            //{
            if (ExistsKey(typeId, keyName))
                return false;
            string sql = "insert into T_KeyInfo (TypeId, KeyName, FriendName, Unit) values (" + typeId.ToString() + ", '" + keyName + "', '" + friendName + "', '" + unit + "')";
            return OleDAL.ExecuteNonQuery(sql);
            //}
        }

        public static bool ExistsKey(int typeId, string keyName)
        {
            string parentIds = GetAllParentIds(typeId);
            if (parentIds != "")
                parentIds = typeId.ToString() + "," + parentIds;
            else
                parentIds = typeId.ToString();
            string sql = "select * from T_KeyInfo where TypeId in (" + parentIds + ") and KeyName='" + keyName + "'";
            DataTable dt = OleDAL.GetRecords(sql);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        private static string GetAllParentIds(int typeId)
        {
            string typeIdStr = "";
            TypeClass type = GetType(typeId);
            int parentId = type.ParentId;
            while (parentId > 0)
            {
                TypeClass parent = GetType(parentId);
                if (typeIdStr == "")
                    typeIdStr += parent.Id;
                else
                    typeIdStr += "," + parent.Id;
                parentId = parent.ParentId;
            }
            return typeIdStr;
        }

        public static int GetLastKeyId()
        {
            return OleDAL.GetMaxID("ID", "T_KeyInfo");
        }

        public static bool UpdateKeyInfo(int id, string friendName, string unit)
        {
            string sql = "update T_KeyInfo set FriendName='" + friendName + "', Unit='" + unit + "' where ID=" + id;
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool CopyKeyInfoTo(int id, int typeId)
        {
            KeyClass key = GetKey(id);
            if (key.TypeId != typeId)
            {
                if (ExistsKey(typeId, key.KeyName))
                    return false;
                string sql = "insert into T_KeyInfo (TypeId, KeyName, FriendName, Unit) values (" + typeId + ", '" + key.KeyName + "', '" + key.FriendName + "', '" + key.Unit + "')";
                return OleDAL.ExecuteNonQuery(sql);
            }
            else
            {
                return false;
            }
        }

        public static bool MoveKeyInfoTo(int id, int typeId)
        {
            string sql = "update T_KeyInfo set TypeId=" + typeId + " where ID=" + id;
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool DeleteKeyInfo(int id)
        {
            string sql = "delete from T_KeyInfo where ID=" + id;
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool UpdateKeyValue(string set, string where)
        {
            if (!string.IsNullOrEmpty(set))
            {
                string sql = "update T_KeyValue set " + set;
                if (!string.IsNullOrEmpty(where))
                {
                    where = where.Trim().ToLower();
                    if (where.StartsWith("and "))
                        where = where.Substring(4);
                    sql += " where (1=1) and " + where;
                }
                return OleDAL.ExecuteNonQuery(sql);
            }
            else
            {
                throw new Exception("set参数为空，更新失败！");
            }
        }

        public static bool UpdateProduct(OleDbParameter[] set, string where)
        {
            if (set.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (OleDbParameter para in set)
                {
                    if (sb.Length == 0)
                        sb.Append(para.ParameterName + "=@" + para.ParameterName);
                    else
                        sb.Append("," + para.ParameterName + "=@" + para.ParameterName);
                }
                string sql = "update T_Product set " + sb.ToString();
                if (!string.IsNullOrEmpty(where))
                {
                    where = where.Trim().ToLower();
                    if (where.StartsWith("and "))
                        where = where.Substring(4);
                    sql += " where (1=1) and " + where;
                }
                return OleDAL.ExecuteNonQuery(sql, set);
            }
            else
            {
                throw new Exception("set参数为空，更新失败！");
            }
        }

        public static bool UpdateInfoVisible(bool visible, string where)
        {
            string sql = "update T_Info set Visible=" + (visible ? "1" : "0");
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool UpdateInfoType(int typeId, string where)
        {
            string sql = "update T_Info set TypeId=" + typeId.ToString();
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool UpdateInfoTitle(string title, string where)
        {
            string sql = "update T_Info set Title='" + title + "'";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static OleDbParameterCollection CreateParameterCollection(List<KeyValuePair<string, object>> parameters)
        {
            OleDbParameterCollection pc = OleDAL.Connection.CreateCommand().Parameters;
            foreach (KeyValuePair<string, object> para in parameters)
            {
                pc.AddWithValue(para.Key, para.Value);
            }
            return pc;
        }

        public static bool DeleteInfo(string where)
        {
            string sql = "dalete from T_Info";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool AddType(string typeName, string description, int parentID)
        {
            //lock (LockObject)
            //{
            if (ExistsType(parentID, typeName))
                return false;
            string sql = "insert into T_Type (TypeName, Description, ParentID) values ('" + typeName + "', '" + description + "', " + parentID.ToString() + ")";
            return OleDAL.ExecuteNonQuery(sql);
            //}
        }

        private static bool ExistsType(int parentID, string typeName)
        {
            string sql = "select * from T_Type where ParentID=" + parentID.ToString() + " and TypeName='" + typeName + "'";
            DataTable dt = OleDAL.GetRecords(sql);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        public static bool UpdateType(int id, string typeName)
        {
            if (ExistsType(GetParentType(id).Id, typeName))
                return false;
            string sql = "update T_Type set TypeName='" + typeName + "' where Id=" + id;
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool DeleteType(string where)
        {
            string sql = "dalete from T_Type";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static ProductClass GetProduct(int infoID)
        {
            string sql = "select * from T_Product where InfoID = " + infoID.ToString();
            DataTable pi = OleDAL.GetRecords(sql);
            if (pi != null && pi.Rows.Count > 0)
            {
                string typeLink = GetTypeNameOfInfo(infoID);
                List<Image> images = new List<Image>();
                try
                {
                    images = GetImageFromDb((byte[])pi.Rows[0]["Picture"]);
                }
                catch// (Exception e)
                {
                    typeLink += "(图片有损)";
                    //throw e;
                }
                Dictionary<KeyClass, ValueClass> keyvalues = GetKeyValues(infoID);
                ProductClass product = new ProductClass((int)pi.Rows[0]["ID"], infoID, GetInfo(infoID).Title, typeLink, keyvalues, images);
                return product;
            }
            return null;
        }

        public static int GetTypeIdOfInfo(int infoID)
        {
            return GetInfo(infoID).TypeId;
        }

        public static string GetTypeNameOfInfo(int infoID)
        {
            List<string> typeArray = new List<string>();
            TypeClass type = GetType(GetInfo(infoID).TypeId);
            typeArray.Add(type.TypeName);
            int parentId = type.ParentId;
            while (parentId > 0)
            {
                TypeClass parent = GetType(parentId);
                typeArray.Add(parent.TypeName);
                parentId = parent.ParentId;
            }
            typeArray.Reverse();
            return string.Join(".", typeArray.ToArray());
        }

        public static List<InfoClass> GetInfos(string where)
        {
            string sql = "select * from T_Info";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            DataTable infos = OleDAL.GetRecords(sql);
            List<InfoClass> Is = new List<InfoClass>();
            for (int i = 0; i < infos.Rows.Count; i++)
            {
                InfoClass ic = new InfoClass((int)infos.Rows[i]["ID"], infos.Rows[i]["Title"].ToString(), (int)infos.Rows[i]["TypeID"], bool.Parse(infos.Rows[i]["Visible"].ToString()), DateTime.Parse(infos.Rows[i]["SysTime"].ToString()));
                Is.Add(ic);
            }
            return Is;
        }

        public static InfoClass GetInfo(int id)
        {
            string sql = "select * from T_Info where Id=" + id;
            InfoClass info = new InfoClass();
            DataTable infos = OleDAL.GetRecords(sql);
            if (infos != null && infos.Rows.Count > 0)
            {
                info = new InfoClass((int)infos.Rows[0]["ID"], infos.Rows[0]["Title"].ToString(), (int)infos.Rows[0]["TypeID"], bool.Parse(infos.Rows[0]["Visible"].ToString()), DateTime.Parse(infos.Rows[0]["SysTime"].ToString()));
                return info;
            }
            return info;
        }

        public static List<TypeClass> GetTypes(string where)
        {
            string sql = "select * from T_Type";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            DataTable types = OleDAL.GetRecords(sql);
            List<TypeClass> ts = new List<TypeClass>();
            for (int i = 0; i < types.Rows.Count; i++)
            {
                TypeClass tc = new TypeClass((int)types.Rows[i]["ID"], types.Rows[i]["TypeName"].ToString(), types.Rows[i]["Description"].ToString(), (int)types.Rows[i]["ParentID"]);
                ts.Add(tc);
            }
            return ts;
        }

        public static List<TypeClass> GetSubTypes(int parentID)
        {
            string sql = "select * from T_Type where ParentID=" + parentID.ToString();
            DataTable types = OleDAL.GetRecords(sql);
            List<TypeClass> ts = new List<TypeClass>();
            for (int i = 0; i < types.Rows.Count; i++)
            {
                TypeClass tc = new TypeClass((int)types.Rows[i]["ID"], types.Rows[i]["TypeName"].ToString(), types.Rows[i]["Description"].ToString(), (int)types.Rows[i]["ParentID"]);
                ts.Add(tc);
            }
            return ts;
        }

        public static ProductClass GetLastOneProduct()
        {
            ProductClass product = null;
            DataTable last = OleDAL.GetRecords("select top 1 * from T_Product order by ID desc");
            if (last != null && last.Rows.Count > 0)
            {
                int infoID = (int)last.Rows[0]["InfoID"];
                string typeLink = GetTypeNameOfInfo(infoID);
                List<Image> images = new List<Image>();
                try
                {
                    images = GetImageFromDb((byte[])last.Rows[0]["Picture"]);
                }
                catch// (Exception e)
                {
                    typeLink += "(图片有损)";
                    //throw e;
                }
                Dictionary<KeyClass, ValueClass> keyvalues = GetKeyValues(infoID);
                product = new ProductClass((int)last.Rows[0]["ID"], infoID, GetInfo(infoID).Title, typeLink, keyvalues, images);
            }
            return product;
        }

        private static List<Image> GetImageFromDb(byte[] pic)
        {
            List<Image> img = new List<Image>();
            try
            {
                using (MemoryStream ms = new MemoryStream(pic))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    img = (List<Image>)bf.Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                throw new Exception("数据库中的图片数据不合法！\n" + e.Message);
            }
            return img;
        }

        public static byte[] GetBytesFromImageList(List<Image> pic)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, pic);
                data = ms.ToArray();
            }
            return data;
        }

        public static int GetLastTypeId()
        {
            return OleDAL.GetMaxID("ID", "T_Type");
        }

        public static TypeClass GetType(int id)
        {
            string sql = "select * from T_Type where ID = " + id.ToString();
            DataTable ti = OleDAL.GetRecords(sql);
            if (ti != null && ti.Rows.Count > 0)
            {
                TypeClass type = new TypeClass(id, ti.Rows[0]["TypeName"].ToString(), ti.Rows[0]["Description"].ToString(), (int)ti.Rows[0]["ParentID"]);
                return type;
            }
            return null;
        }

        public static TypeClass GetParentType(int id)
        {
            return GetType(GetType(id).ParentId);
        }

        public static TypeClass GetRootType(int id)
        {
            TypeClass parent = GetType(id);
            int parentId = parent.ParentId;
            while (parentId > 0)
            {
                parent = GetType(parentId);
                parentId = parent.ParentId;
            }
            return parent;
        }
    }


    public class SqlBLL
    {
        //static object LockObject = new object();
        public static bool AddInfo(int typeID, InfoClass info, ProductClass product, Dictionary<int, string> keyvalues)
        {
            string sql = "insert into T_Info (Title, TypeID) values ('" + info.Title + "', " + info.TypeId.ToString() + ")";
            SqlCommand cmd = SqlDAL.Connection.CreateCommand();
            cmd.Transaction = SqlDAL.Connection.BeginTransaction(IsolationLevel.RepeatableRead);
            bool insert = SqlDAL.ExecuteNonQuery(sql, cmd);
            if (insert)
            {
                int infoID = SqlDAL.GetMaxID("ID", "T_Info");
                List<KeyClass> keys = GetKeys(infoID, false);
                foreach (KeyClass key in keys)
                {
                    string sql1 = "insert into T_KeyValue (KeyId, Value) values (@KeyId, @Value)";
                    cmd.Parameters.AddWithValue("@KeyId", key.Id);
                    cmd.Parameters.AddWithValue("@Value", keyvalues[key.Id]);
                    SqlDAL.ExecuteNonQuery(sql1, cmd);
                }
                string sql2 = "insert into T_Product (Picture) values (@Picture)";
                cmd.Parameters.AddWithValue("@Picture", GetBytesFromImageList(product.Picture));
                bool insert1 = SqlDAL.ExecuteNonQuery(sql2, cmd);
                if (insert1)
                {
                    SqlDAL.CommitTransaction(cmd);
                    return true;
                }
            }
            return false;
        }

        public static List<KeyClass> GetKeys(int typeId, bool privated = false)
        {
            List<KeyClass> keys = new List<KeyClass>();
            string parentIds = "";
            if (privated)
            {
                parentIds = typeId.ToString();
            }
            else
            {
                parentIds = GetAllParentIds(typeId);
                if (parentIds != "")
                    parentIds = typeId.ToString() + "," + parentIds;
                else
                    parentIds = typeId.ToString();
            }
            DataTable dt = SqlDAL.GetRecords("select * from T_KeyInfo where TypeId in (" + parentIds + ")");
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    keys.Add(new KeyClass((int)dt.Rows[i]["ID"], (int)dt.Rows[i]["TypeId"], dt.Rows[i]["KeyName"].ToString(), dt.Rows[i]["FriendName"].ToString(), dt.Rows[i]["Unit"].ToString()));
                }
            }
            return keys;
        }

        public static KeyClass GetKey(int id)
        {
            DataTable dt = SqlDAL.GetRecords("select * from T_KeyInfo where ID=" + id);
            if (dt != null && dt.Rows.Count > 0)
            {
                return new KeyClass((int)dt.Rows[0]["ID"], (int)dt.Rows[0]["TypeId"], dt.Rows[0]["KeyName"].ToString(), dt.Rows[0]["FriendName"].ToString(), dt.Rows[0]["Unit"].ToString());
            }
            return null;
        }

        public static List<ValueClass> GetValues(int infoID)
        {
            List<ValueClass> values = new List<ValueClass>();
            List<KeyClass> keys = GetKeys(GetTypeIdOfInfo(infoID), false);
            if (keys != null && keys.Count > 0)
            {
                foreach (KeyClass key in keys)
                {
                    DataTable dt = SqlDAL.GetRecords("select * from T_KeyValue where InfoId=" + infoID + " and KeyId=" + key.Id);
                    ValueClass vc = new ValueClass();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        vc = new ValueClass((int)dt.Rows[0]["ID"], (int)dt.Rows[0]["InfoId"], key.Id, dt.Rows[0]["Value"].ToString());
                    }
                    values.Add(vc);
                }
            }
            return values;
        }

        public static ValueClass GetValue(int infoID, string keyName)
        {
            DataTable dt = SqlDAL.GetRecords("select * from T_KeyValue where InfoId=" + infoID + " and KeyId=(select ID from T_KeyInfo where KeyName='" + keyName + "'");
            if (dt != null && dt.Rows.Count > 0)
            {
                return new ValueClass((int)dt.Rows[0]["ID"], (int)dt.Rows[0]["InfoId"], (int)dt.Rows[0]["KeyId"], dt.Rows[0]["Value"].ToString());
            }
            return null;
        }

        public static Dictionary<KeyClass, ValueClass> GetKeyValues(int infoId)
        {
            int typeId = GetTypeIdOfInfo(infoId);
            Dictionary<KeyClass, ValueClass> keyvalues = new Dictionary<KeyClass, ValueClass>();
            string parentIds = GetAllParentIds(typeId);
            if (parentIds != "")
                parentIds = typeId.ToString() + "," + parentIds;
            else
                parentIds = typeId.ToString();
            DataTable key = SqlDAL.GetRecords("select * from T_KeyInfo where TypeId in (" + parentIds + ")");
            if (key != null && key.Rows.Count > 0)
            {
                for (int i = 0; i < key.Rows.Count; i++)
                {
                    DataTable value = SqlDAL.GetRecords("select * from T_KeyValue where KeyId=" + key.Rows[i]["ID"].ToString());
                    ValueClass vc = new ValueClass();
                    if (value != null && value.Rows.Count > 0)
                    {
                        vc = new ValueClass((int)value.Rows[0]["ID"], (int)value.Rows[0]["InfoId"], (int)value.Rows[0]["KeyId"], value.Rows[0]["Value"].ToString());
                    }
                    keyvalues.Add(new KeyClass((int)key.Rows[i]["ID"], (int)key.Rows[i]["TypeId"], key.Rows[i]["KeyName"].ToString(), key.Rows[i]["FriendName"].ToString(), key.Rows[i]["Unit"].ToString()), vc);
                }
            }
            return keyvalues;
        }

        public static bool AddKeyInfo(int typeId, string keyName, string friendName, string unit)
        {
            //lock (LockObject)
            //{
            if (ExistsKey(typeId, keyName))
                return false;
            string sql = "insert into T_KeyInfo (TypeId, KeyName, FriendName, Unit) values (" + typeId.ToString() + ", '" + keyName + "', '" + friendName + "', '" + unit + "')";
            return SqlDAL.ExecuteNonQuery(sql);
            //}
        }

        public static bool ExistsKey(int typeId, string keyName)
        {
            string parentIds = GetAllParentIds(typeId);
            if (parentIds != "")
                parentIds = typeId.ToString() + "," + parentIds;
            else
                parentIds = typeId.ToString();
            string sql = "select * from T_KeyInfo where TypeId in (" + parentIds + ") and KeyName='" + keyName + "'";
            DataTable dt = SqlDAL.GetRecords(sql);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        private static string GetAllParentIds(int typeId)
        {
            string typeIdStr = "";
            TypeClass type = GetType(typeId);
            int parentId = type.ParentId;
            while (parentId > 0)
            {
                TypeClass parent = GetType(parentId);
                if (typeIdStr == "")
                    typeIdStr += parent.Id;
                else
                    typeIdStr += "," + parent.Id;
                parentId = parent.ParentId;
            }
            return typeIdStr;
        }

        public static int GetLastKeyId()
        {
            return SqlDAL.GetMaxID("ID", "T_KeyInfo");
        }

        public static bool UpdateKeyInfo(int id, string friendName, string unit)
        {
            string sql = "update T_KeyInfo set FriendName='" + friendName + "', Unit='" + unit + "' where ID=" + id;
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static bool CopyKeyInfoTo(int id, int typeId)
        {
            KeyClass key = GetKey(id);
            if (key.TypeId != typeId)
            {
                if (ExistsKey(typeId, key.KeyName))
                    return false;
                string sql = "insert into T_KeyInfo (TypeId, KeyName, FriendName, Unit) values (" + typeId + ", '" + key.KeyName + "', '" + key.FriendName + "', '" + key.Unit + "')";
                return SqlDAL.ExecuteNonQuery(sql);
            }
            else
            {
                return false;
            }
        }

        public static bool MoveKeyInfoTo(int id, int typeId)
        {
            string sql = "update T_KeyInfo set TypeId=" + typeId + " where ID=" + id;
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static bool DeleteKeyInfo(int id)
        {
            string sql = "delete from T_KeyInfo where ID=" + id;
            return OleDAL.ExecuteNonQuery(sql);
        }

        public static bool UpdateKeyValue(string set, string where)
        {
            if (!string.IsNullOrEmpty(set))
            {
                string sql = "update T_KeyValue set " + set;
                if (!string.IsNullOrEmpty(where))
                {
                    where = where.Trim().ToLower();
                    if (where.StartsWith("and "))
                        where = where.Substring(4);
                    sql += " where (1=1) and " + where;
                }
                return SqlDAL.ExecuteNonQuery(sql);
            }
            else
            {
                throw new Exception("set参数为空，更新失败！");
            }
        }

        public static bool UpdateProduct(SqlParameter[] set, string where)
        {
            if (set.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (SqlParameter para in set)
                {
                    if (sb.Length == 0)
                        sb.Append(para.ParameterName + "=@" + para.ParameterName);
                    else
                        sb.Append("," + para.ParameterName + "=@" + para.ParameterName);
                }
                string sql = "update T_Product set " + sb.ToString();
                if (!string.IsNullOrEmpty(where))
                {
                    where = where.Trim().ToLower();
                    if (where.StartsWith("and "))
                        where = where.Substring(4);
                    sql += " where (1=1) and " + where;
                }
                return SqlDAL.ExecuteNonQuery(sql, set);
            }
            else
            {
                throw new Exception("set参数为空，更新失败！");
            }
        }

        public static bool UpdateInfoVisible(bool visible, string where)
        {
            string sql = "update T_Info set Visible=" + (visible ? "1" : "0");
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static bool UpdateInfoType(int typeId, string where)
        {
            string sql = "update T_Info set TypeId=" + typeId.ToString();
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static bool UpdateInfoTitle(string title, string where)
        {
            string sql = "update T_Info set Title='" + title + "'";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static SqlParameterCollection CreateParameterCollection(List<KeyValuePair<string, object>> parameters)
        {
            SqlParameterCollection pc = SqlDAL.Connection.CreateCommand().Parameters;
            foreach (KeyValuePair<string, object> para in parameters)
            {
                pc.AddWithValue(para.Key, para.Value);
            }
            return pc;
        }

        public static bool DeleteInfo(string where)
        {
            string sql = "dalete from T_Info";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static bool AddType(string typeName, string description, int parentID)
        {
            //lock (LockObject)
            //{
            if (ExistsType(parentID, typeName))
                return false;
            string sql = "insert into T_Type (TypeName, Description, ParentID) values ('" + typeName + "', '" + description + "', " + parentID.ToString() + ")";
            return SqlDAL.ExecuteNonQuery(sql);
            //}
        }

        private static bool ExistsType(int parentID, string typeName)
        {
            string sql = "select * from T_Type where ParentID=" + parentID.ToString() + " and TypeName='" + typeName + "'";
            DataTable dt = SqlDAL.GetRecords(sql);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        public static bool UpdateType(int id, string typeName)
        {
            if (ExistsType(GetParentType(id).Id, typeName))
                return false;
            string sql = "update T_Type set TypeName='" + typeName + "' where Id=" + id;
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static bool DeleteType(string where)
        {
            string sql = "dalete from T_Type";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            return SqlDAL.ExecuteNonQuery(sql);
        }

        public static ProductClass GetProduct(int infoID)
        {
            string sql = "select * from T_Product where InfoID = " + infoID.ToString();
            DataTable pi = SqlDAL.GetRecords(sql);
            if (pi != null && pi.Rows.Count > 0)
            {
                string typeLink = GetTypeNameOfInfo(infoID);
                List<Image> images = new List<Image>();
                try
                {
                    images = GetImageFromDb((byte[])pi.Rows[0]["Picture"]);
                }
                catch// (Exception e)
                {
                    typeLink += "(图片有损)";
                    //throw e;
                }
                Dictionary<KeyClass, ValueClass> keyvalues = GetKeyValues(infoID);
                ProductClass product = new ProductClass((int)pi.Rows[0]["ID"], infoID, GetInfo(infoID).Title, typeLink, keyvalues, images);
                return product;
            }
            return null;
        }

        public static int GetTypeIdOfInfo(int infoID)
        {
            return GetInfo(infoID).TypeId;
        }

        public static string GetTypeNameOfInfo(int infoID)
        {
            List<string> typeArray = new List<string>();
            TypeClass type = GetType(GetInfo(infoID).TypeId);
            typeArray.Add(type.TypeName);
            int parentId = type.ParentId;
            while (parentId > 0)
            {
                TypeClass parent = GetType(parentId);
                typeArray.Add(parent.TypeName);
                parentId = parent.ParentId;
            }
            typeArray.Reverse();
            return string.Join(".", typeArray.ToArray());
        }

        public static List<InfoClass> GetInfos(string where)
        {
            string sql = "select * from T_Info";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            DataTable infos = SqlDAL.GetRecords(sql);
            List<InfoClass> Is = new List<InfoClass>();
            for (int i = 0; i < infos.Rows.Count; i++)
            {
                InfoClass ic = new InfoClass((int)infos.Rows[i]["ID"], infos.Rows[i]["Title"].ToString(), (int)infos.Rows[i]["TypeID"], bool.Parse(infos.Rows[i]["Visible"].ToString()), DateTime.Parse(infos.Rows[i]["SysTime"].ToString()));
                Is.Add(ic);
            }
            return Is;
        }

        public static InfoClass GetInfo(int id)
        {
            string sql = "select * from T_Info where Id=" + id;
            InfoClass info = new InfoClass();
            DataTable infos = SqlDAL.GetRecords(sql);
            if (infos != null && infos.Rows.Count > 0)
            {
                info = new InfoClass((int)infos.Rows[0]["ID"], infos.Rows[0]["Title"].ToString(), (int)infos.Rows[0]["TypeID"], bool.Parse(infos.Rows[0]["Visible"].ToString()), DateTime.Parse(infos.Rows[0]["SysTime"].ToString()));
                return info;
            }
            return info;
        }

        public static List<TypeClass> GetTypes(string where)
        {
            string sql = "select * from T_Type";
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Trim().ToLower();
                if (where.StartsWith("and "))
                    where = where.Substring(4);
                sql += " where (1=1) and " + where;
            }
            DataTable types = SqlDAL.GetRecords(sql);
            List<TypeClass> ts = new List<TypeClass>();
            for (int i = 0; i < types.Rows.Count; i++)
            {
                TypeClass tc = new TypeClass((int)types.Rows[i]["ID"], types.Rows[i]["TypeName"].ToString(), types.Rows[i]["Description"].ToString(), (int)types.Rows[i]["ParentID"]);
                ts.Add(tc);
            }
            return ts;
        }

        public static List<TypeClass> GetSubTypes(int parentID)
        {
            string sql = "select * from T_Type where ParentID=" + parentID.ToString();
            DataTable types = SqlDAL.GetRecords(sql);
            List<TypeClass> ts = new List<TypeClass>();
            for (int i = 0; i < types.Rows.Count; i++)
            {
                TypeClass tc = new TypeClass((int)types.Rows[i]["ID"], types.Rows[i]["TypeName"].ToString(), types.Rows[i]["Description"].ToString(), (int)types.Rows[i]["ParentID"]);
                ts.Add(tc);
            }
            return ts;
        }

        public static ProductClass GetLastOneProduct()
        {
            ProductClass product = null;
            DataTable last = SqlDAL.GetRecords("select top 1 * from T_Product order by ID desc");
            if (last != null && last.Rows.Count > 0)
            {
                int infoID = (int)last.Rows[0]["InfoID"];
                string typeLink = GetTypeNameOfInfo(infoID);
                List<Image> images = new List<Image>();
                try
                {
                    images = GetImageFromDb((byte[])last.Rows[0]["Picture"]);
                }
                catch// (Exception e)
                {
                    typeLink += "(图片有损)";
                    //throw e;
                }
                Dictionary<KeyClass, ValueClass> keyvalues = GetKeyValues(infoID);
                product = new ProductClass((int)last.Rows[0]["ID"], infoID, GetInfo(infoID).Title, typeLink, keyvalues, images);
            }
            return product;
        }

        private static List<Image> GetImageFromDb(byte[] pic)
        {
            List<Image> img = new List<Image>();
            try
            {
                using (MemoryStream ms = new MemoryStream(pic))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    img = (List<Image>)bf.Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                throw new Exception("数据库中的图片数据不合法！\n" + e.Message);
            }
            return img;
        }

        public static byte[] GetBytesFromImageList(List<Image> pic)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, pic);
                data = ms.ToArray();
            }
            return data;
        }

        public static int GetLastTypeId()
        {
            return SqlDAL.GetMaxID("ID", "T_Type");
        }

        public static TypeClass GetType(int id)
        {
            string sql = "select * from T_Type where ID = " + id.ToString();
            DataTable ti = SqlDAL.GetRecords(sql);
            if (ti != null && ti.Rows.Count > 0)
            {
                TypeClass type = new TypeClass(id, ti.Rows[0]["TypeName"].ToString(), ti.Rows[0]["Description"].ToString(), (int)ti.Rows[0]["ParentID"]);
                return type;
            }
            return null;
        }

        public static TypeClass GetParentType(int id)
        {
            return GetType(GetType(id).ParentId);
        }

        public static TypeClass GetRootType(int id)
        {
            TypeClass parent = GetType(id);
            int parentId = parent.ParentId;
            while (parentId > 0)
            {
                parent = GetType(parentId);
                parentId = parent.ParentId;
            }
            return parent;
        }
    }
}
