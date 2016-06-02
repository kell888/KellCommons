using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace KellCommons.MediaPlayer
{
    /// <summary>
    /// 用户配置的XML文档
    /// </summary>
    public class UserConfig
    {
        public UserConfig()
        {

        }

        public static ModuleSettings GetSettings(string configFile = null)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(ModuleSettings));
            ModuleSettings data = new ModuleSettings();
            try
            {

                string fileName = System.Environment.CurrentDirectory + "\\UserConfig.xml";
                if (!string.IsNullOrEmpty(configFile))
                    fileName = configFile;
                FileStream fs = new FileStream(fileName, FileMode.Open);
                data = (ModuleSettings)serializer.Deserialize(fs);
                fs.Close();


            }
            catch (System.IO.FileNotFoundException)
            {
                data = new ModuleSettings();
            }
            return data;
        }

        public static void SaveSettings(ModuleSettings data, string configFile = null)
        {
            string fileName = System.Environment.CurrentDirectory + "\\UserConfig.xml";
            if (!string.IsNullOrEmpty(configFile))
                fileName = configFile;

            XmlSerializer serializer = new XmlSerializer(typeof(ModuleSettings));

            FileStream fs = new FileStream(fileName, FileMode.Create);
            serializer.Serialize(fs, data);
            fs.Close();
        }



    }

    public class ModuleSettings
    {
        private string connectionString;
        private string createProcedureSqlText;

        [XmlElement]
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        public string CreateProcedureSqlText
        {
            get { return createProcedureSqlText; }
            set { createProcedureSqlText = value; }
        }
    }
}
