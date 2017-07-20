using Panto.Framework.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// INI配置助手
    /// </summary>
    public class INIHelper
    {
        #region 读配置
        /// <summary>
        /// 读取配置文件(string值)
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <returns>string值</returns>
        public static string Read(string path, string section, string key)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!File.Exists(path))
                throw new MyException(string.Format("{0}文件不存在", path));

            StringBuilder temp = new StringBuilder(255);
            DllHelper.GetPrivateProfileString(section, key, "", temp, 255, path);

            return temp.ToString();
        }

        /// <summary>
        /// 读取配置文件(int值)
        /// </summary>
        /// <param name="strPath">文件地址</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <returns>int值</returns>
        public static int ReadInt(string strPath, string section, string key)
        {
            string value = Read(strPath, section, key);

            int val;

            int.TryParse(value, out val);

            return val;
        }

        /// <summary>
        /// 读取配置文件(DateTime值)
        /// </summary>
        /// <param name="strPath">文件地址</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <returns>DateTime值</returns>
        public static DateTime ReadDateTime(string strPath, string section, string key)
        {
            string value = Read(strPath, section, key);

            DateTime dt;
            DateTime.TryParse(value, out dt);

            return dt;
        }

        #endregion

        #region 写配置

        /// <summary>
        /// 将string值写入配置文件
        /// </summary>
        /// <param name="strPath">文件地址</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Write(string strPath, string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            if (!File.Exists(strPath))
                File.Create(strPath);

            DllHelper.WritePrivateProfileString(section, key, value, strPath);
        }

        /// <summary>
        /// 将int值写入配置文件
        /// </summary>
        /// <param name="strPath">文件地址</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Write(string strPath, string section, string key, int value)
        {
            Write(strPath, section, key, value.ToString());
        }

        /// <summary>
        /// 将Datetime值写入配置文件
        /// </summary>
        /// <param name="strPath">文件地址</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Write(string strPath, string section, string key, DateTime value)
        {
            Write(strPath, section, key, value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        #endregion
    }
}
