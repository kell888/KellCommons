using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace KellCommons
{
    public class DownloadUtil
    {
        volatile static bool stopServer;
        bool cancelDownload;
        const string FILE_NOT_FOUND = "FILE_NOT_FOUND";
        static string log = AppDomain.CurrentDomain.BaseDirectory + "error.log";
        private int downloadBufferSize = 256;
        private volatile bool downloadFinished;
        private volatile bool downloading;

        public delegate void DownloadExitedHandler(object sender, int progress);
        public event DownloadExitedHandler DownloadExited;

        private void OnDownloadExited(int progress)
        {
            if (DownloadExited != null)
                DownloadExited(this, progress);
        }

        /// <summary>
        /// 下载的缓冲区大小（默认为256字节）
        /// </summary>
        public int DownloadBufferSize
        {
            get { return downloadBufferSize; }
            set { downloadBufferSize = value; }
        }

        public bool DownloadCanceled
        {
            get { return cancelDownload; }
        }

        public void CancelDownload()
        {
            cancelDownload = true;
        }

        public static bool ServerStopped
        {
            get { return stopServer; }
        }

        public static void StopDownloadServer()
        {
            stopServer = true;
        }

        public bool DownloadFinished
        {
            get { return downloadFinished; }
        }

        public bool Downloading
        {
            get { return downloading; }
        }

        public DownloadUtil()
        {
            ClearLog();
        }

        public static void ClearLog()
        {
            File.WriteAllText(log, "", Encoding.UTF8);
        }

        private static void SendFile(Socket socket, string file, int breakPoint)
        {
            if (File.Exists(file))
            {
                //socket.SendFile(file);
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Seek(breakPoint, SeekOrigin.Begin);
                    byte[] len = BitConverter.GetBytes((int)fs.Length);
                    byte[] fileData = new byte[fs.Length - breakPoint];
                    fs.Read(fileData, 0, fileData.Length);
                    byte[] data = new byte[4 + fileData.Length];
                    for (int i = 0; i < len.Length; i++)
                    {
                        data[i] = len[i];
                    }
                    for (int i = 4; i < data.Length; i++)
                    {
                        data[i] = fileData[i - 4];
                    }
                    socket.Send(data); //向客户端发送文件
                }
            }
            else
            {
                byte[] len = BitConverter.GetBytes(0);
                byte[] msg = Encoding.Unicode.GetBytes(FILE_NOT_FOUND);
                byte[] data = new byte[4 + msg.Length];
                for (int i = 0; i < len.Length; i++)
                {
                    data[i] = len[i];
                }
                for (int i = 4; i < data.Length; i++)
                {
                    data[i] = msg[i - 4];
                }
                socket.Send(data); //通知客户程序
            }
        }

        private static void SendToClient(object sockfile)
        {
            SocketAndFileName sf = (SocketAndFileName)sockfile;
            try
            {
                SendFile(sf.Socket, sf.FileName, sf.BreakPoint);
            }
            catch (System.Net.Sockets.SocketException es)
            {
                if (es.ErrorCode == 10054)
                {
                    Log("客户端断开", es);
                }
                else if (es.ErrorCode == 10053)
                {
                    Log("服务器主动断开", es);
                }
                else
                {
                    Log("SocketException", es);
                }
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != "System.ObjectDisposedException")
                {
                    Log("Socket已经被销毁", e);
                }
                else
                {
                    Log("其它异常", e);
                }
            }
            finally
            {
                sf.Socket.Close();
                //sf.Socket.Dispose();
            }
        }
        /// <summary>
        /// 开始文件下载服务（在服务器端）
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="maxConnectCount"></param>
        public static void StartDownloadServer(string ip, int port, int maxConnectCount = 255)
        {
            TcpListener serverListener = null;
            try
            {
                //构建监听器
                serverListener = new TcpListener(IPAddress.Parse(ip), port);
                serverListener.Start(maxConnectCount);
                stopServer = false;
                //监听客户连线请求            
                while (!stopServer)
                {
                    Socket socket = serverListener.AcceptSocket(); //有客户请求连接
                    if (socket == null) continue;

                    byte[] buffer = new Byte[socket.ReceiveBufferSize];
                    int i = socket.Receive(buffer); //接收请求数据.
                    if (i <= 0) continue;
                    if (i > 4)
                    {
                        byte[] lenBytes = new byte[4];
                        for (int j = 0; j < 4; j++)
                        {
                            lenBytes[j] = buffer[j];
                        }
                        byte[] file = new byte[i - 4];
                        for (int j = 0; j < i - 4; j++)
                        {
                            file[j] = buffer[j + 4];
                        }
                        int breakPoint = BitConverter.ToInt32(lenBytes, 0);
                        string filename = Byte2Str(file);
                        SocketAndFileName sf = new SocketAndFileName();
                        sf.Socket = socket;
                        sf.FileName = filename;
                        sf.BreakPoint = breakPoint;
                        //采用多线程异步方式处理请求，以便不会阻塞后面的其它请求（以牺牲服务器CPU资源为代价）
                        Thread thr = new Thread(new ParameterizedThreadStart(SendToClient));
                        thr.Start(sf);
                    }
                }
            }
            catch (Exception e)
            {
                Log("StartDownloadServer出现异常，已停止文件下载服务监听", e);
            }
            finally
            {
                if (serverListener != null)
                    serverListener.Stop(); 
                stopServer = true;
            }
        }

        public static string LogFile
        {
            get { return log; }
        }

        internal struct SocketAndFileName
        {
            public Socket Socket;
            public string FileName;
            public int BreakPoint;
        }

        public struct ValueAndMaximum
        {
            public int Value;
            public int Maximum;
        }

        public class ReportProgressArgs
        {
            int _progress;
            int _fileLength;

            public int Progress
            {
                get { return _progress; }
            }

            public int FileLength
            {
                get { return _fileLength; }
            }

            public ReportProgressArgs(int progress, int fileLength)
            {
                _progress = progress;
                _fileLength = fileLength;
            }
        }

        public delegate void ReportProgressHandler(TcpClient sender, ReportProgressArgs e);

        private void GetFile(TcpClient client, string filename, int breakpoint = 0)
        {
            byte[] len = BitConverter.GetBytes(breakpoint);
            byte[] fileName = Str2Byte(filename);
            byte[] data = new byte[4 + fileName.Length];
            for (int i = 0; i < len.Length; i++)
            {
                data[i] = len[i];
            }
            for (int i = 4; i < data.Length; i++)
            {
                data[i] = fileName[i - 4];
            }
            try
            {
                Log("发送下载文件请求....", null);
                client.Client.Send(data); //发送下载文件请求
            }
            catch (System.Net.Sockets.SocketException es)
            {
                if (es.ErrorCode == 10054)
                {
                    Log("客户端断开", es);
                }
                else if (es.ErrorCode == 10053)
                {
                    Log("服务器主动断开", es);
                }
                else
                {
                    Log("SocketException", es);
                }
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != "System.ObjectDisposedException")
                {
                    Log("Socket已经被销毁", e);
                }
                else
                {
                    Log("其它异常", e);
                }
            }
        }

        /// <summary>
        /// 支持断点续传的文件下载。要配合DownloadServer一起用！（在客户端）
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="savePath"></param>
        /// <param name="hostIp"></param>
        /// <param name="hostPort"></param>
        /// <param name="breakpoint">大于零则是存在断点，也作为输出参数，作为下一次短点记录下来（单位：字节）</param>
        /// <param name="fileLength">下载文件的总长度（单位：字节）</param>
        /// <param name="timeout">接收文件的最大超时时间（单位：秒，默认为60秒）</param>
        /// <returns></returns>
        public bool DownloadWithContinuingBreakpoint(string filename, string savePath, string hostIp, int hostPort, ref int breakpoint, out int fileLength, ReportProgressHandler reportProgress, int timeout = 60)
        {
            fileLength = 0;
            TcpClient client = new TcpClient();
            Log("开始下载....", null);
            try
            {
                client.ReceiveTimeout = 1000 * timeout;

                if (string.IsNullOrEmpty(filename) || filename.Trim() == "") return false;

                if (!TryConnect(client, hostIp, hostPort)) return false; //尝试连接服务器
                if (!client.Connected) return false;//连线失败,退出

                GetFile(client, filename, breakpoint);

                cancelDownload = false;
                //开始接受数据....
                Log("开始接受数据....", null);
                using (NetworkStream ns = client.GetStream())
                {
                    byte[] lenBytes = new byte[4];
                    int l = ns.Read(lenBytes, 0, 4);
                    fileLength = BitConverter.ToInt32(lenBytes, 0);

                    byte[] resBytes = new byte[downloadBufferSize];
                    int resSize;
                    //if (breakpoint > 0)//NetworkStream不支持查找操作！！！这里会引发异常，故屏蔽。采用GetFile方法在服务器端发送剩余的字节来解决此问题。
                    //    ns.Seek(breakpoint, SeekOrigin.Current);

                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                    if (resSize == 0) return false;
                    if (Byte2Str(resBytes) == FILE_NOT_FOUND) return false;

                    using (FileStream fs = new FileStream(savePath, FileMode.Append, FileAccess.Write, FileShare.Write))
                    {
                        do
                        {
                            if (cancelDownload)
                                break;

                            downloading = true;

                            using (MemoryStream ms = new System.IO.MemoryStream())
                            {
                                ms.Write(resBytes, 0, resSize);
                                if (ms.Length > 0)
                                {
                                    if (breakpoint == 0)
                                    {
                                        using (FileStream f = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                                        {
                                            f.Write(ms.ToArray(), 0, (int)ms.Length);
                                        }
                                    }
                                    else
                                    {
                                        fs.Write(ms.ToArray(), 0, (int)ms.Length);
                                    }
                                    //else//这里会造出文件被其他进程占用无法访问的异常！故屏蔽。
                                    //{
                                    //    using (FileStream fs = new FileStream(savePath, FileMode.Append, FileAccess.Write, FileShare.Write))
                                    //    {
                                    //        fs.Write(ms.ToArray(), 0, (int)ms.Length);
                                    //    }
                                    //}
                                }
                            }
                            breakpoint += resSize;
                            reportProgress.Invoke(client, new ReportProgressArgs(breakpoint, fileLength));
                        } while (ns.DataAvailable);
                        downloading = false;
                        if (!ns.DataAvailable)
                            downloadFinished = true;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Log("DownloadWithContinuingBreakpoint", e);
                return false;
            }
            finally
            {
                client.Close();
                OnDownloadExited(breakpoint);
                Log("退出下载....", null);
            }
        }

        public static void Log(string logType, Exception e)
        {
            if (e != null)
                File.AppendAllText(log, "--- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " ---\r\n" + logType + ": " + e.Message + "\r\n", Encoding.UTF8);
            else
                File.AppendAllText(log, "--- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " ---\r\n" + logType + "\r\n", Encoding.UTF8);
        }

        private static byte[] Str2Byte(string str)
        {
            byte[] bs = Encoding.Unicode.GetBytes(str);
            return bs;
        }

        private static string Byte2Str(byte[] buffer)
        {
            string msg = Encoding.Unicode.GetString(buffer).Replace("\0", "");
            return msg.Trim();
        }

        private bool TryConnect(TcpClient client, string IP, int Port)
        {
            try
            {
                client.Connect(IPAddress.Parse(IP), Port); //连接服务器
                return true;
            }
            catch (Exception e)
            {
                Log("主机已关闭或网络不通，不能建立连接", e);
                return false;
            }
        }
    }
}
