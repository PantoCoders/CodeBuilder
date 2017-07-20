using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Panto.Framework
{
	/// <summary>
	/// 常用字符串扩展
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// 将字符串转换为 HTML 编码的字符串。
		/// </summary>
		/// <param name="str">要编码的字符串。</param>
		/// <returns>一个已编码的字符串。</returns>
		public static string HtmlEncode(this string str)
		{
			if( string.IsNullOrEmpty(str) )
				return string.Empty;

			return HttpUtility.HtmlEncode(str);
		}

		/// <summary>
		/// 将字符串最小限度地转换为 HTML 编码的字符串。
		/// </summary>
		/// <param name="str">要编码的字符串。</param>
		/// <returns>一个已编码的字符串。</returns>
		public static string HtmlAttributeEncode(this string str)
		{
			if( string.IsNullOrEmpty(str) )
				return string.Empty;

			return HttpUtility.HtmlAttributeEncode(str);
		}

		/// <summary>
		/// 判断两个字符串是否相等，忽略大小写的比较方式。
		/// </summary>
		/// <param name="a">被比较字符串</param>
        /// <param name="b">比较字符串</param>
		/// <returns>返回true表示相等，返回false表示不相等。</returns>
		public static bool IsSame(this string a, string b)
		{
			return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// 等效于 string.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries)
		/// 且为每个拆分后的结果又做了Trim()操作。
		/// </summary>
        /// <param name="str">要拆分的字符串</param>
		/// <param name="separator">分隔符</param>
		/// <returns>拆分后的字符串数组</returns>
		public static string[] SplitTrim(this string str, params char[] separator)
		{
			if( string.IsNullOrEmpty(str) )
				return null;
			else
				return (from s in str.Split(separator)
						let u = s.Trim()
						where u.Length > 0
						select u).ToArray();
		}

		internal static readonly char[] CommaSeparatorArray = new char[] { ',' };

		/// <summary>
		/// 将字符串的首个英文字母大写
		/// </summary>
		/// <param name="text">字符串</param>
		/// <returns>输出字符串</returns>
		public static string ToTitleCase(this string text)
		{
			// 重新实现：CultureInfo.CurrentCulture.TextInfo.ToTitleCase
			// 那个方法太复杂了，重新实现一个简单的版本。

			if( text == null || text.Length < 2 )
				return text;

			char c = text[0];
			if( (c >= 'a') && (c <= 'z') )
				return ((char)(c - 32)).ToString() + text.Substring(1);
			else
				return text;
		}

        /// <summary>
        /// 清理字符串空格
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>输出字符串</returns>
        public static string TrimAll(this string input)
        {
            return input.Trim().Replace("\r\t", "").Replace("\r\n", "").Replace(" ", "");
        }

        /// <summary>
        /// 清理字符串
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>输出字符串</returns>
        public static string Clear(this string input)
        {
            return input.Clear(string.Empty);
        }

        /// <summary>
        /// 清理字符串
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="removeText">清理的字符，多个字符用“|”号分隔</param>
        /// <returns>输出字符串</returns>
        public static string Clear(this string input, string removeText)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string output = input.Trim();
            output = output.Replace("<br>", "\r\n");
            output = output.Replace("<br />", "\r\n");
            output = output.Replace("<br/>", "\r\n");
            output = output.Replace("&nbsp;", " ");
            output = output.Replace("&amp;", "&");

            if (!string.IsNullOrEmpty(removeText))
            {
                string[] texts = removeText.Split('|');

                if (texts.Length > 0)
                {
                    foreach (string txt in texts)
                    {
                        output = output.Replace(txt, "");
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// 获取字符串长度(1个汉字算2个字符)
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>长度</returns>
        public static int GetRealLength(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            ASCIIEncoding ascii = new ASCIIEncoding();
            int tempLen = 0; byte[] s = ascii.GetBytes(str);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
            }

            return tempLen;
        }

        /// <summary>
        /// 判断字符串是否为数字
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>返回true表示是，返回false表示否。</returns>
        public static bool IsNumeric(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            if (!Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                return false;

            return true;
        }



        /// <summary>
        /// 移除HTML代码，保留字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string StripHTML(this string source)
        {

            try {
                string result;
                result = source.Replace("\r", " ");
                result = result.Replace("\n", " ");
                result = result.Replace("'", " ");
                result = result.Replace("\t", string.Empty);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"( )+", " ");
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*head([^>])*>", "<head>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"(<( )*(/)( )*head( )*>)", "</head>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(<head>).*(</head>)", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*script([^>])*>", "<script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"(<( )*(/)( )*script( )*>)", "</script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"(<script>).*(</script>)", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*style([^>])*>", "<style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"(<( )*(/)( )*style( )*>)", "</style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(<style>).*(</style>)", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*td([^>])*>", "\t", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*br( )*>", "\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*li( )*>", "\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*div([^>])*>", "\r\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*tr([^>])*>", "\r\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*p([^>])*>", "\r\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<[^>]*>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&nbsp;", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&bull;", " * ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&lsaquo;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&rsaquo;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&trade;", "(tm)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&frasl;", "/", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @">", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&copy;", "(c)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&reg;", "(r)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"&(.{2,6});", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = result.Replace("\n", "\r");
                result = System.Text.RegularExpressions.Regex.Replace(result, "(\r)( )+(\r)", "\r\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(\t)( )+(\t)", "\t\t", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(\t)( )+(\r)", "\t\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(\r)( )+(\t)", "\r\t", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(\r)(\t)+(\r)", "\r\r", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(\r)(\t)+", "\r\t", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                string breaks = "\r\r\r";
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++) {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }
                return result;
            }
            catch {
                //MessageBox.Show("Error");
                return source;
            }
        }
	}
}
