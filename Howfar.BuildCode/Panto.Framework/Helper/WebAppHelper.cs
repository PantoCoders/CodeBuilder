using Panto.Framework.Exceptions;
using Panto.Framework.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework
{
    /// <summary>
    /// 网页程序助手
    /// </summary>
    public class WebAppHelper
    {
        /// <summary>
        /// 项目根目录
        /// </summary>
        private static readonly string s_root = HttpContextHelper.AppRootPath.TrimEnd('\\');

        /// <summary>
        /// 安全地记录一个异常对象到文本文件。
        /// </summary>
        /// <param name="ex">异常对象</param>
        public static void SafeLogException(Exception ex)
        {
            if (ex is HttpException)
            {
                HttpException ee = ex as HttpException;
                if (ee.GetHttpCode() == 404)
                    return;
            }

            try
            {
                string logfileDir = s_root + ("/App_Data/Logs/" + DateTime.Now.ToString("yyyy-MM-dd") + "/").Replace("/", "\\");

                if (!Directory.Exists(logfileDir))
                    Directory.CreateDirectory(logfileDir);

                string message = ex.ToString() + "\r\n\r\n\r\n";
                if (HttpContext.Current != null)
                    message = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Url: " + HttpContext.Current.Request.RawUrl + "\r\n" + message;

                File.AppendAllText(logfileDir + DateTime.Now.ToString("yyyy-MM-dd HH") + "时.log", message, System.Text.Encoding.UTF8);
            }
            catch { }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="dbName">数据库名</param>
        public static void Init(string dbName)
        {
            //zhangjm注释
            //if (string.IsNullOrEmpty(dbName))
            //{
            //    throw new ArgumentNullException("dbName");
            //}
            //初始化连接字符串
            //ConnStringHelper.Initializer();

            //注册连接字符串（CPQuery需要）
           // Panto.Map.Extensions.Initializer.UnSafeInit(ConnStringHelper.GetConnString(dbName));
            Panto.Map.Extensions.Initializer.UnSafeInit(ConnStringHelper.GetConnectString(dbName));

            //加载XmlCommand
            //var path = s_root + "/App_Data/XmlCommand/".Replace("/", "\\");
            //if (Directory.Exists(path))
            //{
            //    XmlCommandManager.LoadCommands(path);
            //}
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">路径</param>
        public static void DownloadFile(string fileName, string filePath)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            // 如果文件存在，则按照文件后缀名相关输出方式输出
            if (!File.Exists(filePath))
            {
                // 获取相对路径
                filePath =HttpContext.Current.Request.PhysicalApplicationPath+ filePath;
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("要下载的文件不存在");
            }

            var context = HttpContext.Current;

            // 一次读10K数据
            byte[] buffer = new Byte[10000];

            // 文件的大小
            int length;

            // 总共需要读取的数据长度
            long dataToRead;

            using (Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // 需要读取的数据长度
                dataToRead = iStream.Length;

                context.Response.ContentType = "application/octet-stream";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName));//System.Text.UTF8Encoding.UTF8.GetBytes(FileName)

                // 读取数据
                while (dataToRead > 0)
                {
                    // 验证是不是客户端进行连接
                    if (context.Response.IsClientConnected)
                    {
                        // 读取文件流
                        length = iStream.Read(buffer, 0, 10000);

                        // 输出到页面
                        context.Response.OutputStream.Write(buffer, 0, length);
                        context.Response.Flush();

                        buffer = new Byte[10000];
                        dataToRead = dataToRead - length;
                    }
                    else
                    {
                        //防止用户断开后无限循环
                        dataToRead = -1;
                    }
                }
            }
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns>客户端IP地址</returns>
        public static string GetClientIP()
        {
            string sMyIP = null;
            HttpRequest request = HttpContext.Current.Request;
            //如果客户端没有使用代理，则直接获取远程地址
            if (request.ServerVariables["HTTP_X_FORWARDED_FOR"] == null)
            {
                sMyIP = request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                sMyIP = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            //如果获取的IP为空，则直接从request中获取用户IP地址
            if (string.IsNullOrEmpty(sMyIP))
            {
                sMyIP = request.UserHostAddress;
            }

            return sMyIP;
        }

        #region 资源文件

        /// <summary>
        /// 引用JS文件
        /// </summary>
        /// <param name="path">路径地址</param>
        /// <returns>引用JS的Html片段</returns>
        public static string RefJSFile(string path)
        {
            string filePath = s_root + path.Replace("/", "\\");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            string version = File.GetLastWriteTimeUtc(filePath).Ticks.ToString();
            return string.Format("<script src=\"{0}?v={1}\" type=\"text/javascript\"></script>{2}", path, version, Environment.NewLine);
        }

        /// <summary>
        /// 引用CSS文件
        /// </summary>
        /// <param name="path">路径地址</param>
        /// <returns>引用CSS的Html片段</returns>
        public static string RefCSSFile(string path)
        {
            string filePath = s_root + path.Replace("/", "\\");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            string version = File.GetLastWriteTimeUtc(filePath).Ticks.ToString();
            return string.Format("<link href=\"{0}?v={1}\" rel=\"stylesheet\" type=\"text/css\" />{2}", path, version, Environment.NewLine);
        }

        /// <summary>
        /// 引用Html文件
        /// </summary>
        /// <param name="path">路径地址</param>
        /// <returns>引用Html文件路径</returns>
        public static string RefHtmlFile(string path)
        {
            string filePath = s_root + path.Replace("/", "\\");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            string version = File.GetLastWriteTimeUtc(filePath).Ticks.ToString();
            return string.Format("{0}?v={1}", path, version);
        }

        #endregion
    }
}
