using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace KellCommons
{
    /// <summary>
    /// 采集处理静态类
    /// </summary>
    public static class CollectionProcessor
    {
        static volatile bool stop;

        public static void StopCollecting()
        {
            if (!stop)
                stop = true;
        }
        /// <summary>
        /// 获取指定URL页面的文本内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoder"></param>
        /// <returns></returns>
        public static string RequestTextFromUrl(string url, Encoding encoder)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[1024];
                int len = stream.Read(buffer, 0, buffer.Length);
                sb.Append(encoder.GetString(buffer, 0, len));
                while (len > 0)
                {
                    len = stream.Read(buffer, 0, buffer.Length);
                    sb.Append(encoder.GetString(buffer, 0, len));
                }
                stream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                throw new Exception("请求网络流资源错误：\n" + e.Message);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获取指定的URL的标题
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetTitle(string url, Encoding encoding)
        {
            string title = "无标题";
            string pattern = "<title>(.*?)</title>";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StringBuilder sb = new StringBuilder();
            byte[] testData = new byte[1024];
            Stream stream = response.GetResponseStream();
            while (true)
            {
                int count = stream.Read(testData, 0, testData.Length);
                sb.Append(encoding.GetString(testData, 0, count));
                Match mat = Regex.Match(sb.ToString(), pattern, RegexOptions.IgnoreCase);
                if (mat.Success)
                {
                    title = mat.Groups[1].Value;
                    stream.Close();
                    response.Close();
                    break;
                }
            }
            return title;
        }
        /// <summary>
        /// 采用正则表达式来截取字符串
        /// </summary>
        /// <param name="content"></param>
        /// <param name="beginString"></param>
        /// <param name="endString"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static string GetStringBetweenEx(string content, string beginString, string endString, bool ignoreCase)//, out int nextIndex)
        {
            string list = "";
            if (string.IsNullOrEmpty(beginString) || string.IsNullOrEmpty(endString))
                return list;
            //nextIndex = -1;
            string pattern = beginString + "(.*?)" + endString;
            RegexOptions ro = RegexOptions.Multiline;
            if (ignoreCase)
                ro = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            while (true)
            {
                Match mat = Regex.Match(content, pattern, ro);
                if (mat.Success)
                {
                    list = mat.Groups[1].Value;
                    //nextIndex = mat.Groups[1].Index + beginString.Length + list.Length + endString.Length;
                    break;
                }
            }
            return list;
        }
        /// <summary>
        /// 采用普通的字符串截取办法来截取字符串
        /// </summary>
        /// <param name="content"></param>
        /// <param name="beginString"></param>
        /// <param name="endString"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static string GetStringBetween(string content, string beginString, string endString, bool ignoreCase)
        {
            string list = "";
            if (string.IsNullOrEmpty(beginString) || string.IsNullOrEmpty(endString))
                return list;
            int begin = 0, end = 0;
            if (ignoreCase)
                begin = content.IndexOf(beginString, StringComparison.InvariantCultureIgnoreCase);
            else
                begin = content.IndexOf(beginString);
            if (begin > -1 && begin + beginString.Length < content.Length)
            {
                if (ignoreCase)
                    end = content.IndexOf(endString, begin + beginString.Length, StringComparison.InvariantCultureIgnoreCase);
                else
                    end = content.IndexOf(endString, begin + beginString.Length);
            }
            if (begin > -1 && end - beginString.Length > begin)
                list = content.Substring(begin + beginString.Length, end - begin - beginString.Length);
            return list;
        }
        /// <summary>
        /// 采用正则表达式来分析文本，并返回不超过指定最大数量的匹配的网页信息列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="host"></param>
        /// <param name="beginString"></param>
        /// <param name="endString"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="maxCount"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public static List<WebPageInfo> GetWebListByListPageContent(string content, string host, string beginString, string endString, bool ignoreCase, int maxCount, bool distinct)
        {
            List<WebPageInfo> webs = new List<WebPageInfo>();
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(beginString) || string.IsNullOrEmpty(endString))
                return webs;
            //string pattern = Regex.Escape(beginString) + "(.*?)" + Regex.Escape(endString);
            string pattern = "(?<=" + Regex.Escape(beginString) + ").*(?=" + Regex.Escape(endString) + ")";
            RegexOptions ro = RegexOptions.Multiline;
            if (ignoreCase)
                ro = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            MatchCollection mats = Regex.Matches(content, pattern, ro);
            foreach (Match mat in mats)
            {
                if (stop)
                    break;
                if (!string.IsNullOrEmpty(mat.Value))
                {
                    string fullpath = GetFullPath(host, mat.Value);
                    if (distinct)
                    {
                        bool exist = false;
                        foreach (WebPageInfo w in webs)
                        {
                            if (w.Url.ToLower() == fullpath.ToLower())
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (exist)
                            continue;
                    }
                    string html = GetWebContent(fullpath);
                    WebPageInfo web = new WebPageInfo();
                    web.Url = fullpath;
                    web.Title = GetPageTitle(html);
                    web.Keywords = GetKeywordsContentByTag(GetPageKeywordsTag(html));
                    web.Description = GetDescriptionContentByTag(GetPageDescriptionTag(html));
                    //web.HtmlContent = html;
                    webs.Add(web);
                    if (webs.Count >= maxCount)
                        break;
                }
            }
            stop = false;
            return webs;
        }

        /// <summary>
        /// 当host=null时，不考虑host的情况，直接返回原始target值
        /// </summary>
        /// <param name="host"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetFullPath(string host, string target)
        {
            string fullpath = target;
            if (host != null && !target.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                if (target.StartsWith("/"))
                    fullpath = host + target;
                else
                    fullpath = host + "/" + target;
            }
            return fullpath;
        }
        /// <summary>
        /// 取货指定的URL的主机（域名）
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetHost(string uri)
        {
            string host = "http://localhost";
            if (uri.Length > 7 && uri.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                int end = uri.IndexOf("/", 7);
                if (end != -1)
                {
                    host = uri.Substring(0, end);
                }
                else
                {
                    host = uri;
                }
            }
            return host;
        }

        public static StreamReader GetStreamReader(string uri)
        {
            StreamReader streamReader = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                string timeout = KellCommon.ReadAppSetting("Timeout");
                if (!string.IsNullOrEmpty(timeout))
                {
                    request.Timeout = int.Parse(timeout) * 1000; //设置连接超时时间;
                }
                request.Headers.Set("Pragma", "no-cache");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamReceive = response.GetResponseStream();
                streamReader = new StreamReader(streamReceive, Encoding.Default);
            }
            catch (Exception e)
            {
                throw new Exception("GetStreamReader:" + e.Message);
            }
            return streamReader;
        }

        public static string GetText(string uri)
        {
            string strResult = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                string timeout = KellCommon.ReadAppSetting("Timeout");
                if (!string.IsNullOrEmpty(timeout))
                {
                    request.Timeout = int.Parse(timeout) * 1000; //设置连接超时时间;
                }
                request.Headers.Set("Pragma", "no-cache");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamReceive = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamReceive, Encoding.Default);
                strResult = streamReader.ReadToEnd();
            }
            catch (Exception e) 
            { 
                throw new Exception("GetText:" + e.Message);
            }
            return strResult;
        }

        public static string GetWebContent(string url)
        {
            string strResult = "";
            if (string.IsNullOrEmpty(url))
                return strResult;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 10 * 1000; //设置连接超时时;
                request.Headers.Set("Pragma", "no-cache");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamReceive = response.GetResponseStream();
                strResult = GetWebDefaultEncodingContent(streamReceive);
                //获取网页字符编码描述信息
                string webCharSet = GetCharSet(strResult);
                Encoding encoding = Encoding.GetEncoding(webCharSet);
                if (encoding != Encoding.Default)
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 10 * 1000; //设置连接超时间为10秒;
                    request.Headers.Set("Pragma", "no-cache");
                    response = (HttpWebResponse)request.GetResponse();
                    Stream againReceive = response.GetResponseStream();
                    List<byte> data = new List<byte>();
                    byte[] buffer = new byte[1024];
                    int len = 0;
                    while ((len = againReceive.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        data.AddRange(buffer.Take<byte>(len));
                    }
                    strResult = encoding.GetString(data.ToArray());
                }
                response.Close();
            }
            catch (Exception ex)
            {
                strResult = "读不到网页的内容：" + ex.Message;
            }
            return strResult;
        }

        public static string GetWebDefaultEncodingContent(Stream streamReceive)
        {
            string strResult = "";
            StreamReader streamReader = new StreamReader(streamReceive, Encoding.Default);
            strResult = streamReader.ReadToEnd();
            return strResult;
        }

        public static string GetCharSet(string defaultWebContent)
        {
            Match charSetMatch = Regex.Match(defaultWebContent, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return charSetMatch.Groups[2].Value;
        }

        public static string GetPageTitle(string content)
        {
            try
            {
                Regex r = new Regex("(?<=<title>).*?(?=</title>)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match m = r.Match(content);
                if (m.Success)
                {
                    return m.Value;
                }
            }
            catch (Exception e)
            {
               throw new Exception("GetPageTitle:" + e.Message);
            }
            return "";
        }

        public static string GetPageKeywordsTag(string content)
        {
            try
            {
                Regex r = new Regex(@"<meta\s*name\s*=\s*['""]?keywords['""]?\s*content\s*=\s*['""]?(?<KeyWords>[^'"">]*)['""]?[^>]*>|<meta\s*content\s*=\s*['""]?(?<KeyWords>[^'"">]*)['""]?\s*name\s*=\s*['""]?keywords['""]?\s*[^>]*>", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match m = r.Match(content);
                if (m.Success)
                {
                    return m.Value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetPageKeywordsTag:" + e.Message);
            }
            return "";
        }

        public static string GetPageDescriptionTag(string content)
        {
            try
            {
                Regex r = new Regex(@"<meta\s*name\s*=\s*['""]?description['""]?\s*content\s*=\s*['""]?(?<description>[^'"">]*)['""]?[^>]*>|<meta\s*content\s*=\s*['""]?(?<description>[^'"">]*)['""]?\s*name\s*=\s*['""]?description['""]?\s*[^>]*>", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match m = r.Match(content);
                if (m.Success)
                {
                    return m.Value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetPageDescriptionTag:" + e.Message);
            }
            return "";
        }

        public static string GetPageTagwordsTag(string content)
        {
            try
            {
                Regex r =  new Regex(
 @"<meta\s*name\s*=\s*['""]?tagwords['""]?\s*content\s*=\s*['""]?(?<tagwords>[^'"">]*)['""]?[^>]*>|<meta\s*content\s*=\s*['""]?(?<tagwords>[^'"">]*)['""]?\s*name\s*=\s*['""]?tagwords['""]?\s*[^>]*>
", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                Match m = r.Match(content);
                if (m.Success)
                {
                    return m.Value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetPageTagwordsTag:" + e.Message);
            }
            return "";
        }

        public static string GetPageAuthorTag(string content)
        {
            try
            {
                Regex r =  new Regex(
 @"<meta\s*name\s*=\s*['""]?author['""]?\s*content\s*=\s*['""]?(?<author>[^'"">]*)['""]?[^>]*>|<meta\s*content\s*=\s*['""]?(?<author>[^'"">]*)['""]?\s*name\s*=\s*['""]?author['""]?\s*[^>]*>
", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                Match m = r.Match(content);
                if (m.Success)
                {
                    return m.Value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetPageAuthorTag:" + e.Message);
            }
            return "";
        }

        public static string GetKeywordsContentByTag(string tag)
        {
            try
            {
                Regex r = new Regex(@"content=[""|']?([^""']*)([""|']?).*[\/]?>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match m = r.Match(tag);
                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetKeywordsContentByTag" + e.Message);
            }
            return "";
        }

        public static string GetDescriptionContentByTag(string tag)
        {
            try
            {
                Regex r = new Regex(@"content=[""|']?([^""']*)([""|']?).*[\/]?>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match m = r.Match(tag);
                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }
            catch (Exception e)
            {
                throw new Exception("GetDescriptionContentByTag:" + e.Message);
            }
            return "";
        }

        public static bool VerifyUrl(string uri)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "HEAD";
                req.Timeout = 1000;
                res = (HttpWebResponse)req.GetResponse();
                return (res.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                    res = null;
                }
                if (req != null)
                {
                    req.Abort();
                    req = null;
                }
            }
        }
    }

    public struct WebPageInfo
    {
        public string Url;
        public string Title;
        public string Keywords;
        public string Description;
        //public string HtmlContent;
    }
}
