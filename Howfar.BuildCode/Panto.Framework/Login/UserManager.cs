using Panto.Framework.Entity;
using Panto.Framework.Exceptions;
using Panto.Framework.MVC;
using Panto.Map.Extensions.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
//using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;

namespace Panto.Framework.Login
{
    /// <summary>
    /// 登录用户管理
    /// </summary>
    public class UserManager
    {
        /// <summary>
        /// 用户上下文Key
        /// </summary>
        private static readonly string CurrentUserContextKey = "_currentUser_5D9662D7-F699-E311-92A5-00155D0A2505";

        /// <summary>
        /// 用户过期时间（分钟）
        /// </summary>
        private static readonly double USER_EXPIRE_TIME = 60.0;

        /// <summary>
        /// 从cookie中获取当前用户信息
        /// </summary>
        /// <returns></returns>
        public static T GetCurrentUseInfo<T>(string tableName,string filter) where T : class, new()
        {
            //select *　from table where code =@code
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return null;
            }

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(filter))
            {
                return null;
            }

            var shortName = HttpContext.Current.User.Identity.Name;
            T user = HttpRuntime.Cache[shortName] as T;
            if (user == null)
            {
                var sql = string.Format(@"select * from {0} where {1} = '{2}'", tableName, filter, shortName);
                user = CPQuery.From(sql, null).ToSingle<T>();

                if (user == null)
                {
                    throw new MyException(string.Format("没有找到用户名为{0}的相关用户信息", shortName));
                }

                HttpRuntime.Cache.Insert(shortName, user, null,
                                DateTime.Now.AddMinutes(USER_EXPIRE_TIME), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return user;
        }

        /// <summary>
        /// 获取当前登录人的信息---学校端
        /// </summary>
        /// <returns></returns>
        public static UserInfo GetCurrentUserInfo()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return new UserInfo();
            }

            var identity = HttpContext.Current.User.Identity.Name;
            var userName = identity.Split('|')[0];
            var schoolID = identity.Split('|')[1];
            UserInfo user = HttpRuntime.Cache[identity] as UserInfo;
            if (user == null)
            {
                user = CPQuery.From("select * from v_user where UserName = @UserName and SchoolID= @SchoolID ", new { UserName = userName, SchoolID = schoolID }).ToSingle<UserInfo>();
                if (user == null)
                {
                   throw new Exception(string.Format("没有找到用户名为{0}的相关用户信息", userName));
                }
                HttpRuntime.Cache.Insert(identity, user, null,DateTime.Now.AddMinutes(USER_EXPIRE_TIME), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return user;
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        public static string GetCurrentUser()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return string.Empty;
            }
            return HttpContext.Current.User.Identity.Name;
        }
        /// <summary>
        /// 获取用户信息缓存过期时间
        /// </summary>
        /// <returns>用户信息缓存过期时间</returns>
        private static double GetUserExpireTime()
        {
            double expireTime = 0;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("UserExpireTime")))
                double.TryParse(ConfigurationManager.AppSettings.Get("UserExpireTime"), out expireTime);

            if (expireTime < USER_EXPIRE_TIME)
                expireTime = USER_EXPIRE_TIME;

            return expireTime;
        }

        #region 表单验证登录专用

        /// <summary>
        /// 设置用户表单登录
        /// </summary>
        /// <param name="userName">用户登录名</param>
        public static void SetUserFormLogin(string userName)
        {
            FormsAuthentication.SetAuthCookie(userName, true);
        }

        public static void SetUserFormLogin(string loginName, object userData)
        {
            Int32 expiration;

            try
            {
                AuthenticationSection authenticationSection =
                    (AuthenticationSection)WebConfigurationManager.OpenWebConfiguration("/").GetSection("system.web/authentication");
                expiration = (Int32)authenticationSection.Forms.Timeout.TotalMinutes;
            }
            catch
            {
                // 若未配置, 默认30分钟
                expiration = 30;
            }
            SetUserFormLogin(loginName, userData, expiration);
        }

        /// <summary>
        /// 设置用户表单登录，并设置cookie过期时间
        /// </summary>
        /// <param name="loginName">用户名</param>
        /// <param name="userData">用户对象</param>
        /// <param name="expiration">过期时间（分钟）</param>
        public static void SetUserFormLogin(string loginName, object userData, int expiration)
        {
            if (string.IsNullOrEmpty(loginName))
                throw new ArgumentNullException("loginName");
            if (userData == null)
                throw new ArgumentNullException("userData");

            // 1. 把需要保存的用户数据转成一个字符串。
            string data = null;
            if (userData != null)
                data = userData.ToJson();

            // 2. 创建一个FormsAuthenticationTicket，它包含登录名以及额外的用户数据。
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                2, loginName, DateTime.Now, DateTime.Now.AddMinutes(expiration), true, data);

            // 3. 加密Ticket，变成一个加密的字符串。
            string cookieValue = FormsAuthentication.Encrypt(ticket);

            // 4. 根据加密结果创建登录Cookie
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue);
            cookie.HttpOnly = true;
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Domain = FormsAuthentication.CookieDomain;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (expiration > 0)
                cookie.Expires = DateTime.Now.AddMinutes(expiration);

            HttpContext context = HttpContext.Current;
            if (context == null)
                throw new InvalidOperationException();

            // 5. 写登录Cookie
            context.Response.Cookies.Remove(cookie.Name);
            context.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 表单登出
        /// </summary>
        public static void FormLogout()
        {
            FormsAuthentication.SignOut();
        }

        #endregion
    }
}
