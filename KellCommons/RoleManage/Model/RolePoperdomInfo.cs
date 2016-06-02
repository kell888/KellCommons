using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellCommons.RoleManage.Model
{
    public class RolePoperdomInfo
    {
        private int rolePoerdomID;

        public int RolePoerdomID
        {
            get { return rolePoerdomID; }
            set { rolePoerdomID = value; }
        }
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

    }
}
