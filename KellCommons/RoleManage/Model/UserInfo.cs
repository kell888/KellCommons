using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellCommons.RoleManage.Model
{
    public class UserInfo
    {
        private int userID;
        private string userName;
        private string userPass;
        private string sex;
        private string phone;
        private string address;
        private string trueName;
        private DateTime time;

        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        public string TrueName
        {
            get { return trueName; }
            set { trueName = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public string Phone
        {
            get { return phone; }
            set { phone = value; }
        }

        public string Sex
        {
            get { return sex; }
            set { sex = value; }
        }

        public string UserPass
        {
            get { return userPass; }
            set { userPass = value; }
        }

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public int UserID
        {
            get { return userID; }
            set { userID = value; }
        }

    }
}
