using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.Collections.Specialized;
using OptimizeReflection;


namespace Panto.Framework.MVC.Serializer
{
	internal class FormDataProvider : IActionParametersProvider
	{
		public object[] GetParameters(HttpContext context, ActionDescription action)
		{
			if( context == null )
				throw new ArgumentNullException("context");
			if( action == null )
				throw new ArgumentNullException("action");


			object[] parameters = new object[action.Parameters.Length];

			for( int i = 0; i < action.Parameters.Length; i++ ) {
				ParameterInfo p = action.Parameters[i];

				if( p.IsOut )
					continue;

				if( p.ParameterType == typeof(VoidType) )
					continue;


				if( p.ParameterType == typeof(HttpContext) ) {
					parameters[i] = context;
				}
				else if( p.ParameterType == typeof(NameValueCollection) ) {
					if( string.Compare(p.Name, "Form", StringComparison.OrdinalIgnoreCase) == 0 )
						parameters[i] = context.Request.Form;
					else if( string.Compare(p.Name, "QueryString", StringComparison.OrdinalIgnoreCase) == 0 )
						parameters[i] = context.Request.QueryString;
					else if( string.Compare(p.Name, "Headers", StringComparison.OrdinalIgnoreCase) == 0 )
						parameters[i] = context.Request.Headers;
					else if( string.Compare(p.Name, "ServerVariables", StringComparison.OrdinalIgnoreCase) == 0 )
						parameters[i] = context.Request.ServerVariables;
				}
				else {
					ContextDataAttribute[] rdAttrs = (ContextDataAttribute[])p.GetCustomAttributes(typeof(ContextDataAttribute), false);
					if( rdAttrs.Length == 1 )
						parameters[i] = EvalFromHttpContext(context, rdAttrs[0], p);
					else
						parameters[i] = GetObjectFromHttp(context, p);
				}
			}

			return parameters;
		}


		private object EvalFromHttpContext(HttpContext context, ContextDataAttribute attr, ParameterInfo p)
		{
			// 直接从HttpRequest对象中获取数据，根据Attribute中指定的表达式求值。
			string expression = attr.Expression;
			object requestData = null;

			if( expression.StartsWith("Request.") )
				requestData = System.Web.UI.DataBinder.Eval(context.Request, expression.Substring(8));

			else if( expression.StartsWith("HttpRuntime.") ) {
				PropertyInfo property = typeof(HttpRuntime).GetProperty(expression.Substring(12), BindingFlags.Static | BindingFlags.Public);
				if( property == null )
					throw new ArgumentException(string.Format("参数 {0} 对应的ContextDataAttribute计算表达式 {1} 无效：", p.Name, expression));
				requestData = property.FastGetValue(null);
			}
			else
				requestData = System.Web.UI.DataBinder.Eval(context, expression);


			if( requestData == null )
				return null;
			else {
				if( requestData.GetType().IsCompatible(p.ParameterType) )
					return requestData;
				else
					throw new ArgumentException(string.Format("参数 {0} 的申明的类型与HttpRequest对应属性的类型不一致。", p.Name));
			}
		}


		private object GetObjectFromHttp(HttpContext context, ParameterInfo p)
		{
			Type paramterType = p.ParameterType.GetRealType();

			// 如果参数是可支持的类型，则直接从HttpRequest中读取并赋值
			if( paramterType.IsSupportableType() ) {
				object val = ModelHelper.GetValueByNameAndTypeFrommRequest(context, p.Name, paramterType, null);
				if( val != null )
					return val;
				else {
					if( p.ParameterType.IsValueType && p.ParameterType.IsNullableType() == false )
						throw new ArgumentException("未能找到指定的参数值：" + p.Name);
					else
						return null;
				}
			}
			else {
				// 自定义的类型。首先创建实例，然后给所有成员赋值。
				// 注意：这里不支持嵌套类型的自定义类型。
				//object item = Activator.CreateInstance(paramterType);
				object item = paramterType.FastNew();
				ModelHelper.FillModel(context, item, p.Name);
				return item;
			}
		}


	}
}
