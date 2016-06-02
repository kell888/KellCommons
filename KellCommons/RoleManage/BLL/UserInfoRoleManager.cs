using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.DAL;
using KellCommons.RoleManage.Model;

namespace KellCommons.RoleManage.BLL
{
    public class UserInfoRoleManager
    {
        UserInfoRoleService userInfoService = new UserInfoRoleService();
        public List<UserInfoRole> GetUserInfoRoles(int userId)
        {
            return userInfoService.GetUserInfoRoles(userId);
        }

        public bool InsertUserRole(int userId)
        {
            return userInfoService.InsertUserRole(userId);
        }

        public bool UpdateUserRoleByUserId(int RoleId, int userId)
        {
            return userInfoService.UpdateUserRoleByUserId(RoleId,userId);
        }

        public bool DeleteUserRoleByRoleId(int RoleId)
        {
            return userInfoService.DeleteUserRoleByRoleId(RoleId);
        }
        public bool UpdateRoleByRoleId(int RoleId)
        {
            return userInfoService.UpdateRoleByRoleId(RoleId);
        }

    }
}
