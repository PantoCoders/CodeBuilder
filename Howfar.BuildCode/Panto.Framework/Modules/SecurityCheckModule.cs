using Panto.Framework.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace Panto.Framework.Modules
{
    /// <summary>
    /// 安全检查模块
    /// </summary>
    public class SecurityCheckModule : IHttpModule
    {
        private static readonly double s_GetFrequencySecond = GetConfigValue("SecurityCheckModule-second-GET", 0.3, 1000);
        private static readonly double s_PostFrequencySecond = GetConfigValue("SecurityCheckModule-second-POST", 3, 1000);

        private static readonly Hashtable s_checkTable = new Hashtable(1024, StringComparer.Ordinal);
        private static readonly object s_lock = new object();

        /// <summary>
        /// 用户检查信息
        /// </summary>
        private class UserCheckInfo
        {
            /// <summary>
            /// 用户名
            /// </summary>
            public string UserName;
            /// <summary>
            /// 上次Get提交
            /// </summary>
            public long LastGet;
            /// <summary>
            /// 上次Post请求
            /// </summary>
            public long LastPost;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="app">Http应用程序</param>
        public void Init(HttpApplication app)
        {
            app.PostResolveRequestCache += new EventHandler(app_PostResolveRequestCache);
        }

        void app_PostResolveRequestCache(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            //本地调试（非IIS）不走验证
            if (app.Context.Request.IsLocal && !app.Context.Request.IsAuthenticated)
                return;

            if (app.Context.IsDebuggingEnabled)
                app.Response.AppendHeader("X-SecurityCheckModule", "running");

            if (app.Request.RequestType == "POST")
            {
                if (app.Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                {
                    app.Response.Write("无效的提交请求-1。");
                    app.Response.End();
                }
            }

            if (app.Request.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) == false)
                return;


            bool endRequest = false;

            UserCheckInfo info = GetCheckInfo(app.User.Identity.Name);

            // 为了防止工具程序以并发方式发起多次请求
            long currentTime = DateTime.Now.Ticks;

            if (app.Request.RequestType == "GET")
            {
                long lastTime = Interlocked.Exchange(ref info.LastGet, currentTime);

                if (new DateTime(lastTime).AddSeconds(s_GetFrequencySecond) > new DateTime(currentTime))
                {
                    app.Response.Write("您的请求频率太快，休息，休息一会儿！");
                    endRequest = true;
                }
            }

            else
            {
                long lastTime = Interlocked.Exchange(ref info.LastPost, currentTime);

                if (new DateTime(lastTime).AddSeconds(s_PostFrequencySecond) > new DateTime(currentTime))
                {
                    throw new InvalidRequestException("您的请求频率太快，休息，休息一会儿！");
                }
            }

            if (endRequest)
                app.Response.End();
        }

        /// <summary>
        /// 获取用户检查信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户检查信息</returns>
        private UserCheckInfo GetCheckInfo(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new InvalidRequestException();

            UserCheckInfo info = s_checkTable[username] as UserCheckInfo;
            if (info == null)
            {

                // 为了防止工具程序以并发方式发起多次请求，因此这里采用全局锁，
                // 确保一个用户只生成一个UserCheckInfo实例。
                lock (s_lock)
                {
                    info = s_checkTable[username] as UserCheckInfo;

                    if (info == null)
                    {
                        info = new UserCheckInfo
                        {
                            UserName = username,
                            LastGet = DateTime.MinValue.Ticks,
                            LastPost = DateTime.MinValue.Ticks
                        };
                        s_checkTable[username] = info;
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// 获取配置的值
        /// 注：如果配置的值超过最大值和最小值的范围，则返回最小值
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>配置的值</returns>
        private static double GetConfigValue(string name, double min, double max)
        {
            string configValue = ConfigurationManager.AppSettings[name];

            double number = 0d;
            double.TryParse(configValue, out number);

            if (number >= min && number <= max)
                return number;
            else
                return min;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {

        }
    }
}
