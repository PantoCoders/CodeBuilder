using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;
namespace Panto.Framework
{
    public class AppSettingsHelper
    {
        private static readonly object objLock = new object();

        private static Dictionary<string, string> cacheAppSettings = new Dictionary<string, string>();
        private static Dictionary<string, string> cacheDBAppSettings = new Dictionary<string, string>();
        private static Dictionary<string, AppSettings> cacheDBInfoAppSettings = new Dictionary<string, AppSettings>();



        /// <summary>
        /// 获取web.config中的AppSettings值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            string value = string.Empty;
            if (!cacheAppSettings.TryGetValue(key, out value))
            {
                lock (objLock)
                {
                    if (!cacheAppSettings.TryGetValue(key, out value))
                    {
                        value = System.Configuration.ConfigurationManager.AppSettings[key].ToString();
                        cacheAppSettings.Add(key, value);
                    }
                }
            }
            return value;
        }







        private const string _sqlGetKey = "SELECT [KeyValue] FROM {0}.[dbo].[AppSettings] where KeyName = @KeyName and GroupName is null";
        private const string _sqlGetInfoKey = "SELECT * FROM {0}.[dbo].[AppSettings] where KeyName = @KeyName and GroupName is null";

        /// <summary>
        /// 获取SystemCore库的 公共配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DBGet(string key)
        {
            string result = string.Empty;
            if (!cacheDBAppSettings.TryGetValue(key, out result))
            {
                lock (objLock)
                {
                    if (!cacheDBAppSettings.TryGetValue(key, out result))
                    {
                        result = CPQuery.From(string.Format(_sqlGetKey, ConnStringHelper.GetDBName("SystemCore")), new { KeyName = key }).ExecuteScalar<string>();
                        if (!string.IsNullOrEmpty(result))
                        {
                            cacheDBAppSettings.Add(key, result);
                        }
                    }
                }
            }
            return result;
        }





        /// <summary>
        /// 获取SystemCore库的 公共配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static AppSettings DBGetInfo(string key)
        {
            AppSettings result = null;
            if (!cacheDBInfoAppSettings.TryGetValue(key, out result))
            {
                lock (objLock)
                {
                    if (!cacheDBInfoAppSettings.TryGetValue(key, out result))
                    {
                        result = CPQuery.From(string.Format(_sqlGetInfoKey, ConnStringHelper.GetDBName("SystemCore")), new { KeyName = key }).ToSingle<AppSettings>();
                        if (result != null)
                        {
                            cacheDBInfoAppSettings.Add(key, result);
                        }
                    }
                }
            }
            return result;
        }
    }


    public class AppSettings
    {
        /// <summary>
        /// 唯一编号
        /// </summary>
        public Guid AppSettingsID { get; set; }
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 键名称
        /// </summary>
        public string KeyName { get; set; }
        /// <summary>
        /// 键值
        /// </summary>
        public string KeyValue { get; set; }
        /// <summary>
        /// 配置说明
        /// </summary>
        public string Remarks { get; set; }
        /// <summary>
        /// 备用字段
        /// </summary>
        public string KeyBak { get; set; }
    }
}
