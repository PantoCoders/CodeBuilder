using System;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using Panto.Framework.MVC;
using Panto.Framework.Exceptions;

namespace Panto.Framework
{
    /// <summary>
    /// 配置文件助手
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// 项目根目录
        /// </summary>
        private static readonly string s_root = HttpContextHelper.AppRootPath.TrimEnd('\\');// HttpRuntime.AppDomainAppPath.TrimEnd('\\');

        /// <summary>
        /// 获取配置文件信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>配置文件信息</returns>
        public static Configuration GetConfigFile(string filePath)
        {
            string configPath = s_root + filePath.Replace("/", "\\");    //HttpContext.Current.Server.MapPath(filePath);

            if (!File.Exists(configPath))
            {
                throw new MyException("请检查配置文件是否存在,地址:" + configPath);
            }

            ExeConfigurationFileMap file = new ExeConfigurationFileMap() 
            { 
                ExeConfigFilename = configPath 
            };
            return ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);
        }
    }
}
