using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Reflection;
using System.Xml.Serialization;
using OptimizeReflection;
using System.IO;

namespace Panto.Framework.MVC.Serializer
{
	internal class XmlDataProvider : IActionParametersProvider
	{
		public object[] GetParameters(HttpContext context, ActionDescription action)
		{
			if( action.Parameters.Length == 1 ) {
				object value = GetObjectFromRequest(context.Request, action);
				return new object[1] { value };
			}
			else
				return GetMultiObjectsFormRequest(context.Request, action);
		}


		public object GetObjectFromRequest(HttpRequest request, ActionDescription action)
		{
			if( action.Parameters[0].ParameterType == typeof(string) )
				return request.ReadInputStream();


			if( action.Parameters[0].ParameterType == typeof(XmlDocument) ) {
				string xml = request.ReadInputStream();
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				return doc;
			}

			Type destType = action.Parameters[0].ParameterType.GetRealType();

			XmlSerializer mySerializer = new XmlSerializer(destType);

			request.InputStream.Position = 0;
			StreamReader sr = new StreamReader(request.InputStream, request.ContentEncoding);
			return mySerializer.Deserialize(sr);
		}

		public object[] GetMultiObjectsFormRequest(HttpRequest request, ActionDescription action)
		{
			string xml = request.ReadInputStream();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			XmlNode root = doc.LastChild;

			//if( root.ChildNodes.Count != action.Parameters.Length )
			//    throw new ArgumentException("客户端提交的数据项与服务端的参数项的数量不匹配。");

			object[] parameters = new object[action.Parameters.Length];

			for( int i = 0; i < parameters.Length; i++ ) {
				string name = action.Parameters[i].Name;
				XmlNode node = (from n in root.ChildNodes.Cast<XmlNode>()
								where string.Compare(n.Name, name, StringComparison.OrdinalIgnoreCase) == 0
								select n).FirstOrDefault();

				try {
					if( node != null ) {
						object parameter = null;
						Type destType = action.Parameters[i].ParameterType.GetRealType();

						if( destType.IsSupportableType() )
							parameter = ModelHelper.UnSafeChangeType(node.InnerText, destType);
						else
							parameter = XmlDeserialize(node.OuterXml, destType, request.ContentEncoding);

						parameters[i] = parameter;
					}
				}
				catch( Exception ex ) {
					throw new InvalidCastException("数据转换失败，当前参数名：" + name, ex);
				}
			}

			return parameters;
		}



		public static object XmlDeserialize(string xml, Type destType, Encoding encoding)
		{
			if( string.IsNullOrEmpty(xml) )
				throw new ArgumentNullException("xml");
			if( encoding == null )
				throw new ArgumentNullException("encoding");

			XmlSerializer mySerializer = new XmlSerializer(destType);
			using( MemoryStream ms = new MemoryStream(encoding.GetBytes(xml)) ) {
				using( StreamReader sr = new StreamReader(ms, encoding) ) {
					return mySerializer.Deserialize(sr);
				}
			}
		}

	}
}
