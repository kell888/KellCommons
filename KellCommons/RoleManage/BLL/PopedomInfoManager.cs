using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using KellCommons.RoleManage.DAL;

namespace KellCommons.RoleManage.BLL
{
    public class PopedomInfoManager
    {
        PopedomInfoService p = new PopedomInfoService();
        public List<PopedomInfo> GetPopedomInfos(int userId)
        {
             return p.GetPopedomInfos(userId);
        }

        public List<PopedomInfo> GetPopedomInfoByUserId(int userId)
        {
            return p.GetPopedomInfoByUserId(userId);
        }

        public List<PopedomInfo> GetPopedomInfos()
        {
            return p.GetPopedomInfos();
        }

        public List<PopedomInfo> GetPopedomInfosByRoleId(int roleId)
        {
            return p.GetPopedomInfosByRoleId(roleId);
        }

      
    }
}
