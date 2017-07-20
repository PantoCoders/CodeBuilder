using Panto.Framework.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Panto.Framework.Modules
{
    /// <summary>
    /// 表单登录模块
    /// </summary>
    public class FormLoginModule : IHttpModule
    {
        /// <summary>
        /// 表单登录URL
        /// </summary>
        private const string FORM_LOGIN_URL = "/Login.aspx";

        /// <summary>
        /// 表单登录Ajax请求地址
        /// </summary>
        private const string FORM_LOGIN_AJAXURL = "Login.cspx";

        #region IHttpModule 成员

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="app">Http应用程序</param>
        public void Init(HttpApplication app)
        {
            app.PostAuthenticateRequest += new EventHandler(app_PostAuthenticateRequest);
            app.PreRequestHandlerExecute += new EventHandler(app_PreRequestHandlerExecute);
        }

        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (app.Context.IsDebuggingEnabled)
                app.Response.AppendHeader("X-FormLoginModule", "running");
        }

        void app_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            // 排除非aspx的请求
            if (!Regex.IsMatch(app.Request.Url.ToString(), ".aspx|.cspx"))
            {
                return;
            }

            // 排除登录页面和登录请求
            if (Regex.IsMatch(app.Request.Url.ToString(), GetFormLoginUrl() + "|" + GetFormLoginAjaxUrl()))
            {
                return;
            }

            if (!app.Context.Request.IsAuthenticated)
            {
                app.Response.Redirect(GetFormLoginUrl() + "?page=" + HttpUtility.UrlEncode(app.Request.Url.ToString()));
                return;
            }
        }

        #endregion

        /// <summary>
        /// 获取表单登录URL
        /// </summary>
        /// <returns></returns>
        private string GetFormLoginUrl()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("FormLoginUrl")))
                return ConfigurationManager.AppSettings.Get("FormLoginUrl");

            return FORM_LOGIN_URL;
        }

        /// <summary>
        /// 获取表单登录Ajax请求地址
        /// </summary>
        /// <returns></returns>
        private string GetFormLoginAjaxUrl()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("FormLoginAjaxUrl")))
                return ConfigurationManager.AppSettings.Get("FormLoginAjaxUrl");

            return FORM_LOGIN_AJAXURL;
        }
    }
}
