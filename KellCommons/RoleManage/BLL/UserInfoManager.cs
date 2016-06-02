using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellCommons.RoleManage.Model;
using KellCommons.RoleManage.DAL;

namespace KellCommons.RoleManage.BLL
{
    public class UserInfoManager
    {
        UserInfoService userinfoS = new UserInfoService();
        public List<UserInfo> GetUserInfos()
        {
            return userinfoS.GetUserInfos();
        }

        public UserInfo GetUserInfoByName(string name, string pass) 
        {
           return userinfoS.GetUserInfoByName(name,pass);
        }

        public UserInfo GetUserInfoById(int id)
        {
            return userinfoS.GetUserInfoById(id);
        }

        public int RegisterUser(UserInfo userInfo)
        {
            return userinfoS.RegisterUser(userInfo);
        }

        public int DeleteUser(int userId)
        {
            return userinfoS.DeleteUser(userId);
        }
    }
}
