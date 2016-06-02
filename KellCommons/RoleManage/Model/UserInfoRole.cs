using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellCommons.RoleManage.Model
{
    public class UserInfoRole
    {
        private int userRoleId;
        private UserInfo userInfo;
        private RoleInfo roleInfo;

        public RoleInfo RoleInfo
        {
            get { return roleInfo; }
            set { roleInfo = value; }
        }

        public UserInfo UserInfo
        {
            get { return userInfo; }
            set { userInfo = value; }
        }

        public int UserRoleId
        {
            get { return userRoleId; }
            set { userRoleId = value; }
        }
    }
}
