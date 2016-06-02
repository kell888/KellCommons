using System;
using System.Collections.Generic;
using System.Data;
using DBOperation;

namespace KellCommons
{
    /*
    /// <summary>
    /// 荆州短信发送类
    /// </summary>
    public class SMS
    {
        public class MessageState
        {
            string messageId;
            string information;
            public string MessageId
            {
                get { return messageId; }
                internal set { messageId = value; }
            }
            public string Information
            {
                get { return information; }
                internal set { information = value; }
            }
        }

        public SMS()
        {
            service = new WebReference.MessageServiceService();
            state = new MessageState();
        }

        static string Username = ConfigurationManager.AppSettings["smsUserName"];
        static string Password = ConfigurationManager.AppSettings["smsPassword"];
        WebReference.MessageServiceService service;
        MessageState state;
        int type;
        int priority;

        public int PRIORITY
        {
            get { return priority; }
            set { priority = value; }
        }

        public int TYPE
        {
            get { return type; }
            set { type = value; }
        }

        public MessageState State
        {
            get { return state; }
        }

        public string GetMsgState(string msgStateString)
        {
            string friendState = "";
            switch (msgStateString.ToLower())
            {
                case "success":
                    friendState = "已经发送";
                    break;
                case "wait_send":
                    friendState = "等待发送";
                    break;
                case "no_exist":
                    friendState = "短信不存在";
                    break;
            }
            return friendState;
        }

        public string GetUserMoney()
        {
            try
            {
                string money = service.getUserMoney(Username, Password).ToLower();
                if (money.Equals("username_or_password_error"))
                    return "用户名密码错误";
                else
                    return Convert.ToString(int.Parse(money) / 100) + "元" + Convert.ToString(int.Parse(money) % 100) + "分";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 同步发送短信
        /// </summary>
        /// <param name="userCodes"></param>
        /// <param name="content"></param>
        /// <param name="sendTime">时间格式:yyyy-mm-dd hh:mm:ss 空为立即发送</param>
        /// <param name="isSignName"></param>
        /// <param name="msgState"></param>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public bool SendMessageByTelNo(List<string> telNos, string content, string sendTime, string signName, out string msgState, out string errorInfo)
        {
            msgState = "";
            errorInfo = "";
            if (telNos.Count > 0)
            {
                if (string.IsNullOrEmpty(content))
                {
                    errorInfo = "短信内容为空";
                    return false;
                }
                try
                {
                    string smsReturnInfo = service.sendMessage(Username, Password, sendTime, string.Join(",", telNos.ToArray()), content + " -- " + signName);
                    bool sended = false;
                    if (smsReturnInfo.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string msgid = smsReturnInfo.Substring(smsReturnInfo.IndexOf(",") + 1).Trim();
                        msgState = GetMsgState(service.getMessageState(msgid));
                        sended = true;
                    }
                    else
                    {
                        switch (smsReturnInfo.ToLower())
                        {
                            case "params_not_null":
                                errorInfo = "参数不能为空 时间除外";
                                break;
                            case "username_or_password_error":
                                errorInfo = "用户名或者密码错误";
                                break;
                            case "system_error":
                                errorInfo = "系统错误";
                                break;
                            case "time_error":
                                errorInfo = "时间格式错误";
                                break;
                            case "content_length_error":
                                errorInfo = "短信内容过长 超过200";
                                break;
                            case "content_black_keywords":
                                errorInfo = "短信内容含有非法关键字";
                                break;
                            case "calledNumber_error":
                                errorInfo = "被叫号码格式错误";
                                break;
                            case "money_not_toomuch":
                                errorInfo = "资金不足";
                                break;
                        }
                    }
                    return sended;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    service.Dispose();
                }
            }
            else
            {
                errorInfo = "被叫号码为空";
            }
            return false;
        }

        /// <summary>
        /// 异步发送短信
        /// </summary>
        /// <param name="userCodes"></param>
        /// <param name="content"></param>
        /// <param name="sendTime">时间格式:yyyy-mm-dd hh:mm:ss 空为立即发送</param>
        /// <param name="isSignName"></param>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public bool SendMessageAsyncByTelNo(List<string> telNos, string content, string sendTime, string signName, out string errorInfo)
        {
            errorInfo = "";
            if (telNos.Count > 0)
            {
                if (string.IsNullOrEmpty(content))
                {
                    errorInfo = "短信内容为空";
                    return false;
                }
                try
                {
                    service.getMessageStateCompleted += new WebReference.getMessageStateCompletedEventHandler(service_getMessageStateCompleted);
                    service.getUserMoneyCompleted += new WebReference.getUserMoneyCompletedEventHandler(service_getUserMoneyCompleted);
                    service.sendMessageCompleted += new WebReference.sendMessageCompletedEventHandler(service_sendMessageCompleted);
                    service.sendMessageAsync(Username, Password, sendTime, string.Join(",", telNos.ToArray()), content + " -- " + signName, state);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                errorInfo = "被叫号码为空";
            }
            return false;
        }

        /// <summary>
        /// 取消异步发送
        /// </summary>
        /// <returns></returns>
        public bool CancelSend()
        {
            try
            {
                service.CancelAsync(state);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        void service_sendMessageCompleted(object sender, WebReference.sendMessageCompletedEventArgs e)
        {
            WebReference.MessageServiceService service = (WebReference.MessageServiceService)sender;
            string msgid = "";
            string info = "";
            if (e.Cancelled)
                info = "取消。";
            if (e.Error != null)
                info += "错误：（" + e.Error.Message + "）";
            MessageState state = (MessageState)e.UserState;
            state.MessageId = msgid;
            state.Information = "发送完毕[发送结果：" + e.Result + info + "]";
            service.getMessageStateAsync(msgid, state);
        }

        void service_getUserMoneyCompleted(object sender, WebReference.getUserMoneyCompletedEventArgs e)
        {
            WebReference.MessageServiceService service = (WebReference.MessageServiceService)sender;
            string msgid = "";
            string info = "";
            if (e.Cancelled)
                info = "取消。";
            if (e.Error != null)
                info += "错误：（" + e.Error.Message + "）";
            MessageState state = (MessageState)e.UserState;
            state.MessageId = msgid;
            state.Information = "获取余额完毕[获取结果：" + e.Result + info + "]";
            service.getMessageStateAsync(msgid, state);
        }

        void service_getMessageStateCompleted(object sender, WebReference.getMessageStateCompletedEventArgs e)
        {
            WebReference.MessageServiceService service = (WebReference.MessageServiceService)sender;
            string msgid = "";
            string info = "";
            if (e.Cancelled)
                info = "取消。";
            if (e.Error != null)
                info += "错误：（" + e.Error.Message + "）";
            MessageState state = (MessageState)e.UserState;
            state.MessageId = msgid;
            state.Information = "获取发送状态完毕[获取结果：" + e.Result + info + "]";
            service.getMessageStateAsync(msgid, state);
        }

        public static bool Sendmessage(string SEQ_NUM, string MOBILE, string CONTENT, string SENDTIME, string PRIORITY, string EXTEND_CODE, out string result)
        {
            result = "";
            Encoding encoding = Encoding.GetEncoding("GBK");

            string requestData = "<?xml version=\"1.0\" encoding=\"GBK\" ?>" +
                    "<REQUEST><TRANS_TYPE>SMS_DOWN_REQUEST</TRANS_TYPE>" +
                    "<SP_ID>"+SP_ID+"</SP_ID>" +
                    "<PASSWORD>"+PASSWORD+"</PASSWORD>" +				
                    "<SEQ_NUM>"+SEQ_NUM+"</SEQ_NUM><MOBILE>"+MOBILE+"</MOBILE>" +
                    "<CONTENT><![CDATA["+CONTENT+"]]></CONTENT>" +
                    "<DATETIME>"+SENDTIME+"</DATETIME>" +
                    "<PRIORITY>"+PRIORITY+"</PRIORITY><EXTEND_CODE>"+EXTEND_CODE+"</EXTEND_CODE></REQUEST>";
		
            WebClient client = new WebClient();
            try
            {
                client.Encoding = encoding;
                result = client.UploadString(smsURL, "POST", requestData);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("向远程短信服务器提交请求数据失败: " + e.Message);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
    */
    /// <summary>
    /// 舜天短信平台发送类
    /// </summary>
    public class STSMS
    {
        RemoteSqlProcesser rsp;
        public STSMS()
        {
            rsp = new RemoteSqlProcesser();
        }

        /// <summary>
        /// 同步单个发送短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="content"></param>
        /// <param name="type">系统短信为0，群发为1</param>
        /// <param name="SCODE"></param>
        /// <param name="TIMING"></param>
        /// <param name="SENDTIME"></param>
        /// <param name="signName"></param>
        /// <param name="errorinfo">异常信息</param>
        /// <returns></returns>
        public bool Send(string mobile, string content, bool TIMING, DateTime SENDTIME, string signName, out string errorinfo)
        {
            bool flag = false;
            errorinfo = "";
            if (!string.IsNullOrEmpty(mobile))
            {
                try
                {
                    string TABLE = "SMS_BILL_TIME";
                    string smsContent = content + " -- " + signName;
                    List<string> contents = new List<string>();
                    int i = 0;
                    while (i < smsContent.Length)
                    {
                        if (smsContent.Substring(i).Length >= 70)
                            contents.Add(smsContent.Substring(i, 70));
                        else
                            contents.Add(smsContent.Substring(i));
                        i += 70;
                    }
                    int count = 0;
                    foreach (string sms in contents)
                    {
                        count++;
                        DateTime sendT = DateTime.Now;
                        if (TIMING)
                        {
                            sendT = SENDTIME;
                        }
                        string insert = "insert into " + TABLE + " (TO_MOBILE,MSG_CONTENT,SEND_TIME,user_name) values ('" + mobile + "','" + sms + "','" + sendT + "','KELL')";
                        if (rsp.WriteDBBySingleSql(insert))
                        {
                            flag = true;
                        }
                        else
                        {
                            errorinfo += " 第" + count + "/" + contents.Count + "条短信异常：无法插入到远程短信服务器，发送失败！";
                        }
                    }
                }
                catch (Exception e)
                {
                    errorinfo += " 发送异常：" + e.Message;
                    throw e;
                }
            }
            else
            {
                errorinfo = "电话号码为空，发送失败！";
            }
            return flag;
        }

        /// <summary>
        /// 同步批量发送短信
        /// </summary>
        /// <param name="mobiles"></param>
        /// <param name="content"></param>
        /// <param name="type">系统短信为0，群发为1</param>
        /// <param name="SCODE"></param>
        /// <param name="TIMING"></param>
        /// <param name="SENDTIME"></param>
        /// <param name="signName"></param>
        /// <param name="errorinfo">异常信息</param>
        /// <returns></returns>
        public bool Send(List<string> mobiles, string content, bool TIMING, DateTime SENDTIME, string signName, out string errorinfo)
        {
            bool flag = false;
            errorinfo = "";
            if (mobiles != null && mobiles.Count > 0)
            {
                try
                {
                    string TABLE = "SMS_BILL_TIME";
                    string smsContent = content + "  " + signName;
                    List<string> contents = new List<string>();
                    int i = 0;
                    for (i = smsContent.Length; i > 0; i = i + 70)
                    {
                        if (smsContent.Substring(smsContent.Length - i).Length >= 70)
                            contents.Add(smsContent.Substring(smsContent.Length - i, 70) + "--[续]");
                        else
                            contents.Add(smsContent.Substring(smsContent.Length - i));
                    }
                    foreach (string mobile in mobiles)
                    {
                        int count = 0;
                        foreach (string sms in contents)
                        {
                            count++;
                            DateTime sendT = DateTime.Now;
                            if (TIMING)
                            {
                                sendT = SENDTIME;
                            }
                            string insert = "insert into " + TABLE + " (TO_MOBILE,MSG_CONTENT,SEND_TIME,user_name) values ('" + mobile + "','" + sms + "','" + sendT + "','KELL')";
                            if (rsp.WriteDBBySingleSql(insert))
                            {
                                flag = true;
                            }
                            else
                            {
                                errorinfo += " 第" + count + "/" + contents.Count + "条短信[" + mobile + "]异常：无法插入到远程短信服务器，发送失败！";
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    errorinfo += " 发送异常：" + e.Message;
                    throw e;
                }
            }
            else
            {
                errorinfo = "电话号码为空，发送失败！";
            }
            return flag;
        }

        /// <summary>
        /// 处理远程数据操作
        /// </summary>
        public class RemoteSqlProcesser
        {
            //数据库类型初始化
            private DataBaseInfo info;

            //数据库操作接口初始化
            private IDBOperateService service;

            /// <summary>
            /// 初始化
            /// </summary>
            public RemoteSqlProcesser()
            {
                info = new DataBaseInfo(KellCommons.KellCommon.ReadAppSetting("connectString1"), DBConnType.SqlServer);
                service = new DBOperateService(info);
            }

            /// <summary>
            /// 单一SQL语句获取数据表
            /// </summary>
            /// <param name="sql"></param>
            /// <returns></returns>
            public DataTable GetDTBySingleSql(string sql)
            {
                //定义一个DataTable实例
                DataTable result = new DataTable();

                //定义SQL语句集
                SqlDataCollection sqlData = new SqlDataCollection();

                sqlData.Add(new SqlData(sql));
                try
                {
                    if (service.Excute(result, sqlData, DBOperationFashion.Select))
                        return result;
                    else
                        return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            /// <summary>
            /// 单一SQL语句获取数据表
            /// </summary>
            /// <param name="sql"></param>
            /// <param name="para"></param>
            /// <returns></returns>
            public DataTable GetDTBySingleSql(string sql, ParaCollection para)
            {
                //定义一个DataTable实例
                DataTable result = new DataTable();

                //定义SQL语句集
                SqlDataCollection sqlData = new SqlDataCollection();

                sqlData.Add(new SqlData(sql, para));
                try
                {
                    if (service.Excute(result, sqlData, DBOperationFashion.Select))
                    {
                        int row = result.Rows.Count;
                        return result;
                    }
                    else
                        return null;

                }
                catch (Exception)
                {
                    return null;
                }
            }


            /// <summary>
            /// 单一SQL语句数据库写操作
            /// </summary>
            /// <param name="sql"></param>
            /// <returns></returns>
            public bool WriteDBBySingleSql(string sql)
            {
                //定义SQL语句集
                SqlDataCollection sqlData = new SqlDataCollection();

                sqlData.Add(new SqlData(sql));

                try
                {
                    return service.Excute(new DataTable(), sqlData, DBOperationFashion.Other);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// 单一SQL语句数据库写操作
            /// </summary>
            /// <param name="sql"></param>
            /// <param name="para"></param>
            /// <returns></returns>
            public bool WriteDBBySingleSql(string sql, ParaCollection para)
            {
                //定义SQL语句集
                SqlDataCollection sqlData = new SqlDataCollection();

                sqlData.Add(new SqlData(sql, para));

                try
                {
                    return service.Excute(new DataTable(), sqlData, DBOperationFashion.Other);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// 多个SQL语句数据库写操作
            /// </summary>
            /// <param name="sqlDatas"></param>
            /// <returns></returns>
            public bool WriteDBByMultiSql(List<SqlData> sqlDatas)
            {
                //定义SQL语句集
                SqlDataCollection sqlDataCes = new SqlDataCollection();

                foreach (SqlData sd in sqlDatas)
                    sqlDataCes.Add(sd);
                try
                {
                    return service.Excute(new DataTable(), sqlDataCes, DBOperationFashion.Other);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}