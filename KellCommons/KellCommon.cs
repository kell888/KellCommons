using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace KellCommons
{
    /// <summary>
    /// Kell常用类库
    /// </summary>
    public class KellCommon
    {
        const string Subkey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        static string p = "5201314";
        [DllImport("user32.dll")]
        static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        const int HTCAPTION = 0x0002;
        const int SC_SCREENSAVE = 0xF140;
        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        /// <summary>
        /// 提供当前程序域默认ini文档的路径
        /// </summary>
        public static string IniFile
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + "\\config.ini";
            }
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        /// <summary>
        /// 读取当前应用程序配置文档的AppSetting中指定key的配置值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        /// <summary>
        /// 读取当前应用程序配置文档的ConnectionString中指定name的连接字符串
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ConnectionStringSettings ReadConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name];
        }
        /// <summary>
        /// 获取当前机器的注册申请码（根据硬盘序列号生成）
        /// </summary>
        /// <param name="HDSN"></param>
        /// <returns></returns>
        public static string GetRegCodeByHardDiskSN(string HDSN)
        {
            if (string.IsNullOrEmpty(HDSN))
                return string.Empty;

            string pwd = HDSN[0].ToString();
            for (int i = 1; i < HDSN.Length; i++)
            {
                if (i < 8)
                    pwd += p[i - 1].ToString() + HDSN[i].ToString();
                else
                    pwd += HDSN[i].ToString();
            }
            return pwd;
        }
        /// <summary>
        /// 读取ini文档的配置项
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="iniFullpath"></param>
        /// <param name="length">读取的值长度默认为1024字节</param>
        /// <returns></returns>
        public static string ReadIni(string section, string key, string iniFullpath, int length = 1024)
        {
            if (!File.Exists(iniFullpath))
            {
                using (StreamWriter sw = File.CreateText(iniFullpath))
                {
                    sw.WriteLine("[" + section + "]");
                    sw.WriteLine(key + "=");
                }
            }
            StringBuilder sb = new StringBuilder(length);
            GetPrivateProfileString(section, key, "", sb, length, iniFullpath);
            string value = sb.ToString();
            string format = value.Replace("$13$", "\r").Replace("$10$", "\n").Replace("$34$", "\"");
            return format;
        }
        /// <summary>
        /// 将配置项写入ini文档
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="iniFullpath"></param>
        public static void WriteIni(string section, string key, string value, string iniFullpath)
        {
            if (!File.Exists(iniFullpath))
            {
                using (StreamWriter sw = File.CreateText(iniFullpath))
                {
                    sw.WriteLine("[" + section + "]");
                    sw.WriteLine(key + "=" + value);
                }
            }
            string format = value.Replace("\r", "$13$").Replace("\n", "$10$").Replace("\"", "$34$");
            WritePrivateProfileString(section, key, format, iniFullpath);
        }
        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public static void ClearIniAllSection(string iniFullpath)
        {
            WritePrivateProfileString(null, null, null, iniFullpath);
        }
        /// <summary>
        /// 删除ini文件下section段落下的所有键
        /// </summary>
        /// <param name="section">指定的段落名</param>
        public static void ClearIniSection(string section, string iniFullpath)
        {
            WritePrivateProfileString(section, null, null, iniFullpath);
        }
        /// <summary>
        /// 根据指定的IP查询网址获取当前机器的公网IP
        /// </summary>
        /// <param name="getUrl"></param>
        /// <returns></returns>
        public static string GetIPByIP138(string getUrl = "http://www.ip138.com/ip2city.asp")
        {
            string ip = "127.0.0.1";
            Uri uri = new Uri(getUrl);
            System.Net.WebRequest wr = System.Net.WebRequest.Create(uri);
            using (System.IO.Stream s = wr.GetResponse().GetResponseStream())
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(s, Encoding.Default))
                {
                    string all = sr.ReadToEnd(); //读取网站的数据
                    int i = all.IndexOf("[") + 1;
                    int j = all.IndexOf("]", i);
                    string tempip = all.Substring(i, j - i);
                    ip = tempip;
                    //ip = tempip.Replace("]", "").Replace(" ", "");//找出ip
                }
            }
            return ip;
        }
        /// <summary>
        /// 获取当前机器的公网IP
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string ip = "127.0.0.1";
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipa = ipe.AddressList[0];
            ip = ipa.ToString();
            if (!IsIPv4(ip) && ipe.AddressList.Length > 3)
                ip = ipe.AddressList[3].ToString();
            return ip;

            //下面的方法也可以，因为不管什么操作系统下，用GetHostByName()方法返回的AddressList就只有一个
            //IPHostEntry ipe = Dns.GetHostByName(Dns.GetHostName());
            //ip = ipe.AddressList[0].ToString();
        }
        /// <summary>
        /// 判断指定的IP地址是否为IPv4版本
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPv4(string ip)
        {
            return Regex.IsMatch(ip, @"^((25[0-5]|2[0-4]\d|[0-1]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[0-1]?\d\d?)$");
        }
        /// <summary>
        /// 根据指定的网址获取网页内容
        /// </summary>
        /// <param name="url">要访问的网站地址</param>
        /// <param name="charSet">目标网页的编码，如果传入的是null或者""，那就自动分析网页的编码</param>
        /// <returns></returns>
        public static string GetHtml(string url, string charSet)
        {
            WebClient client = new WebClient(); //创建WebClient实例myWebClient
            // 需要注意的：
            //有的网页可能下不下来，有种种原因比如需要cookie,编码问题等等
            //这是就要具体问题具体分析比如在头部加入cookie
            // webclient.Headers.Add("Cookie", cookie);
            //这样可能需要一些重载方法。根据需要写就可以了

            //获取或设置用于对向 Internet 资源的请求进行身份验证的网络凭据。
            //client.Credentials = CredentialCache.DefaultCredentials;
            //如果服务器要验证用户名,密码
            //NetworkCredential mycred = new NetworkCredential(struser, strpassword);
            //myWebClient.Credentials = mycred;
            //从资源下载数据并返回字节数组。（加@是因为网址中间有"/"符号）
            byte[] buffer = client.GetData(url);
            string html = Encoding.Default.GetString(buffer);

            //获取网页字符编码描述信息
            Match charSetMatch = Regex.Match(html, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string webCharSet = charSetMatch.Groups[2].Value;

            if (string.IsNullOrEmpty(charSet))
                charSet = webCharSet;

            if (!string.IsNullOrEmpty(charSet) && Encoding.GetEncoding(charSet) != Encoding.Default)
                html = Encoding.GetEncoding(charSet).GetString(buffer);

            return html;
        }
        /// <summary>
        /// 获取指定url的文本内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetWebContent(string url)
        {
            string strResult = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 10 * 1000; //设置连接超时时;
                request.Headers.Set("Pragma", "no-cache");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamReceive = response.GetResponseStream();
                strResult = GetWebDefaultContent(streamReceive);
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
        /// <summary>
        /// 获取默认字符编码下的网页内容
        /// </summary>
        /// <param name="streamReceive"></param>
        /// <returns></returns>
        public static string GetWebDefaultContent(Stream streamReceive)
        {
            string strResult = "";
            StreamReader streamReader = new StreamReader(streamReceive, Encoding.Default);
            strResult = streamReader.ReadToEnd();
            return strResult;
        }
        /// <summary>
        /// 根据默认字符编码的网页内容，自动获取实际的字符编码
        /// </summary>
        /// <param name="defaultWebContent"></param>
        /// <returns></returns>
        public static string GetCharSet(string defaultWebContent)
        {
            Match charSetMatch = Regex.Match(defaultWebContent, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return charSetMatch.Groups[2].Value;
        }
        /// <summary>
        /// 获取网页的Title标签的内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 获取网页的Keywords标签
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 获取网页的Description标签
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 获取网页的Tagwords标签
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetPageTagwordsTag(string content)
        {
            try
            {
                Regex r = new Regex(
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 获取网页的Author标签
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetPageAuthorTag(string content)
        {
            try
            {
                Regex r = new Regex(
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 根据Keywords标签，获取Keywords标签的内容
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 根据Description标签，获取Description标签的内容
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
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
                throw e;
            }
            return "";
        }
        /// <summary>
        /// 判断指定URI是否存在
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool IsExistUri(string uri)
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
        /// <summary>
        /// 利用Ping方法确定是否可以通过网络访问远程计算机
        /// </summary>
        /// <param name="status">网络状态</param>
        /// <param name="ipAddressOrHostName">默认是www.baidu.com</param>
        /// <param name="timeout">默认是1000毫秒</param>
        /// <returns></returns>
        public static bool PingIsConnectedInternet(out System.Net.NetworkInformation.IPStatus status, string ipAddressOrHostName = "www.baidu.com", int timeout = 1000)
        {
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            options.DontFragment = true;
            string data = "aa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            //Wait seconds for a reply. 
            status = System.Net.NetworkInformation.IPStatus.Unknown;
            try
            {
                System.Net.NetworkInformation.PingReply reply = p.Send(ipAddressOrHostName, timeout, buffer, options);
                status = reply.Status;
                if (status == System.Net.NetworkInformation.IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 利用wininet.dll中的InternetGetConnectedState方法来确定是否连接上Internet网
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedInternet()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }
        /// <summary>
        /// 开机自运行注册。检查当前机器中是否已经注册过指定的注册项，如若尚未注册，就注册指定的可执行文件路径到该注册项，最后返回是否曾被注册过
        /// </summary>
        /// <param name="keyName">指定的注册项名</param>
        /// <param name="executablePath">指定的可执行文件路径</param>
        /// <returns></returns>
        public static bool CheckAndRegister(string executablePath, string keyName = null)
        {
            int fileNameStart = executablePath.LastIndexOf("\\") + 1;
            int extensionStart = executablePath.LastIndexOf(".");
            string exeFileName = executablePath.Substring(fileNameStart, extensionStart - fileNameStart);
            string KeyName = exeFileName + "Init";
            if (!string.IsNullOrEmpty(keyName))
                KeyName = keyName;
            if (!IsRegeditExist(KeyName))
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(Subkey))
                    {
                        key.SetValue(KeyName, executablePath);
                    }
                }
                catch
                {
                    return false;
                }
                return false;
            }
            return true;
        }
        /// <summary>
        /// 检查当前机器中是否已经注册过指定的注册项
        /// </summary>
        /// <param name="keyName">指定的注册项名</param>
        /// <returns></returns>
        public static bool IsRegeditExist(string keyName)
        {
            RegistryKey software = Registry.LocalMachine.OpenSubKey(Subkey, true);
            string[] subkeyNames = software.GetValueNames();
            //取得该项下所有键值的名称的序列，并传递给预定的数组中   
            foreach (string key in subkeyNames)
            {
                if (keyName == key)
                {
                    Registry.LocalMachine.Close();
                    return true;
                }
            }
            Registry.LocalMachine.Close();
            return false;
        }
        /// <summary>
        /// 鼠标点击非标题栏时拖动窗口
        /// </summary>
        public static void FormMoveForMouseDown(IntPtr formHandle)
        {
            ReleaseCapture();
            SendMessage(formHandle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
        /// <summary>
        /// 启动windows屏幕保护程序
        /// </summary>
        public static void LaunchScreenSave(IntPtr formHandle)
        {
            SendMessage(formHandle, WM_SYSCOMMAND, SC_SCREENSAVE, 0);
        }
        /// <summary>
        /// 闪烁指定的窗体一次
        /// </summary>
        /// <param name="formHandle"></param>
        public static void FlashWindow(IntPtr formHandle)
        {
            FlashWindow(formHandle, true);
        }
        /// <summary>
        /// 通过逼近的方式扫描图片的轮廓，返回不透明的区域路径
        /// </summary>
        /// <param name="bitmap">要扫描的图片</param>
        /// <param name="transparentColor">透明色</param>
        /// <returns></returns>
        public static GraphicsPath CalculateGraphicsPath(Bitmap bitmap, Color transparentColor)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            //存储发现的第一个不透明的象素的列值（即x坐标），这个值将定义我们扫描不透明区域的边缘
            int opaquePixelX = 0;
            //从纵向开始
            for (int y = 0; y < bitmap.Height; y++)
            {
                opaquePixelX = 0;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) != transparentColor)
                    {
                        //标记不透明象素的位置
                        opaquePixelX = x;
                        //记录当前位置
                        int nextX = x;
                        for (nextX = opaquePixelX; nextX < bitmap.Width; nextX++)
                        {
                            if (bitmap.GetPixel(nextX, y) == transparentColor)
                            {
                                break;
                            }
                        }
                        graphicsPath.AddRectangle(new Rectangle(opaquePixelX, y, nextX - opaquePixelX, 1));
                        x = nextX;
                    }
                }
            }
            return graphicsPath;
        }
        /// <summary>
        /// 通过逼近的方式扫描图片的轮廓，返回不透明的区域路径
        /// </summary>
        /// <param name="bitmap">要扫描的图片</param>
        /// <param name="transparentColor">透明色</param>
        /// <param name="tolerance">透明色容差</param>
        /// <returns></returns>
        public static GraphicsPath CalculateGraphicsPath(Bitmap bitmap, Color transparentColor, uint tolerance)
        {
            List<Point> ps = GetPointsByNearestColor(bitmap, transparentColor, tolerance);
            GraphicsPath graphicsPath = new GraphicsPath();
            //存储发现的第一个不透明的象素的列值（即x坐标），这个值将定义我们扫描不透明区域的边缘
            int opaquePixelX = 0;
            //从纵向开始
            for (int y = 0; y < bitmap.Height; y++)
            {
                opaquePixelX = 0;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (!ps.Contains(new Point(x, y)))//bitmap.GetPixel(x, y) != transparentColor)
                    {
                        //标记不透明象素的位置
                        opaquePixelX = x;
                        //记录当前位置
                        int nextX = x;
                        for (nextX = opaquePixelX; nextX < bitmap.Width; nextX++)
                        {
                            if (ps.Contains(new Point(nextX, y)))
                                break;
                            //Color pixel = bitmap.GetPixel(nextX, y);
                            //if (pixel == transparentColor)
                            //{
                            //    break;
                            //}
                        }
                        graphicsPath.AddRectangle(new Rectangle(opaquePixelX, y, nextX - opaquePixelX, 1));
                        x = nextX;
                    }
                }
            }
            return graphicsPath;
        }
        /// <summary>
        /// 获取指定位图中对应指定颜色以及容差的临近颜色点集
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="c"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<Point> GetPointsByNearestColor(Bitmap bmp, Color c, uint tolerance)
        {
            if (tolerance == 0)
            {
                System.Windows.Forms.MessageBox.Show("容差不能为零！");
                return new List<Point>();
            }
            if (tolerance > 443)
            {
                System.Windows.Forms.MessageBox.Show("容差超出最大的范围！");
                return new List<Point>();
            }

            List<Point> ps = new List<Point>();
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int BPP = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                int stride = data.Stride;
                int offset = stride - BPP * bmp.Width;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        double distance = Math.Sqrt((c.R - p[2]) * (c.R - p[2]) + (c.G - p[1]) * (c.G - p[1]) + (c.B - p[0]) * (c.B - p[0]));
                        if (distance < tolerance)
                        {
                            ps.Add(new Point(x, y));
                        }
                        p += BPP;
                    }
                    p += offset;
                }
            }
            bmp.UnlockBits(data);

            return ps;
        }
        /// <summary>
        /// 将指定的位图中不透明部分设置为指定控件的外围轮廓（边界）
        /// </summary>
        /// <param name="control"></param>
        /// <param name="bitmap"></param>
        /// <param name="transparentColor"></param>
        /// <param name="graphicsPath"></param>
        public static void SetControlRegion(Control control, Bitmap bitmap, Color transparentColor, out GraphicsPath graphicsPath)
        {
            graphicsPath = CalculateGraphicsPath(bitmap, transparentColor);
            Region reg = new Region(graphicsPath);
            RectangleF rec = graphicsPath.GetBounds();
            control.BackgroundImage = bitmap.Clone(rec, PixelFormat.Format24bppRgb);
            reg.Translate(-rec.X, -rec.Y);
            control.Region = reg;
            control.Size = graphicsPath.GetBounds().Size.ToSize();
        }
        /// <summary>
        /// 将指定的位图中不透明部分设置为指定窗体的外围轮廓（指定相似颜色容差，且返回区域路径）
        /// </summary>
        /// <param name="form"></param>
        /// <param name="bitmap"></param>
        /// <param name="transparentColor"></param>
        /// <param name="tolerance"></param>
        /// <param name="graphicsPath"></param>
        public static void SetFormRegion(Form form, Bitmap bitmap, Color transparentColor, uint tolerance, out GraphicsPath graphicsPath)
        {
            form.FormBorderStyle = FormBorderStyle.None;
            graphicsPath = CalculateGraphicsPath(bitmap, transparentColor, tolerance);
            Region reg = new Region(graphicsPath);
            RectangleF rec = graphicsPath.GetBounds();
            form.BackgroundImage = bitmap.Clone(rec, PixelFormat.Format24bppRgb);
            reg.Translate(-rec.X, -rec.Y);
            form.Region = reg;
            form.Size = graphicsPath.GetBounds().Size.ToSize();
        }
        /// <summary>
        /// 将指定的位图中不透明部分设置为指定窗体的外围轮廓
        /// </summary>
        /// <param name="form"></param>
        /// <param name="bitmap"></param>
        /// <param name="transparentColor"></param>
        public static void SetFormRegion(Form form, Bitmap bitmap, Color transparentColor)
        {
            form.FormBorderStyle = FormBorderStyle.None;
            form.BackgroundImage = bitmap;
            form.Size = bitmap.Size;
            form.TransparencyKey = transparentColor;
        }
        ///
        ///获取鼠标闲置时间
        ///
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref　LASTINPUTINFO plii);
        ///
        ///设置鼠标状态的计数器（非状态）
        ///
        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        static extern int ShowCursor(bool bShow);
        ///
        ///获取闲置时间
        ///
        static long GetIdleTick()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = (uint)Marshal.SizeOf(vLastInputInfo);
            vLastInputInfo.dwTime = 0;
            if (!GetLastInputInfo(ref vLastInputInfo))
                return 0;
            return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }
        /// <summary>
        /// 鼠标或键盘超过timeout秒的闲置时间就隐藏光标，该方法应该放在Timer_Tick函数中（Timer的间隔时间最大可以设置为1秒），进行循环判断
        /// </summary>
        /// <param name="timeout">闲置时间(单位：秒)</param>
        /// <param name="cursorStatus">一定要设为全局变量，初始化为0</param>
        public static void ShowOrHideCursorByTimerTick(int timeout, ref int cursorStatus)
        {
            //光标状态计数器cursorStatus>=0的情况下光标可见，cursorStatus<0不可见，并不是直接受api函数ShowCursor的调用而改变
            long i = GetIdleTick();
            if (i > 1000 * timeout)//超过timeout秒的闲置时间就隐藏光标
            {
                while (cursorStatus >= 0)//循环，直到光标消失
                {
                    cursorStatus = ShowCursor(false);
                }
            }
            else
            {
                while (cursorStatus < 0)//循环，直到光标出现
                {
                    cursorStatus = ShowCursor(true);
                }
            }
        }
        /// <summary>
        /// 鼠标或键盘超过timeout秒的闲置时间就执行外部委托方法，该方法应该放在Timer_Tick函数中（Timer的间隔时间设置得越小越好，且必须小于timeout），进行循环判断
        /// </summary>
        /// <param name="timeout">闲置时间(单位：毫秒)</param>
        /// <param name="doSomething"></param>
        /// <param name="args">要执行委托的参数数组</param>
        public static void DoSomethingWhenNoInput(int timeout, Delegate doSomething, params object[] args)
        {
            if (doSomething == null)
                return;

            long t = GetIdleTick();
            if (t > timeout)
            {
                try
                {
                    //object[] ar = null;
                    //if (args.Length > 1)
                    //{
                    //    ar = new object[args.Length - 1];
                    //    for (int i = 0; i < args.Length - 1; i++)
                    //    {
                    //        ar[i] = args[i + 1];
                    //    }
                    //}
                    //doSomething.Method.Invoke(args[0], ar);
                    //若使用以上方法，就要在外部调用本方法时在参数数组索引0处传入该委托所在的类实例
                    if (args.Length > 0)
                        doSomething.DynamicInvoke(args);
                    else
                        doSomething.DynamicInvoke(null);
                }
                catch (Exception e)
                {
                    throw new Exception("执行委托方法" + doSomething.Method.Name + "时出错:" + e.Message);
                }
            }
        }
        //示例：
        //void timer_Tick(object sender, EventArgs e)
        //{
        //    //光标状态计数器iCount>=0的情况下光标可见，iCount<0不可见，并不是直接受api函数ShowCursor的调用而改变
        //    long i = GetIdleTick();
        //    if (i > 5000)//超过5秒的闲置时间就隐藏光标
        //    {
        //        while (iCount >= 0)//循环，直到光标消失
        //        {
        //            iCount = ShowCursor(false);
        //        }
        //    }
        //    else
        //    {
        //        while (iCount < 0)//循环，直到光标出现
        //        {
        //            iCount = ShowCursor(true);
        //        }
        //    }
        //}
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LASTINPUTINFO
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public uint dwTime;
    }

    /// <summary>
    /// 程序集信息类
    /// </summary>
    public class AssemblyUtil
    {
        Assembly ass;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assembly"></param>
        public AssemblyUtil(Assembly assembly)
        {
            ass = assembly;
        }
        #region 程序集属性访问器
        /// <summary>
        /// 程序集标题
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                // 获取此程序集上的所有 Title 属性
                object[] attributes = ass.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // 如果至少有一个 Title 属性
                if (attributes.Length > 0)
                {
                    // 请选择第一个属性
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // 如果该属性为非空字符串，则将其返回
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                // 如果没有 Title 属性，或者 Title 属性为一个空字符串，则返回 .exe 的名称
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }
        /// <summary>
        /// 程序集版本
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return ass.GetName().Version.ToString();
            }
        }
        /// <summary>
        /// 程序集描述
        /// </summary>
        public string AssemblyDescription
        {
            get
            {
                // 获取此程序集的所有 Description 属性
                object[] attributes = ass.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                // 如果 Description 属性不存在，则返回一个空字符串
                if (attributes.Length == 0)
                    return "";
                // 如果有 Description 属性，则返回该属性的值
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }
        /// <summary>
        /// 程序集产品名
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                // 获取此程序集上的所有 Product 属性
                object[] attributes = ass.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // 如果 Product 属性不存在，则返回一个空字符串
                if (attributes.Length == 0)
                    return "";
                // 如果有 Product 属性，则返回该属性的值
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }
        /// <summary>
        /// 程序集版权
        /// </summary>
        public string AssemblyCopyright
        {
            get
            {
                // 获取此程序集上的所有 Copyright 属性
                object[] attributes = ass.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // 如果 Copyright 属性不存在，则返回一个空字符串
                if (attributes.Length == 0)
                    return "";
                // 如果有 Copyright 属性，则返回该属性的值
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }
        /// <summary>
        /// 程序集厂商
        /// </summary>
        public string AssemblyCompany
        {
            get
            {
                // 获取此程序集上的所有 Company 属性
                object[] attributes = ass.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                // 如果 Company 属性不存在，则返回一个空字符串
                if (attributes.Length == 0)
                    return "";
                // 如果有 Company 属性，则返回该属性的值
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion
    }
}
