using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.MVC.Serializer
{
	internal static class ActionParametersProviderFactory
	{
		public static IActionParametersProvider CreateActionParametersProvider(HttpContext context)
		{
			if( context == null )
				throw new ArgumentNullException("context");


			string contentType = context.Request.ContentType;

			if( contentType.IndexOf("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) >= 0 )
				return new FormDataProvider();

			if( contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0 )
				return new JsonDataProvider();

			if( contentType.IndexOf("application/xml", StringComparison.OrdinalIgnoreCase) >= 0 )
				return new XmlDataProvider();


			// 默认还是表单的 key = vlaue格式。
			return new FormDataProvider();
		}
	}
}
