using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// 常用扩展方法
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// 将DataTable转换成Dictionary
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<int, string> ToDictionary(this DataTable dt, string defaultValue)
        {
            if (dt == null)
                return null;

            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(defaultValue))
                dict.Add(-1, defaultValue);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dict.Add(Convert.ToInt32(dr["value"].ToString()), dr["text"].ToString());
                }
            }

            return dict;
        }

        /// <summary>
        /// 可空时间转换为字符串
        /// (如果为null，则返回空字符串)
        /// </summary>
        /// <param name="time">可空时间</param>
        /// <param name="format">格式化字符串</param>
        /// <returns>时间字符串</returns>
        public static string ToString(this DateTime? time, string format)
        {
            return time.HasValue ? time.Value.ToString(format) : "";
        }

        /// <summary>
        /// 在指定的字符串集合的每个元素之间串联指定的分隔符，从而产生单个串联的字符串
        /// </summary>
        /// <param name="list">字符串集合</param>
        /// <param name="separator">分隔符</param>
        /// <returns>以分隔符串联的字符串</returns>
        public static string Join(this IEnumerable<string> list, string separator)
        {
            if (list == null || list.Count() == 0)
            {
                return string.Empty;
            }

            return string.Join(separator, list.ToArray());
        }
    }
}
