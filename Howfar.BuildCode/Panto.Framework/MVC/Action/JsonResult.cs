using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.MVC
{

	/// <summary>
	/// 一个Json对象结果
	/// </summary>
	public sealed class JsonResult : IActionResult
	{
        /// <summary>
        /// Json对象
        /// </summary>
		public object Model { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model">Json对象</param>
		public JsonResult(object model)
		{
			if( model == null )
				throw new ArgumentNullException("model");

			this.Model = model;
		}

		void IActionResult.Ouput(HttpContext context)
		{
			context.Response.ContentType = "application/json";
			string json = this.Model.ToJson();
			context.Response.Write(json);
		}

		/// <summary>
		/// 将一个对象序列化为JSON字符串
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string ObjectToJson(object data)
		{
			if( data == null )
				throw new ArgumentNullException("data");

			return data.ToJson();
		}
	}


}
