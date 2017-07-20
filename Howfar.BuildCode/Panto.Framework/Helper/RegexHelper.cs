using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Panto.Framework
{
    /// <summary>
    /// 正则表达式助手
    /// </summary>
    public class RegexHelper
    {
        /// <summary>
        /// 获取第一个匹配的值
        /// </summary>
        /// <param name="body">内容</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns>第一个匹配的值</returns>
        public static string GetRegexValue(string body, string pattern)
        {
            var list = GetRegexValues(body, pattern);

            if (list.Count == 0)
                return string.Empty;

            return list[0];
        }

        /// <summary>
        /// 获取匹配值
        /// </summary>
        /// <param name="body">内容</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns>匹配到的值列表</returns>
        public static List<string> GetRegexValues(string body, string pattern)
        {
            List<string> values = new List<string>();

            if (string.IsNullOrEmpty(body) || string.IsNullOrEmpty(pattern))
                return values;

            MatchCollection matches = Regex.Matches(body, pattern);
            for (int i = 0; i < matches.Count; i++)
            {
                values.Add(matches[i].ToString().Trim());
            }

            return values;
        }

        /// <summary>
        /// 获取Email后缀地址
        /// </summary>
        /// <param name="strEmail">内容</param>
        /// <returns>Email后缀地址(全小写)</returns>
        public static string GetEmailEndAddress(string strEmail)
        {
            string email = string.Empty;

            if (string.IsNullOrEmpty(strEmail))
                return email;

            Match match = Regex.Match(strEmail, @"@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            if (match != null)
                email = match.ToString().ToLower();

            return email;
        }

        /// <summary>
        /// 获取域名
        /// </summary>
        /// <param name="url">网址</param>
        /// <returns>域名(全小写)</returns>
        public static string GetDomain(string url)
        {
            string domain = string.Empty;

            if (string.IsNullOrEmpty(url))
                return domain;

            Match match = Regex.Match(url, "(?<=http://).[^/]*");
            if (match != null)
                domain = match.ToString().ToLower();

            return domain;
        }

        /// <summary>
        /// 移除HTML标签   
        /// </summary>   
        /// <param name="HTMLStr">Html内容</param>   
        /// <returns></returns>
        public static string ParseTags(string HTMLStr)
        {
            if (string.IsNullOrEmpty(HTMLStr)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(HTMLStr, "<[^>]*>", " ");
        }

        /// <summary>
        /// 移除HTML标签   
        /// </summary>   
        /// <param name="HTMLStr">Html内容</param>   
        /// <returns></returns>
        public static string ParseTagsAndTrim(string HTMLStr)
        {
            if (string.IsNullOrEmpty(HTMLStr)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(HTMLStr, "<[^>]*>", "");
        }

        /// <summary>
        /// 得到HTML标题
        /// </summary>
        /// <param name="content">Html内容</param>
        /// <returns></returns>
        public static string GetTitleValue(string content)
        {
            var list = RegexHelper.GetRegexValues(content, @"<title>[\s\S]*?</title>");
            if (list.Count > 0)
            {
                //去掉前后替换标签产生的空格
                return ParseTags(list[0]).Trim();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
