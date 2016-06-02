using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.DAL;

namespace KellCommons.RoleManage.BLL
{
    public class RolePopedomManager
    {
        RolePopedomService rp = new RolePopedomService();

        public bool DeletePopedomByRoleId(int RoleId)
        {
            return rp.DeletePopedomByRoleId(RoleId);
        }

        public int InsertPopedom(int RoleId, int popedomId)
        {
            return rp.InsertPopedom(RoleId,popedomId);
        }

    }
}
