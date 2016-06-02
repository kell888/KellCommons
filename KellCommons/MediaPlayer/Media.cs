using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace KellCommons.MediaPlayer
{
    public class APIClass
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(string lpszLongPath, string shortFile, int cchBuffer);
        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
    }

    public class Information
    {
        //总时间
        public static int GetMediaLength()
        {
            string durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            APIClass.mciSendString("status media length", durLength, durLength.Length, 0);
            durLength = durLength.Trim();

            if (string.IsNullOrEmpty(durLength))
                return 0;
            else
                return Convert.ToInt32(durLength) / 1000;

            //int h = s / 3600;
            //int m = (s - (h * 3600)) / 60;
            //s = s - (h * 3600 + m * 60);
            //vReturn = string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);  
        }
        //当前播放位置
        public static int CurrentPosition()
        {
            string buf = "";
            buf = buf.PadLeft(128, ' ');
            APIClass.mciSendString("status media position", buf, buf.Length, 0);
            buf = buf.Trim();
            if (string.IsNullOrEmpty(buf))
                return 0;
            else
                return (int)(Convert.ToDouble(buf)) / 1000;
        }
        //进度控制
        public static bool SetProgress(int progress)
        {
            bool result = false;
            string MciCommand = string.Format("seek media to {0}", progress);
            int RefInt = APIClass.mciSendString(MciCommand, progress.ToString(), 0, 0);
            APIClass.mciSendString("play media", null, 0, 0);
            if (RefInt == 0)
            {
                result = true;
            }

            return result;
        }
        //声音控制
        public static bool SetValume(int Valume)
        {
            bool result = false;
            string MciCommand = string.Format("setaudio media volume to {0}", Valume);
            int RefInt = APIClass.mciSendString(MciCommand, null, 0, 0);
            if (RefInt == 0)
            {
                result = true;
            }

            return result;
        }
        //获取mp3信息
        private Mp3Info GetMp3Info(string vFile)
        {
            Mp3Info mp3 = new Mp3Info();
            using (FileStream mystream = new FileStream(vFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mystream.Seek(-128, SeekOrigin.End);
                byte[] mytag = new byte[3];
                mystream.Read(mytag, 0, 3);
                if (System.Text.Encoding.Default.GetString(mytag) == "TAG")
                {
                    mp3.SongName = GetTagData(mystream, 30);
                    mp3.Singer = GetTagData(mystream, 30);
                    mp3.Album = GetTagData(mystream, 30);
                    mp3.Duration = int.Parse(GetTagData(mystream, 30));
                }
            }
            return mp3;
        }

        private string GetTagData(Stream mystream, int mylength)
        {
            byte[] mybytes = new byte[mylength];
            mystream.Read(mybytes, 0, mylength);
            string mytagdata = System.Text.Encoding.Default.GetString(mybytes);
            mytagdata = mytagdata.Trim();
            return mytagdata;
        }
    }

    public struct Mp3Info
    {
        public string SongName;
        public string Singer;
        public string Album;
        public int Duration;
    }

    public struct MediaInfo
    {
        public string FileName;
        public string FileLength;
        public string MediaName;
        public string Author;
        public string Album;
        public int Duration;
    }

    //定义播放状态枚举变量
    public enum  PlayState
    {
        Playing = 1,
        Puase = 2,
        Stop = 3
    }

    public struct structMCI
    {
        public bool Mute;
        public int Duration;
        public int Position;
        public int Volume;
        public int Balance;
        public string Name;
        public PlayState State;
    }

    /// <summary>
    /// 支持mp3/wma/wmv/avi/vob/asf
    /// </summary>
    public class Player
    {
        //定义API函数使用的字符串变量  
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        private string Name = "";
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string Length = "";
        [MarshalAs(UnmanagedType.LPTStr, SizeConst = 128)]
        private string TemStr = "";
        int ilong;

        public structMCI mc = new structMCI();

        //取得播放文件属性
        public string FileName
        {
            get
            {
                return mc.Name;
            }

            set
            {
                //ASCIIEncoding asc = new ASCIIEncoding(); 
                try
                {
                    TemStr = "";
                    TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
                    Name = Name.PadLeft(260, Convert.ToChar(" "));
                    mc.Name = value;
                    ilong = APIClass.GetShortPathName(mc.Name, Name, Name.Length);
                    Name = GetCurrPath(Name);
                    //Name = "open " + Convert.ToChar(34) +  Name + Convert.ToChar(34) + " alias media";
                    Name = "open " + Convert.ToChar(34) + Name + Convert.ToChar(34) + " alias media";
                    ilong = APIClass.mciSendString("close all", TemStr, TemStr.Length, 0);
                    ilong = APIClass.mciSendString(Name, TemStr, TemStr.Length, 0);
                    ilong = APIClass.mciSendString("set media time format milliseconds", TemStr, TemStr.Length, 0);
                    mc.State = PlayState.Stop;
                }
                catch
                {
                    //MessageBox.Show("出错错误!"); 
                }
            }
        }
        //播放
        public void Play()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
            APIClass.mciSendString("play media", TemStr, TemStr.Length, 0);
            mc.State = PlayState.Playing;
        }
        //停止
        public void Stop()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(128, Convert.ToChar(" "));
            ilong = APIClass.mciSendString("close media", TemStr, 128, 0);
            ilong = APIClass.mciSendString("close all", TemStr, 128, 0);
            mc.State = PlayState.Stop;

        }

        public void Puase()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(128, Convert.ToChar(" "));
            ilong = APIClass.mciSendString("pause media", TemStr, TemStr.Length, 0);
            mc.State = PlayState.Puase;
        }
        private string GetCurrPath(string name)
        {
            if (name.Length < 1) return "";
            name = name.Trim();
            name = name.Substring(0, name.Length - 1);
            return name;
        }
        //总时间
        public int Duration
        {
            get
            {
                //FileStream fs = new FileStream(mc.iName, FileMode.Open, FileAccess.Read);
                //long len = fs.Length;
                //string length = Convert.ToInt32(len / 15.9999).ToString().Trim() + "\0";
                try
                {
                    Length = "";
                    Length = Length.PadLeft(128, Convert.ToChar(" "));
                    APIClass.mciSendString("status media length", Length, Length.Length, 0);
                    Length = Length.Trim();
                    if (Length == "") return 0;
                    //if (durLength != length)
                    //{ return (int)(Convert.ToDouble(length) / 1000f); }
                    else
                        return (int)(Convert.ToDouble(Length) / 1000f);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        //当前时间
        public int CurrentPosition
        {
            get
            {
                Length = "";
                Length = Length.PadLeft(128, Convert.ToChar(" "));
                APIClass.mciSendString("status media position", Length, Length.Length, 0);
                mc.Position = (int)(Convert.ToDouble(Length) / 1000f);
                return mc.Position;
            }
        }

        public List<MediaInfo> OpenDirectory(string dirPath)
        {
            List<MediaInfo> minfo = new List<MediaInfo>();
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].FullName.ToLower().IndexOf(".mp3") > 0)
                {
                    //取得文件大小
                    FileInfo MyFileInfo = new FileInfo(files[i].FullName);
                    float MyFileSize = (float)MyFileInfo.Length / (1024 * 1024);
                    //取得不含路径的文件名
                    string MyShortFileName = files[i].FullName.Substring(files[i].FullName.LastIndexOf("\\") + 1);
                    MyShortFileName = MyShortFileName.Substring(0, MyShortFileName.Length - 4);
                    //填充歌曲列表	
                    string[] SubItem = { MyShortFileName, MyFileSize.ToString().Substring(0, 4) + "M", files[i].FullName };
                    MediaInfo mi = new MediaInfo();

                    minfo.Add(mi);
                }
            }
            ModuleSettings MS = new ModuleSettings();
            MS.ConnectionString = dirPath.ToString().Trim();
            UserConfig.SaveSettings(MS);
            return minfo;
        }
        /// <summary>
        /// 自动循环播放
        /// </summary>
        private void AutoPlayNext()
        {
            if (this.ListMusicFile.Items.Count > 0)
            {
                PlayMp3 mp3 = new PlayMp3();
                if (ListMusicFile.SelectedItems.Count > 0)
                {
                    //label7.Text = "连续播放";
                    int iPos = this.ListMusicFile.SelectedItems[0].Index;
                    if (iPos < this.ListMusicFile.Items.Count - 1)
                    {
                        button1.Enabled = false;
                        button2.Enabled = false;
                        textBox1.Enabled = false;
                        button3.Enabled = false;
                        button4.Enabled = false;
                        mp3.FileName = ListMusicFile.Items[ListMusicFile.SelectedItems[0].Index + 1].SubItems[2].Text;
                        ListMusicFile.Items[ListMusicFile.SelectedItems[0].Index + 1].Selected = true;
                        mp3.play();
                        second = mp3.Duration;
                        if (second != 0)
                        {
                            PlayTime_TrackBar.Maximum = mp3.Duration;
                            Btn_Play.Visible = false;
                            Btn_Puase.Visible = true;
                        }
                    }
                    else
                    {
                        label7.Text = "连续播放完毕";
                        second = 1;
                    }
                }
            }
        }
    }
}
public void timer1_Tick(object sender, EventArgs e)
        {
            if (label7.Text == "连续播放" && second == 0)
            {
                AutoPlayNext();
            }
            else if (label7.Text == "连续播放完毕")
            {
                if (second > 0)
                { second = second - 1; }
                else
                {
                    Application.Exit();
                }
            }
            if (label5.Text == "暂停")
            {
                if (ListMusicFile.Items.Count > 0)
                {
                    PlayMp3 mp3 = new PlayMp3();
                    mp3.FileName = ListMusicFile.Text.ToString();
                    mp3.FileName = ListMusicFile.Items[ListMusicFile.SelectedItems[0].Index].SubItems[2].Text;
                    mp3.Puase();
                }
            }
            else
            {
                if (second > 0)
                {
                    second = second - 1;
                    label1.Text = "剩下" + second.ToString() + "秒后完成";
                    if (label5.Text == "关闭")
                    {
                        label6.Visible = true;
                        label6.Text = "电脑将在" + second.ToString() + "秒后关闭";
                    }
                    if (second <= PlayTime_TrackBar.Maximum)
                    {
                        PlayTime_TrackBar.Value = PlayTime_TrackBar.Maximum - second;
                    }
                }
                else
                {
                    if (label5.Text == "关闭")
                    {
                        label6.Text = "正在关闭计算机...";
                        ComputerShutDown.DoExitWin(0x00000008);
                    }
                    else if (label5.Text == "取消")
                    {
                        label6.Text = "";
                        Application.Exit();
                        //this.Close();
                    }
                    else if (this.label5.Text == "Play_Music" || this.label5.Text == "播放" || label5.Text == "双击")
                    {
                        button1.Enabled = true;
                        button2.Enabled = true;
                    }
                    else
                    {
                        label1.Text = "剩下" + second.ToString() + "秒后完成";
                        PlayTime_TrackBar.Value = 0;
                    }
                }
            }
        }

class Song
    {
        public Note[] Notes;

        public Song()
        {
            Notes = new Note[0];
        }

        public static Song LoadFile(String fileName)
        {
            String strSong = File.ReadAllText(fileName);
            String[] strInts = strSong.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Note[] NotesFromFile = new Note[strInts.Length / 2];
            int i = 0;
            int curNote = 0;
            while (i+1 < strInts.Length)  // If there is an odd int leftover, we ignore it
            {
                NotesFromFile[curNote].Frequency =Convert.ToInt32(strInts[i]);
                NotesFromFile[curNote].Duration = Convert.ToInt32(strInts[i + 1]);
                i += 2;
                curNote++;
            }
            Song song = new Song();
            song.Notes = NotesFromFile;
            return song;
        }

        public void Play()
        {
            for (int i = 0; i < Notes.Length; i++)
            {
                Notes[i].Play();
            }
        }
    }

    struct Note
    {
        public int Frequency;
        public int Duration;

        public void Play()
        {
            if (Frequency == 0)  // Zero frequency is a rest
                Thread.Sleep(Duration);
            else
                Console.Beep(Frequency, Duration);
            Thread.Sleep(Duration / 5);  // A short break between notes makes them more distinct
        }
    }
}
