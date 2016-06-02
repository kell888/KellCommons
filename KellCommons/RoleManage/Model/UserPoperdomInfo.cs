using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellCommons.RoleManage.Model
{
    public class UserPoperdomInfo
    {
        private int userPoerdomID;
        private UserInfo userinfo;
        private RoleInfo roleInfo;
        private PopedomInfo popedomInfo;

        public PopedomInfo PopedomInfo
        {
            get { return popedomInfo; }
            set { popedomInfo = value; }
        }

        public RoleInfo RoleInfo
        {
            get { return roleInfo; }
            set { roleInfo = value; }
        }

        public UserInfo Userinfo
        {
            get { return userinfo; }
            set { userinfo = value; }
        }

        public int UserPoerdomID
        {
            get { return userPoerdomID; }
            set { userPoerdomID = value; }
        }
    }
}
