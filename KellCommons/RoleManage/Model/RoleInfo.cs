using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellCommons.RoleManage.Model
{
    public class RoleInfo
    {
        private int roleID;
        private string roleName;

        public string RoleName
        {
            get { return roleName; }
            set { roleName = value; }
        }

        public int RoleID
        {
            get { return roleID; }
            set { roleID = value; }
        }
    }
}
