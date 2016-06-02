using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace KellCommons
{
    /// <summary>
    /// 服务器请求处理类
    /// </summary>
    public class WebRequestUtil
    {
        /// <summary>
        /// 判断当前页面是否接收到了Post请求
        /// </summary>
        /// <returns>接收到了Post请求为真(true),否则为false</returns>
        public static bool IsPost()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("POST");
        }

        /// <summary>
        /// 判断当前页面是否接收到了Get请求
        /// </summary>
        /// <returns>接收到了Get请求为真(true),否则为false</returns>
        public static bool IsGet()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("GET");
        }

        /// <summary>
        /// 返回指定服务器变量名的信息
        /// </summary>
        /// <param name="strName">服务器变量名</param>
        /// <returns>服务器变量名的信息</returns>
        public static string GetServerVariablesString(string strName)
        {
            if (HttpContext.Current.Request.ServerVariables[strName] == null)
            {
                return string.Empty;
            }

            return HttpContext.Current.Request.ServerVariables[strName].ToString();
        }

        /// <summary>
        /// 返回上一个页面的地址
        /// </summary>
        /// <returns>上一个页面的地址</returns>
        public static string GetUrlReferrer()
        {
            string retValue = string.Empty;
            try
            {
                retValue = HttpContext.Current.Request.UrlReferrer.ToString();
            }
            catch { }

            return retValue;
        }

        /// <summary>
        /// 返回当前完整的主机:端口号
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFullHost()
        {
            HttpRequest request = HttpContext.Current.Request;
            if (!request.Url.IsDefaultPort)
            {
                return string.Format("{0}:{1}", request.Url.Host, request.Url.Port.ToString());
            }

            return request.Url.Host;
        }

        /// <summary>
        /// 返回主机
        /// </summary>
        /// <returns></returns>
        public static string GetHost()
        {
            return HttpContext.Current.Request.Url.Host;
        }

        /// <summary>
        /// 获取当前请求的原始 URL(URL 中域信息之后的部分,包括查询字符串(如果存在))
        /// </summary>
        /// <returns>原始 URL</returns>
        public static string GetRawUrl()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        /// <summary>
        /// 判断当前访问是否来自浏览器访问
        /// </summary>
        /// <returns>如果是返回为真,否则为假</returns>
        public static bool IsBrowserGet()
        {
            string[] browserName = { "ie", "opera", "netscape", "mozilla" };
            string currBrowser = HttpContext.Current.Request.Browser.Type.ToLower();
            for (int i = 0; i < browserName.Length; i++)
            {
                if (currBrowser.IndexOf(browserName[i]) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断当前访问是否来自搜索引擎访问
        /// </summary>
        /// <returns>如果是返回为真,否则为假</returns>
        public static bool IsSearchEngineGet()
        {
            string[] searchEngine = { "baidu", "google", "yahoo", "msn", "sogou", "sohu", "163", "sina", "tom" };
            string referrer = HttpContext.Current.Request.UrlReferrer.ToString().ToLower();
            for (int i = 0; i < searchEngine.Length; i++)
            {
                if (referrer.IndexOf(searchEngine[i]) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获得当前完整Url地址
        /// </summary>
        /// <returns>当前完整Url地址</returns>
        public static string GetUrl()
        {
            return HttpContext.Current.Request.Url.ToString();
        }

        /// <summary>
        /// 返回当前指定Url参数的值
        /// </summary>
        /// <param name="strName">url参数名称</param>
        /// <returns></returns>
        public static string GetQueryString(string strName)
        {
            if (HttpContext.Current.Request.QueryString[strName] == null)
                return string.Empty;

            return HttpContext.Current.Request.QueryString[strName];
        }

        /// <summary>
        /// 返回当前访问的页面名称
        /// </summary>
        /// <returns>当前访问的页面名称</returns>
        public static string GetCurrentPageName()
        {
            string[] urlArray = HttpContext.Current.Request.Url.AbsoluteUri.Split('/');
            return urlArray[urlArray.Length - 1].ToLower();
        }

        /// <summary>
        /// 返回当前访问参数总个数
        /// </summary>
        /// <returns></returns>
        public static int GetQueryStringCount()
        {
            return HttpContext.Current.Request.QueryString.Count;
        }

        /// <summary>
        /// 返回请求表单里参数值
        /// </summary>
        /// <param name="strName">表单参数名</param>
        /// <returns>表单里参数值</returns>
        public static string GetFormString(string strName)
        {
            if (HttpContext.Current.Request.Form[strName] == null)
                return string.Empty;

            return HttpContext.Current.Request.Form[strName];
        }

        /// <summary>
        /// 获得当前页面客户端的IP
        /// </summary>
        /// <returns>当前页面客户端的IP</returns>
        public static string GetClientIP()
        {
            string result = String.Empty;

            result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }

            if (null == result || result == String.Empty)
            {
                return "0.0.0.0";
            }

            return result;

        }

        /// <summary>
        /// 保存用户上传的文件
        /// </summary>
        /// <param name="path">保存路径</param>
        public static void SaveRequestFile(string path)
        {
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                HttpContext.Current.Request.Files[0].SaveAs(path);
            }
        }

        /// <summary>
        /// 获取服务器站点的虚拟路径 如:/Web
        /// </summary>
        public static string GetBasePath()
        {
            string rootPath = HttpContext.Current.Request.ApplicationPath;
            if (rootPath.EndsWith("/"))
                rootPath = rootPath.Substring(0, rootPath.LastIndexOf("/"));

            return rootPath;
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
    }
}
