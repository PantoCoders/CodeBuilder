using Microsoft.Win32;
using Panto.Framework.MVC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.Handlers
{
    /// <summary>
    /// 文件分享处理器（适用于多个网站共享附件的情况）
    /// </summary>
    public class ShareFileHandler : IHttpHandler
    {
        // 每种扩展名对应诉Mime类型对照表
        private static readonly Hashtable s_mineTable = Hashtable.Synchronized(new Hashtable(10, StringComparer.OrdinalIgnoreCase));

        public void ProcessRequest(HttpContext context)
        {
            // 获取文件物理路径
            string filePath = context.Request.PhysicalPath;

            // 如果文件存在，则按照文件后缀名相关输出方式输出
            if (!File.Exists(filePath))
            {
                // 获取相对路径
                filePath = filePath.Replace(context.Request.PhysicalApplicationPath, ConfigurationManager.AppSettings.Get("ExtFilePath"));
            }

            if (!File.Exists(filePath))
            {
                new Http404Result().Ouput(context);
                return;
            }

            var file = new FileInfo(filePath);
            // 设置响应内容标头
            string contentType = (string)s_mineTable[file.Extension];
            if (contentType == null)
            {
                contentType = GetMimeType(file);
                s_mineTable[file.Extension] = contentType;
            }
            context.Response.ContentType = contentType;

            context.Response.TransmitFile(filePath);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// 获取文件Mime类型
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns></returns>
        private string GetMimeType(FileInfo file)
        {
            string mimeType = "application/octet-stream";
            if (string.IsNullOrEmpty(file.Extension))
                return mimeType;

            using (RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(file.Extension.ToLower()))
            {
                if (regKey != null)
                {
                    object regValue = regKey.GetValue("Content Type");
                    if (regValue != null)
                        mimeType = regValue.ToString();
                }
            }
            return mimeType;
        }
    }
}
