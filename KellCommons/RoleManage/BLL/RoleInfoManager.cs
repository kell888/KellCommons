using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using KellCommons.RoleManage.DAL;

namespace KellCommons.RoleManage.BLL
{
    public class RoleInfoManager
    {
        RoleInfoService roleInfoService = new RoleInfoService();
        public List<RoleInfo> GetRoleInfos()
        {
            return roleInfoService.GetRoleInfos();
        }

        public RoleInfo GetRoleInfoById(int roleId)
        {
            return roleInfoService.GetRoleInfoById(roleId);
        }

        public int AddRoleInfo(string roleName)
        {
            return roleInfoService.AddRoleInfo(roleName);
        }

        public int UpdateRoleInfoByRole(RoleInfo roleInfo)
        {
            return roleInfoService.UpdateRoleInfoByRole(roleInfo);
        }

        public int DeleteRoleInfoByRole(int roleId)
        {
            return roleInfoService.DeleteRoleInfoByRole(roleId);
        }
    }
}
