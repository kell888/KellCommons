using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellCommons.RoleManage.Model
{
    public class PopedomInfo
    {
        private int popedomId;
        private string popedomName;
        private int parentID;
        private string url;

        public string Url
        {
            get { return url; }
            set { url = value; }
        }


        public int ParentID
        {
            get { return parentID; }
            set { parentID = value; }
        }

        public string PopedomName
        {
            get { return popedomName; }
            set { popedomName = value; }
        }

        public int PopedomId
        {
            get { return popedomId; }
            set { popedomId = value; }
        }
    }
}
