using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panto.Framework.Entity
{
    /// <summary>
    /// 学校端的用户
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 登陆用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录用户GUID
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// 用户显示名(包括学校名)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 学校ID
        /// </summary>
        public Guid SchoolID { get; set; }

        /// <summary>
        /// 学校名
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool AutoRemeber { get; set; }

        /// <summary>
        /// 0 学校用户，1 职工用户，2 学生用户
        /// </summary>
        public int Type { get; set; }


    }
}
