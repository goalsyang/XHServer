using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.Login
{
    public class UserModel
    {
        /// <summary>
        /// 用户编码
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 账号状态
        /// </summary>
        // [StatusDescription("0","启用")]
        //[StatusDescription("1", "停用")]
        // public string UserStatus { get; set; }

    }
}
