using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using OptimizeReflection;
using Newtonsoft.Json;


namespace Panto.Framework.MVC.Serializer
{
	internal class JsonDataProvider : IActionParametersProvider
	{
        internal static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            //DateFormatString = "yyyy-MM-dd HH:mm:ss"
        };

		public object[] GetParameters(HttpContext context, ActionDescription action)
		{
			string input = context.Request.ReadInputStream();
			
			if( action.Parameters.Length == 1 ) {
				object value = GetObjectFromString(input, action);
				return new object[1] { value };
			}
			else
				return GetMultiObjectsFormString(input, action);
		}


		public object GetObjectFromString(string input, ActionDescription action)
		{
			if( action.Parameters[0].ParameterType == typeof(string) )
				return input;

			Type destType = action.Parameters[0].ParameterType.GetRealType();

            return Newtonsoft.Json.JsonConvert.DeserializeObject(input, destType, settings);
		}

		public object[] GetMultiObjectsFormString(string input, ActionDescription action)
		{

            Dictionary<string, object> dict = JsonConvert.DeserializeObject(input) as Dictionary<string, object>;

			//if( dict.Count != action.Parameters.Length )
			//    throw new ArgumentException("客户端提交的数据项与服务端的参数项的数量不匹配。");

			object[] parameters = new object[action.Parameters.Length];

			for( int i = 0; i < parameters.Length; i++ ) {
				string name = action.Parameters[i].Name;
				object value = (from kv in dict
								where string.Compare(kv.Key, name, StringComparison.OrdinalIgnoreCase) == 0
								select kv.Value).FirstOrDefault();

				try {
					if( value != null ) {
						Type destType = action.Parameters[i].ParameterType.GetRealType();

                        object parameter = JsonConvert.DeserializeObject(value.ToString(), destType, settings);
						parameters[i] = parameter;
					}
				}
				catch( Exception ex ) {
					throw new InvalidCastException("数据转换失败，当前参数名：" + name, ex);
				}
			}

			return parameters;
		}



	}
}
