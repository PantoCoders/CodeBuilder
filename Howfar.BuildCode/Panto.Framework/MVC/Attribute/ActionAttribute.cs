using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework.MVC
{
	/// <summary>
	/// 定义针对请求的安全检查模式
	/// </summary>
	public enum ValidateRequestMode
	{
		/// <summary>
		/// 从web.config中继承设置
		/// </summary>
		Inherits,
		/// <summary>
		/// 打开安全检查
		/// </summary>
		Enable,
		/// <summary>
		/// 关闭安全检查
		/// </summary>
		Disable
	}


	/// <summary>
	/// 将一个方法标记为一个Action
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class ActionAttribute : Attribute
	{
		/// <summary>
		/// 允许哪些访问动词，与web.config中的httpHanlder的配置意义一致。
		/// </summary>
		public string Verb { get; set; }

		/// <summary>
		/// 确定 ASP.NET 是否针对危险值检查来自浏览器的输入。
		/// </summary>
		public ValidateRequestMode ValidateRequest { get; set; }


		internal bool AllowExecute(string httpMethod)
		{
			if( string.IsNullOrEmpty(Verb) || Verb == "*" ) {
				return true;
			}
			else {
				string[] verbArray = Verb.SplitTrim(StringExtensions.CommaSeparatorArray);

				return verbArray.Contains(httpMethod, StringComparer.OrdinalIgnoreCase);
			}
		}

		internal bool NeedValidateRequest()
		{
			if( this.ValidateRequest == ValidateRequestMode.Enable )
				return true;

			if( this.ValidateRequest == ValidateRequestMode.Disable )
				return false;

			return WebConfig.ValidateRequest;
		}
	}


	
}
