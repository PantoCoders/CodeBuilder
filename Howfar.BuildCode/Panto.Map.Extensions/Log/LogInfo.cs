using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Panto.Map.Extensions.Xml;

namespace Panto.Map.Extensions.Log
{
	/// <summary>
	/// 日志对象信息
	/// </summary>
	public class LogInfo
	{
		/// <summary>
		/// 调用LogHelper.LogException的时刻
		/// </summary>
		[XmlElement(Order = 1)]
		public DateTime Time { get; set; }

		/// <summary>
		/// 浏览器类型。注意：此信息可能不准确。
		/// </summary>
		[XmlElement(Order = 2)]
		public string Browser { get; set; }

		/// <summary>
		/// 当前用户的登录名。注意：这里取的是  HttpContext.User.Identity.Name
		/// </summary>
		[XmlElement(Order = 3)]
		public string UserName { get; set; }

		/// <summary>
		/// 当前请求的URL
		/// </summary>
		[XmlElement(Order = 4)]
		public string Url { get; set; }


		/// <summary>
		/// 当前请求的访问方法（POST, GET）
		/// </summary>
		[XmlElement(Order = 5)]
		public string RequestType { get; set; }


		/// <summary>
		/// 当前请求的内容编码
		/// </summary>
		[XmlElement(Order = 6)]
		public string ContentEncoding { get; set; }



		/// <summary>
		/// 当前请求的UrlReferrer地址
		/// </summary>
		[XmlElement(Order = 7)]
		public string UrlReferrer { get; set; }

		/// <summary>
		/// 用户提交的数据，如果有文件上传时，仅保留非文件部分
		/// </summary>
		[XmlElement(Order = 8)]
		public MyCDATA PostData { get; set; }

		/// <summary>
		/// 当前请求的Cookie信息（上传部分）
		/// </summary>
		[XmlArray(Order = 9)]
		[XmlArrayItem("Cookie")]
		public List<NameValue> Cookie = new List<NameValue>();

		/// <summary>
		/// 当前请求的Session信息
		/// </summary>
		[XmlArray(Order = 10)]
		[XmlArrayItem("Session")]
		public List<NameValue> Session = new List<NameValue>();

		/// <summary>
		/// 异常类型
		/// </summary>
		[XmlElement(Order = 11)]
		public string ExceptionType { get; set; }

		/// <summary>
		/// 异常信息
		/// </summary>
		[XmlElement(Order = 12)]
		public MyCDATA Exception { get; set; }

		/// <summary>
		/// 额外的消息。
		/// </summary>
		[XmlElement(Order = 13)]
		public MyCDATA Message { get; set; }

	}


	/// <summary>
	/// Name / Value 值对
	/// </summary>
	public sealed class NameValue
	{
		/// <summary>
		/// Name 值
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; }

		/// <summary>
		/// Value 值
		/// </summary>
		[XmlAttribute]
		public string Value { get; set; }
	}

}
