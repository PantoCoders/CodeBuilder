using Panto.Framework.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.Modules
{
    /// <summary>
    /// 程序错误处理模块
    /// </summary>
    [Obsolete("此类不支持目前的日志服务，推荐使用Panto.Framework.Logs.Modules.AppErrorModule")]
    public class AppErrorModule : IHttpModule
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="app">Http应用程序</param>
        public void Init(HttpApplication app)
        {
            app.Error += new EventHandler(app_Error);
            app.PreRequestHandlerExecute += new EventHandler(app_PreRequestHandlerExecute);
        }

        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (app.Context.IsDebuggingEnabled)
                app.Response.AppendHeader("X-AppErrorModule", "running");
        }

        void app_Error(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            Exception ex = app.Server.GetLastError();

            if (ex is HttpException)
            {
                HttpException ee = ex as HttpException;
                int httpCode = ee.GetHttpCode();
                if (httpCode == 404 || httpCode == 403)
                {
                    if (app.Request.IsLocal == false)
                    {
                        app.Server.ClearError();

                        IActionResult page = new PageResult("/Error.aspx", ex.Message);
                        page.Ouput(app.Context);
                        app.Response.End();
                    }
                    return;
                }
            }

            //记录异常日志
            WebAppHelper.SafeLogException(ex);

            // 判断是否为AJAX请求。
            // 如果是AJAX请求，我们可以不用做任何处理，
            // 因为前端已经有统一的全局处理逻辑。
            bool isAjaxRequest = string.Compare(app.Request.Headers["X-Requested-With"],
                    "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) == 0;

            if (isAjaxRequest == false)
            {
                // 是一个页面请求，此时我们可以这样处理：
                // 1. 本机请求（调试），那就出现黄页。
                // 2. 来自其他用户的访问，显示自定义的错误显示页面


                if (app.Request.IsLocal == false)
                {
                    // 不是本机请求
                    // 首先要清除异常，防止产生黄页。
                    app.Server.ClearError();

                    app.Response.StatusCode = 500;	// 继续设置500的响应，供IIS日志记录

                    // 这里，我直接显示所有的异常信息，
                    // 如果不希望这样显示，可以修改下面方法调用的第二个参数。
                    IActionResult page = new PageResult("/Error.aspx", ex.Message);
                    page.Ouput(app.Context);
                    app.Response.End();
                }
            }
            else
            {
                // 黄页显示时，页面的标题栏是根异常的描述，提示很不友好。
                // 所以放弃了黄页的显示，改成自定义的XML结构，
                // 使用XML格式的错误信息主要是为了兼容以前的前端处理代码。
                app.Server.ClearError();
                app.Response.StatusCode = 500;

                app.Response.Write(ex.Message);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
        }
    }
}
