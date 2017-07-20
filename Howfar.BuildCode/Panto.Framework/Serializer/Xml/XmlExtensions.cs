using Panto.Framework.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// 序列化助手
    /// </summary>
    public static class XmlExtensions
	{
		/// <summary>
		/// 将对象执行XML序列化
		/// </summary>
		/// <param name="obj">要序列化的对象</param>
		/// <returns>XML序列化的结果</returns>
        public static string ToXml(this object obj)
		{
			return XmlHelper.XmlSerialize(obj, Encoding.UTF8);
		}

		/// <summary>
		/// 从XML字符串中反序列化对象
		/// </summary>
		/// <typeparam name="T">反序列化的结果类型</typeparam>
        /// <param name="xml">XML字符串</param>
		/// <returns>反序列化的结果</returns>
        public static T DeserializeFromXml<T>(this string xml)
		{
			return XmlHelper.XmlDeserialize<T>(xml, Encoding.UTF8);
		}

	}
}
